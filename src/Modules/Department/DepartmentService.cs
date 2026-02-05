using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Dto;
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

        /// <summary>
        /// Delete single department
        /// Pattern: RESTful single resource delete (returns void, throws on error)
        /// Reference: Microsoft REST API Guidelines - DELETE returns 204 No Content
        /// Business Rules: 
        /// - Cannot delete if department has Employees (DEPARTMENT_HAS_EMPLOYEES)
        /// - Cannot delete if department has IssueLogs (DEPARTMENT_IN_USE)
        /// </summary>
        public async Task DeleteDepartmentAsync(int dptId, bool softDelete = true)
        {
            _logger.LogInformation("Deleting department {DptId} | SoftDelete: {SoftDelete}",
                dptId, softDelete);

            // ✅ FIX 1: Use transaction for BOTH soft and hard delete
            // Reference: Microsoft Entity Framework Best Practices
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var department = await _db.Departments
                    .FirstOrDefaultAsync(d => d.DptId == dptId && d.DeletedAt == null);

                if (department == null)
                {
                    _logger.LogWarning("Department {DptId} not found", dptId);
                    throw new NotFoundException("Phòng ban", dptId);
                }

                // ✅ Business rule 1: Check for Employees
                // Pattern: Referential integrity check
                // Reference: Database Design Best Practices
                var hasEmployees = await _db.Employees
                    .AnyAsync(e => e.DptId == dptId && e.DeletedAt == null);

                if (hasEmployees)
                {
                    var employeeCount = await _db.Employees
                        .CountAsync(e => e.DptId == dptId && e.DeletedAt == null);

                    _logger.LogWarning("Department {DptId}:{Name} has {Count} employees",
                        department.DptId, department.Name, employeeCount);

                    throw new BusinessRuleException(
                        $"Không thể xóa phòng ban '{department.Name}' vì có {employeeCount} nhân viên",
                        "DEPARTMENT_HAS_EMPLOYEES");
                }

                // ✅ Business rule 2: Check for IssueLogs
                var hasIssueLogs = await _db.IssueLogs
                    .AnyAsync(il => il.DptId == dptId && il.DeletedAt == null);

                if (hasIssueLogs)
                {
                    var issueLogCount = await _db.IssueLogs
                        .CountAsync(il => il.DptId == dptId && il.DeletedAt == null);

                    _logger.LogWarning("Department {DptId}:{Name} has {Count} issue logs",
                        department.DptId, department.Name, issueLogCount);

                    throw new BusinessRuleException(
                        $"Không thể xóa phòng ban '{department.Name}' vì có {issueLogCount} nhật ký sự cố",
                        "DEPARTMENT_IN_USE");
                }

                // ✅ Perform delete
                if (softDelete)
                {
                    department.DeletedAt = DateTime.UtcNow;
                    _db.Departments.Update(department);
                    
                    _logger.LogInformation("Soft deleted department {DptId}:{Name}",
                        department.DptId, department.Name);
                }
                else
                {
                    _db.Departments.Remove(department);
                    
                    _logger.LogInformation("Hard deleted department {DptId}:{Name}",
                        department.DptId, department.Name);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Delete multiple departments with all-or-nothing transaction
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Strategy: Validate ALL → Delete ALL → Return summary
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        public async Task<BulkDeleteResultDto> DeleteDepartmentsAsync(List<int> dptIds, bool softDelete = true)
        {
            _logger.LogInformation("Batch delete started | Count: {Count} | SoftDelete: {SoftDelete}", 
                dptIds?.Count ?? 0, softDelete);

            // ✅ Input validation
            ArgumentNullException.ThrowIfNull(dptIds);

            if (dptIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("dptIds",
                    "Vui lòng chọn ít nhất một phòng ban để xóa");
            }

            // ✅ Remove duplicates
            var uniqueIds = dptIds.Distinct().ToList();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // ✅ Step 1: Validate ALL items BEFORE any deletion
                var existing = await _db.Departments
                    .Where(d => uniqueIds.Contains(d.DptId) && d.DeletedAt == null)
                    .ToListAsync();

                var notFoundIds = uniqueIds.Except(existing.Select(d => d.DptId)).ToList();
                if (notFoundIds.Any())
                {
                    _logger.LogWarning("Departments not found: {Ids}", string.Join(", ", notFoundIds));
                    throw new NotFoundException($"Phòng ban không tồn tại: {string.Join(", ", notFoundIds)}");
                }

                // ✅ Step 2: Check business rules for ALL items

                // Rule 1: Check for Employees
                var deptsWithEmployees = await _db.Employees
                    .Where(e => uniqueIds.Contains(e.DptId) && e.DeletedAt == null)
                    .Select(e => e.DptId)
                    .Distinct()
                    .ToListAsync();

                if (deptsWithEmployees.Any())
                {
                    var usedDepts = existing
                        .Where(d => deptsWithEmployees.Contains(d.DptId))
                        .ToList();

                    var errorDetails = new List<string>();
                    foreach (var dept in usedDepts)
                    {
                        var count = await _db.Employees
                            .CountAsync(e => e.DptId == dept.DptId && e.DeletedAt == null);
                        errorDetails.Add($"{dept.Name} ({count} nhân viên)");
                    }

                    _logger.LogWarning("Departments with employees: {Depts}",
                        string.Join(", ", usedDepts.Select(d => $"{d.DptId}:{d.Name}")));

                    throw new BusinessRuleException(
                        $"Không thể xóa phòng ban {string.Join(", ", errorDetails)} vì có nhân viên",
                        "DEPARTMENT_HAS_EMPLOYEES");
                }

                // Rule 2: Check for IssueLogs
                var deptsWithIssueLogs = await _db.IssueLogs
                    .Where(il => uniqueIds.Contains(il.DptId) && il.DeletedAt == null)
                    .Select(il => il.DptId)
                    .Distinct()
                    .ToListAsync();

                if (deptsWithIssueLogs.Any())
                {
                    var usedDepts = existing
                        .Where(d => deptsWithIssueLogs.Contains(d.DptId))
                        .ToList();

                    var errorDetails = new List<string>();
                    foreach (var dept in usedDepts)
                    {
                        var count = await _db.IssueLogs
                            .CountAsync(il => il.DptId == dept.DptId && il.DeletedAt == null);
                        errorDetails.Add($"{dept.Name} ({count} nhật ký)");
                    }

                    _logger.LogWarning("Departments with issue logs: {Depts}",
                        string.Join(", ", usedDepts.Select(d => $"{d.DptId}:{d.Name}")));

                    throw new BusinessRuleException(
                        $"Không thể xóa phòng ban {string.Join(", ", errorDetails)} vì có nhật ký sự cố",
                        "DEPARTMENT_IN_USE");
                }

                // ✅ Step 3: All checks passed → Delete ALL
                if (softDelete)
                {
                    // ✅ FIX 2: Optimize soft delete with UpdateRange
                    // Pattern: Batch update for performance
                    // Reference: Microsoft EF Core - Efficient Updating
                    var now = DateTime.UtcNow;
                    foreach (var dept in existing)
                    {
                        dept.DeletedAt = now;
                        _logger.LogInformation("Deleted department {DptId}:{Name}", dept.DptId, dept.Name);
                    }
                    _db.Departments.UpdateRange(existing);
                }
                else
                {
                    foreach (var dept in existing)
                    {
                        _db.Departments.Remove(dept);
                        _logger.LogInformation("Deleted department {DptId}:{Name}", dept.DptId, dept.Name);
                    }
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