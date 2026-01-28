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

        public async Task<PaginatedResult<List<AreaDto>>> GetAreasAsync(string? query, int page, int pageSize, SortOBJ? sort)
        {
            _logger.LogInformation("Fetching areas with query: {Query}, page: {Page}", query, page);

            var areasQuery = _mapper.ProjectToAreaDto(_db.Areas
                .Where(a => a.DeletedAt == null)
                .AsNoTracking());

            if (!string.IsNullOrWhiteSpace(query))
            {
                areasQuery = areasQuery.Where(a =>
                    a.Name.Contains(query) ||
                    (a.Description != null && a.Description.Contains(query)));
            }

            var result = await Pagination<AreaDto>.PaginationAsync(areasQuery, page, pageSize, sort);
            
            _logger.LogInformation("Retrieved {Count} areas", result.TotalItems);
            
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

            // Manual validation
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

            var newArea = _mapper.MapToArea(dto);
            // ✅ REMOVED: newArea.CreatedAt = DateTime.UtcNow;
            // Interceptor handles this automatically!

            await _db.Areas.AddAsync(newArea);
            await _db.SaveChangesAsync();  // ← CreatedAt set here by interceptor

            _logger.LogInformation("Successfully created area with ID: {AreaId}", newArea.AreaId);

            return _mapper.MapToAreaDto(newArea);
        }

        public async Task<AreaDto> UpdateAreaAsync(int areaId, UpdateAreaDto dto)
        {
            _logger.LogInformation("Updating area {AreaId}", areaId);

            // Manual validation
            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var area = await _db.Areas.FindAsync(areaId);

            if (area is null || area.DeletedAt != null)
            {
                _logger.LogWarning("Area {AreaId} not found", areaId);
                throw new NotFoundException("Khu vực", areaId);
            }

            // ✅ PARTIAL UPDATE LOGIC: Only update if value provided
            bool hasChanges = false;

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                // Check if name is different before updating
                if (area.Name != dto.Name)
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
            }

            // Description: null = don't update, empty string = clear the field
            if (dto.Description != null)
            {
                if (area.Description != dto.Description)
                {
                    area.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
                    hasChanges = true;
                }
            }

            // Only save if there are actual changes
            if (hasChanges)
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated area {AreaId}", areaId);
            }
            else
            {
                _logger.LogInformation("No changes detected for area {AreaId}", areaId);
            }

            return _mapper.MapToAreaDto(area);
        }

        public async Task<bool> DeleteAreasAsync(List<int> areaIds, bool softDelete = true)
        {
            _logger.LogInformation("Deleting {Count} areas (soft: {SoftDelete})", areaIds?.Count ?? 0, softDelete);

            ArgumentNullException.ThrowIfNull(areaIds);

            if (areaIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("areaIds", "Vui lòng chọn khu vực để xóa");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            
            try
            {
                // Validation
                if (areaIds == null || areaIds.Count == 0)
                {
                    throw new Shared.Exceptions.ValidationException("areaIds", "Vui lòng chọn khu vực để xóa");
                }

                var existing = await _db.Areas
                    .Where(c => areaIds.Contains(c.AreaId))
                    .ToListAsync();

                if (existing.Count == 0)
                {
                    throw new NotFoundException("Không tìm thấy khu vực nào để xóa");
                }

                // Check for dependencies
                var usedAreaIds = await _db.Employees
                    .Where(e => e.DeletedAt == null && areaIds.Contains(e.AreaId))
                    .Select(e => e.AreaId)
                    .Distinct()
                    .ToListAsync();

                if (usedAreaIds.Any())
                {
                    var usedAreaNames = existing
                        .Where(c => usedAreaIds.Contains(c.AreaId))
                        .Select(c => c.Name)
                        .ToList();

                    _logger.LogWarning("Cannot delete areas {Areas} - in use", string.Join(", ", usedAreaNames));

                    throw new BusinessRuleException(
                        $"Không thể xóa khu vực {string.Join(", ", usedAreaNames)} vì đang được sử dụng");
                }

                // Perform deletion
                if (softDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;  // ✅ Keep this - manual soft delete
                    }
                    _db.Areas.UpdateRange(existing);
                }
                else
                {
                    _db.Areas.RemoveRange(existing);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully deleted {Count} areas", existing.Count);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
