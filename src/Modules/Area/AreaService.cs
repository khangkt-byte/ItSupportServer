using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.Area
{
    public class AreaService : IAreaService
    {
        private readonly AppDbContext _db;
        private readonly AreaMapper _mapper;
        private readonly ILogger<AreaService> _logger;
        private readonly IValidator<CreateAreaDto> _createValidator;
        private readonly IValidator<UpdateAreaDto> _updateValidator;

        public AreaService(
            AppDbContext db,
            AreaMapper mapper,
            ILogger<AreaService> logger,
            IValidator<CreateAreaDto> createValidator,
            IValidator<UpdateAreaDto> updateValidator)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PaginatedResult<AreaDto>> GetAreasAsync(QueryParameters parameters)
        {
            _logger.LogInformation("Fetching areas with search: {Search}, page: {Page}", 
                parameters.Search, parameters.Page);

            var query = _mapper.ProjectToAreaDto(_db.Areas
                .Where(a => a.DeletedAt == null)
                .AsNoTracking());

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(a =>
                    a.Name.Contains(parameters.Search) ||
                    (a.Description != null && a.Description.Contains(parameters.Search)));
            }

            // Use extension method for pagination
            var result = await query.ToPaginatedResultAsync(
                parameters,
                defaultSortField: "CreatedAt");

            _logger.LogInformation("Retrieved {Count} areas", result.TotalCount);

            return result;
        }

        public async Task<AreaDto> GetAreaByIdAsync(int areaId)
        {
            _logger.LogInformation("Fetching area with ID: {AreaId}", areaId);

            var areaDto = await _mapper.ProjectToAreaDto(_db.Areas
                .Where(a => a.AreaId == areaId && a.DeletedAt == null)
                .AsNoTracking())
                .FirstOrDefaultAsync();

            if (areaDto is null)
            {
                _logger.LogWarning("Area with ID {AreaId} not found", areaId);
                throw new NotFoundException("Khu vực", areaId);
            }

            return areaDto;
        }

        public async Task<AreaDto> CreateAreaAsync(CreateAreaDto dto)
        {
            _logger.LogInformation("Creating new area with name: {Name}", dto.Name);

            // Validation
            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            // Check for duplicates
            var existing = await _db.Areas
                .Where(a => a.Name == dto.Name && a.DeletedAt == null)
                .AnyAsync();

            if (existing)
            {
                _logger.LogWarning("Area with name {Name} already exists", dto.Name);
                throw new ConflictException("Khu vực", dto.Name);
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var newArea = _mapper.MapToArea(dto);
                // CreatedAt set automatically by interceptor

                await _db.Areas.AddAsync(newArea);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created area with ID: {AreaId}", newArea.AreaId);

                return _mapper.MapToAreaDto(newArea);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AreaDto> UpdateAreaAsync(int areaId, UpdateAreaDto dto)
        {
            _logger.LogInformation("Updating area {AreaId}", areaId);

            // Validation
            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var area = await _db.Areas.FindAsync(areaId);

            if (area is null || area.DeletedAt != null)
            {
                _logger.LogWarning("Area {AreaId} not found", areaId);
                throw new NotFoundException("Khu vực", areaId);
            }

            bool hasChanges = false;

            // Update Name
            if (dto.Name != null && area.Name != dto.Name)
            {
                // Check for duplicate name
                var nameExists = await _db.Areas
                    .Where(a => a.Name == dto.Name && a.AreaId != areaId && a.DeletedAt == null)
                    .AnyAsync();

                if (nameExists)
                {
                    throw new ConflictException("Khu vực", dto.Name);
                }

                area.Name = dto.Name;
                hasChanges = true;
            }

            // Update Description
            if (dto.Description != null && area.Description != dto.Description)
            {
                area.Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description;
                hasChanges = true;
            }

            if (hasChanges)
            {
                // UpdatedAt set automatically by interceptor
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated area {AreaId}", areaId);
            }
            else
            {
                _logger.LogInformation("No changes detected for area {AreaId}", areaId);
            }

            return _mapper.MapToAreaDto(area);
        }

        /// <summary>
        /// Delete single area
        /// Pattern: RESTful single resource delete (returns void, throws on error)
        /// Reference: Microsoft REST API Guidelines - DELETE returns 204 No Content
        /// Business Rules: Cannot delete if area is in use by employees
        /// </summary>
        public async Task DeleteAreaAsync(int areaId, bool softDelete = true)
        {
            _logger.LogInformation("Deleting area {AreaId} | SoftDelete: {SoftDelete}",
                areaId, softDelete);

            var area = await _db.Areas
                .Include(a => a.Employees) // Load relationship for dependency check
                .FirstOrDefaultAsync(a => a.AreaId == areaId && a.DeletedAt == null);

            if (area == null)
            {
                _logger.LogWarning("Area {AreaId} not found", areaId);
                throw new NotFoundException("Khu vực", areaId);
            }

            // ✅ Business rule: Check if area is in use
            if (area.Employees != null && area.Employees.Any(e => e.DeletedAt == null))
            {
                var activeEmployeeCount = area.Employees.Count(e => e.DeletedAt == null);
                _logger.LogWarning("Area {AreaId}:{Name} in use by {Count} employees",
                    area.AreaId, area.Name, activeEmployeeCount);
                throw new BusinessRuleException(
                    $"Không thể xóa khu vực {area.Name} vì đang được sử dụng bởi {activeEmployeeCount} nhân viên",
                    "AREA_IN_USE");
            }

            // ✅ Perform delete
            if (softDelete)
            {
                area.DeletedAt = DateTime.UtcNow;
                _db.Areas.Update(area);
                
                await _db.SaveChangesAsync();
                
                _logger.LogInformation("Soft deleted area {AreaId}:{Name}", 
                    area.AreaId, area.Name);
            }
            else
            {
                // Hard delete with transaction
                using var transaction = await _db.Database.BeginTransactionAsync();
                
                try
                {
                    _db.Areas.Remove(area);
                    
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    _logger.LogInformation("Hard deleted area {AreaId}:{Name}", 
                        area.AreaId, area.Name);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete multiple areas with all-or-nothing transaction
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Strategy: Validate ALL → Delete ALL → Return summary
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        public async Task<BulkDeleteResultDto> DeleteAreasAsync(List<int> areaIds, bool softDelete = true)
        {
            _logger.LogInformation("Batch delete started | Count: {Count} | SoftDelete: {SoftDelete}",
                areaIds?.Count ?? 0, softDelete);

            // ✅ Input validation
            ArgumentNullException.ThrowIfNull(areaIds);

            if (areaIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("areaIds",
                    "Vui lòng chọn ít nhất một khu vực để xóa");
            }

            // ✅ Remove duplicates
            var uniqueIds = areaIds.Distinct().ToList();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // ✅ Step 1: Validate ALL items BEFORE any deletion
                var existing = await _db.Areas
                    .Where(a => uniqueIds.Contains(a.AreaId) && a.DeletedAt == null)
                    .Include(a => a.Employees)
                    .ToListAsync();

                var notFoundIds = uniqueIds.Except(existing.Select(a => a.AreaId)).ToList();
                if (notFoundIds.Any())
                {
                    _logger.LogWarning("Areas not found: {Ids}", string.Join(", ", notFoundIds));
                    throw new NotFoundException($"Khu vực không tồn tại: {string.Join(", ", notFoundIds)}");
                }

                // ✅ Step 2: Check business rules for ALL items
                var areasInUse = existing
                    .Where(a => a.Employees != null && a.Employees.Any(e => e.DeletedAt == null))
                    .ToList();

                if (areasInUse.Any())
                {
                    var errorDetails = areasInUse
                        .Select(a => $"{a.Name} ({a.Employees.Count(e => e.DeletedAt == null)} nhân viên)")
                        .ToList();

                    _logger.LogWarning("Areas in use: {Areas}",
                        string.Join(", ", areasInUse.Select(a => $"{a.AreaId}:{a.Name}")));

                    throw new BusinessRuleException(
                        $"Không thể xóa khu vực {string.Join(", ", errorDetails)} vì đang được sử dụng",
                        "AREA_IN_USE");
                }

                // ✅ Step 3: All checks passed → Delete ALL
                foreach (var area in existing)
                {
                    if (softDelete)
                        area.DeletedAt = DateTime.UtcNow;
                    else
                        _db.Areas.Remove(area);

                    _logger.LogInformation("Deleted area {AreaId}:{Name}", area.AreaId, area.Name);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Batch delete SUCCESS | Count: {Count}", existing.Count);

                return new BulkDeleteResultDto
                {
                    Success = true,
                    DeletedCount = existing.Count,
                    TotalRequested = areaIds.Count,
                    Message = $"Đã xóa {existing.Count} khu vực thành công"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
