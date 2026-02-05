using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.Department
{
    /// <summary>
    /// Department service implementation
    /// Pattern: Domain-Driven Design (DDD) service layer
    /// Security: Input validation, FK verification, business rule enforcement
    /// Reference: ServiceNow organizational management
    /// </summary>
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _db;
        private readonly DepartmentMapper _mapper;
        private readonly ILogger<DepartmentService> _logger;
        private readonly IValidator<CreateDepartmentDto> _createValidator;
        private readonly IValidator<UpdateDepartmentDto> _updateValidator;

        public DepartmentService(
            AppDbContext db,
            DepartmentMapper mapper,
            ILogger<DepartmentService> logger,
            IValidator<CreateDepartmentDto> createValidator,
            IValidator<UpdateDepartmentDto> updateValidator)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PaginatedResult<DepartmentDto>> GetDepartmentsAsync(QueryParameters parameters)
        {
            _logger.LogInformation("Fetching departments with search: {Search}, page: {Page}",
                parameters.Search, parameters.Page);

            var query = _mapper.ProjectToDepartmentDto(_db.Departments
                .Where(d => d.DeletedAt == null)
                .AsNoTracking());

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(d => d.Name.Contains(parameters.Search));
            }

            var result = await query.ToPaginatedResultAsync(parameters, defaultSortField: "Name");

            // ✅ Batch calculate counts (2 queries instead of N)
            var dptIds = result.Items.Select(d => d.DptId).ToList();

            var employeeCounts = await _db.Employees
                .Where(e => dptIds.Contains(e.DptId) && e.DeletedAt == null)
                .GroupBy(e => e.DptId)
                .Select(g => new { DptId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DptId, x => x.Count);

            var issueLogCounts = await _db.IssueLogs
                .Where(il => dptIds.Contains(il.DptId) && il.DeletedAt == null)
                .GroupBy(il => il.DptId)
                .Select(g => new { DptId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DptId, x => x.Count);

            // ✅ Enrich with counts
            var itemsWithCounts = result.Items.Select(d => d with
            {
                EmployeeCount = employeeCounts.GetValueOrDefault(d.DptId, 0),
                IssueLogCount = issueLogCounts.GetValueOrDefault(d.DptId, 0)
            }).ToList();

            return result with { Items = itemsWithCounts };
        }

        public async Task<DepartmentDto> GetDepartmentByIdAsync(int dptId)
        {
            _logger.LogInformation("Fetching department {DptId}", dptId);

            var department = await _mapper.ProjectToDepartmentDto(_db.Departments
                .Where(d => d.DptId == dptId && d.DeletedAt == null)
                .AsNoTracking())
                .FirstOrDefaultAsync();

            if (department is null)
            {
                _logger.LogWarning("Department {DptId} not found", dptId);
                throw new NotFoundException("Phòng ban", dptId);
            }

            // ✅ Calculate counts (2 separate queries)
            var employeeCount = await _db.Employees
                .CountAsync(e => e.DptId == dptId && e.DeletedAt == null);

            var issueLogCount = await _db.IssueLogs
                .CountAsync(il => il.DptId == dptId && il.DeletedAt == null);

            return department with 
            { 
                EmployeeCount = employeeCount,
                IssueLogCount = issueLogCount
            };
        }

        public async Task<List<DepartmentSuggestionDto>> GetDepartmentSuggestionsAsync(string? search = null)
        {
            _logger.LogInformation("Fetching department suggestions, search: {Search}", search);

            var query = _db.Departments
                .Where(d => d.DeletedAt == null)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Name.Contains(search));
            }

            var departments = await query
                .OrderBy(d => d.Name)
                .Take(20)
                .Select(d => new
                {
                    d.DptId,
                    d.Name
                })
                .ToListAsync();

            // ✅ Calculate employee counts
            var dptIds = departments.Select(d => d.DptId).ToList();

            var employeeCounts = await _db.Employees
                .Where(e => dptIds.Contains(e.DptId) && e.DeletedAt == null)
                .GroupBy(e => e.DptId)
                .Select(g => new { DptId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DptId, x => x.Count);

            var suggestions = departments
                .Select(d => new DepartmentSuggestionDto
                {
                    DptId = d.DptId,
                    Name = d.Name,
                    EmployeeCount = employeeCounts.GetValueOrDefault(d.DptId, 0)
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} department suggestions", suggestions.Count);

            return suggestions;
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto dto)
        {
            _logger.LogInformation("Creating new department: {Name}", dto.Name);

            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            // Check duplicate name
            var nameExists = await _db.Departments
                .Where(d => d.Name == dto.Name && d.DeletedAt == null)
                .AnyAsync();

            if (nameExists)
            {
                _logger.LogWarning("Department with name {Name} already exists", dto.Name);
                throw new ConflictException("Phòng ban", dto.Name);
            }

            var newDepartment = _mapper.MapToDepartment(dto);
            // DptId auto-increment
            // CreatedAt set by interceptor

            await _db.Departments.AddAsync(newDepartment);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Successfully created department {DptId}", newDepartment.DptId);

            return await GetDepartmentByIdAsync(newDepartment.DptId);
        }

        public async Task<DepartmentDto> UpdateDepartmentAsync(int dptId, UpdateDepartmentDto dto)
        {
            _logger.LogInformation("Updating department {DptId}", dptId);

            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var department = await _db.Departments
                .FirstOrDefaultAsync(d => d.DptId == dptId);

            if (department is null || department.DeletedAt != null)
            {
                _logger.LogWarning("Department {DptId} not found", dptId);
                throw new NotFoundException("Phòng ban", dptId);
            }

            bool hasChanges = false;

            // Update Name
            if (dto.Name != null && department.Name != dto.Name)
            {
                // Check duplicate
                var nameExists = await _db.Departments
                    .Where(d => d.Name == dto.Name && d.DptId != dptId && d.DeletedAt == null)
                    .AnyAsync();

                if (nameExists)
                {
                    throw new ConflictException("Phòng ban", dto.Name);
                }

                department.Name = dto.Name;
                hasChanges = true;
            }

            // Update Description
            if (dto.Description != null && department.Description != dto.Description)
            {
                department.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
                hasChanges = true;
            }

            if (hasChanges)
            {
                // UpdatedAt set by interceptor
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated department {DptId}", dptId);
            }
            else
            {
                _logger.LogInformation("No changes detected for department {DptId}", dptId);
            }

            return await GetDepartmentByIdAsync(dptId);
        }

        public async Task<bool> DeleteDepartmentAsync(int dptId, bool softDelete = true)
        {
            _logger.LogInformation("Deleting department {DptId} | SoftDelete: {SoftDelete}",
                dptId, softDelete);

            var department = await _db.Departments
                .Include(d => d.Employees)
                .Include(d => d.IssueLogs)
                .FirstOrDefaultAsync(d => d.DptId == dptId && d.DeletedAt == null);

            if (department == null)
            {
                _logger.LogWarning("Department {DptId} not found", dptId);
                throw new NotFoundException("Phòng ban", dptId);
            }

            // ✅ Check business rules - department in use
            var hasEmployees = await _db.Employees
                .AnyAsync(e => e.DptId == dptId && e.DeletedAt == null);

            var hasIssueLogs = await _db.IssueLogs
                .AnyAsync(il => il.DptId == dptId && il.DeletedAt == null);

            if (hasEmployees || hasIssueLogs)
            {
                _logger.LogWarning("Department {DptId}:{Name} in use - Employees: {EmpCount}, IssueLogs: {LogCount}",
                    department.DptId, department.Name, 
                    department.Employees.Count, department.IssueLogs.Count);
                
                throw new BusinessRuleException(
                    $"Không thể xóa phòng ban '{department.Name}' vì đang được sử dụng",
                    "DEPARTMENT_IN_USE");
            }

            // Delete
            if (softDelete)
            {
                department.DeletedAt = DateTime.UtcNow;
                _db.Departments.Update(department);
            }
            else
            {
                _db.Departments.Remove(department);
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Deleted department {DptId}:{Name} successfully",
                department.DptId, department.Name);

            return true;
        }

        public async Task<BulkDeleteResultDto> DeleteDepartmentsAsync(List<int> dptIds, bool softDelete = true)
        {
            _logger.LogInformation("Batch delete started | Count: {Count} | SoftDelete: {SoftDelete}", 
                dptIds?.Count ?? 0, softDelete);

            ArgumentNullException.ThrowIfNull(dptIds);

            if (dptIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("dptIds",
                    "Vui lòng chọn phòng ban để xóa");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // ✅ Step 1: Validate ALL items BEFORE any deletion
                var existing = await _db.Departments
                    .Where(d => dptIds.Contains(d.DptId) && d.DeletedAt == null)
                    .ToListAsync();

                var notFoundIds = dptIds.Except(existing.Select(d => d.DptId)).ToList();
                if (notFoundIds.Any())
                {
                    _logger.LogWarning("Departments not found: {Ids}", string.Join(", ", notFoundIds));
                    throw new NotFoundException($"Phòng ban không tồn tại: {string.Join(", ", notFoundIds)}");
                }

                // ✅ Step 2: Check business rules for ALL items
                var dptIdsToCheck = existing.Select(d => d.DptId).ToList();

                var deptsWithEmployees = await _db.Employees
                    .Where(e => dptIdsToCheck.Contains(e.DptId) && e.DeletedAt == null)
                    .Select(e => e.DptId)
                    .Distinct()
                    .ToListAsync();

                var deptsWithIssueLogs = await _db.IssueLogs
                    .Where(il => dptIdsToCheck.Contains(il.DptId) && il.DeletedAt == null)
                    .Select(il => il.DptId)
                    .Distinct()
                    .ToListAsync();

                var deptsInUse = deptsWithEmployees.Union(deptsWithIssueLogs).Distinct().ToList();

                if (deptsInUse.Any())
                {
                    var usedDeptNames = existing
                        .Where(d => deptsInUse.Contains(d.DptId))
                        .Select(d => d.Name)
                        .ToList();

                    _logger.LogWarning("Departments in use: {Depts}",
                        string.Join(", ", usedDeptNames));

                    throw new BusinessRuleException(
                        $"Không thể xóa phòng ban {string.Join(", ", usedDeptNames)} vì đang được sử dụng",
                        "DEPARTMENT_IN_USE");
                }

                // ✅ Step 3: All checks passed → Delete ALL
                foreach (var dept in existing)
                {
                    if (softDelete)
                        dept.DeletedAt = DateTime.UtcNow;
                    else
                        _db.Departments.Remove(dept);

                    _logger.LogInformation("Deleted department {DptId}:{Name}", dept.DptId, dept.Name);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Batch delete SUCCESS | Count: {Count}", existing.Count);

                return new BulkDeleteResultDto
                {
                    Success = true,
                    DeletedCount = existing.Count,
                    TotalRequested = dptIds.Count,
                    Message = $"Đã xóa {existing.Count} phòng ban thành công"
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