using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Dto;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.Employee
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _db;
        private readonly EmployeeMapper _mapper;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IValidator<CreateEmployeeDto> _createValidator;
        private readonly IValidator<UpdateEmployeeDto> _updateValidator;
        private readonly IValidator<UpdateProfileDto> _profileValidator;

        public EmployeeService(
            AppDbContext db,
            EmployeeMapper mapper,
            ILogger<EmployeeService> logger,
            IValidator<CreateEmployeeDto> createValidator,
            IValidator<UpdateEmployeeDto> updateValidator,
            IValidator<UpdateProfileDto> profileValidator)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _profileValidator = profileValidator;
        }

        public async Task<PaginatedResult<ListEmployeeDto>> GetEmployeesAsync(QueryParameters parameters)
        {
            _logger.LogInformation("Fetching employees with search: {Search}, page: {Page}",
                parameters.Search, parameters.Page);

            var employeeQuery = _db.Employees
                .Where(e => e.DeletedAt == null)
                .AsNoTracking();

            if (parameters.DptId.HasValue)
            {
                employeeQuery = employeeQuery.Where(e => e.DptId == parameters.DptId.Value);
            }

            if (parameters.AreaId.HasValue)
            {
                employeeQuery = employeeQuery.Where(e => e.AreaId == parameters.AreaId.Value);
            }

            var query = _mapper.ProjectToListEmployeeDto(employeeQuery);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(e =>
                    e.FullName.Contains(parameters.Search) ||
                    (e.Email != null && e.Email.Contains(parameters.Search)) ||
                    (e.EmpCode != null && e.EmpCode.Contains(parameters.Search)) ||
                    (e.PhoneNumber != null && e.PhoneNumber.Contains(parameters.Search)) ||
                    (e.Position != null && e.Position.Contains(parameters.Search)));
            }

            // Use extension method
            var result = await query.ToPaginatedResultAsync(parameters, defaultSortField: "CreatedAt");

            _logger.LogInformation("Retrieved {Count} employees", result.TotalCount);

            return result;
        }

        public async Task<DetailEmployeeDto> GetEmployeeByIdAsync(Guid empId)
        {
            _logger.LogInformation("Fetching employee {EmpId}", empId);

            var employee = await _mapper.ProjectToDetailEmployeeDto(_db.Employees
                .Include(e => e.Account)
                    .ThenInclude(a => a.AccountRoles)
                        .ThenInclude(ar => ar.Role)
                .Where(e => e.EmpId == empId && e.DeletedAt == null)
                .AsNoTracking())
                .FirstOrDefaultAsync();

            if (employee is null)
            {
                _logger.LogWarning("Employee {EmpId} not found", empId);
                throw new NotFoundException("Nhân viên", empId);
            }

            return employee;
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            _logger.LogInformation("Creating new employee: {FullName}", dto.FullName);

            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            // Check duplicate email
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExists = await _db.Employees
                    .Where(e => e.Email == dto.Email && e.DeletedAt == null)
                    .AnyAsync();

                if (emailExists)
                {
                    _logger.LogWarning("Email {Email} already exists", dto.Email);
                    throw new ConflictException("Email", dto.Email);
                }
            }

            // Check if EmpCode is provided and is unique
            if (!string.IsNullOrWhiteSpace(dto.EmpCode))
            {
                var codeExists = await _db.Employees
                    .Where(e => e.EmpCode == dto.EmpCode && e.DeletedAt == null)
                    .AnyAsync();

                if (codeExists)
                {
                    throw new ConflictException("Mã nhân viên", dto.EmpCode);
                }
            }

            // Validate Department exists
            var deptExists = await _db.Departments
                .Where(d => d.DptId == dto.DptId && d.DeletedAt == null)
                .AnyAsync();

            if (!deptExists)
            {
                throw new NotFoundException("Phòng ban", dto.DptId);
            }

            // Validate Area exists
            var areaExists = await _db.Areas
                .Where(a => a.AreaId == dto.AreaId && a.DeletedAt == null)
                .AnyAsync();

            if (!areaExists)
            {
                throw new NotFoundException("Khu vực", dto.AreaId);
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var newEmployee = _mapper.MapToEmployee(dto);
                newEmployee.EmpId = Guid.CreateVersion7();

                // Auto-generate EmpCode if not provided
                if (string.IsNullOrWhiteSpace(newEmployee.EmpCode))
                {
                    var count = await _db.Employees.CountAsync() + 1;
                    newEmployee.EmpCode = $"NV{count:D4}";
                }

                await _db.Employees.AddAsync(newEmployee);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created employee {EmpId}", newEmployee.EmpId);

                return _mapper.MapToEmployeeDto(newEmployee);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EmployeeDto> UpdateEmployeeAsync(Guid empId, UpdateEmployeeDto dto)
        {
            _logger.LogInformation("Updating employee {EmpId}", empId);

            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var employee = await _db.Employees.FindAsync(empId);

            if (employee is null || employee.DeletedAt != null)
            {
                _logger.LogWarning("Employee {EmpId} not found", empId);
                throw new NotFoundException("Nhân viên", empId);
            }

            bool hasChanges = false;

            // EmpCode
            if (dto.EmpCode != null && employee.EmpCode != dto.EmpCode)
            {
                var codeExists = await _db.Employees
                    .Where(e => e.EmpCode == dto.EmpCode && e.EmpId != empId && e.DeletedAt == null)
                    .AnyAsync();

                if (codeExists)
                {
                    throw new ConflictException("Mã nhân viên", dto.EmpCode);
                }

                employee.EmpCode = dto.EmpCode;
                hasChanges = true;
            }

            // FullName
            if (dto.FullName != null && employee.FullName != dto.FullName)
            {
                employee.FullName = dto.FullName;
                hasChanges = true;
            }

            // PhoneNumber
            if (dto.PhoneNumber != null && employee.PhoneNumber != dto.PhoneNumber)
            {
                employee.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber;
                hasChanges = true;
            }

            // Email
            if (dto.Email != null && employee.Email != dto.Email)
            {
                var emailExists = await _db.Employees
                    .Where(e => e.Email == dto.Email && e.EmpId != empId && e.DeletedAt == null)
                    .AnyAsync();

                if (emailExists)
                {
                    throw new ConflictException("Email", dto.Email);
                }

                employee.Email = dto.Email;
                hasChanges = true;
            }

            // DptId
            if (dto.DptId.HasValue && employee.DptId != dto.DptId.Value)
            {
                var deptExists = await _db.Departments
                    .Where(d => d.DptId == dto.DptId.Value && d.DeletedAt == null)
                    .AnyAsync();

                if (!deptExists)
                {
                    throw new NotFoundException("Phòng ban", dto.DptId.Value);
                }

                employee.DptId = dto.DptId.Value;
                hasChanges = true;
            }

            // AreaId
            if (dto.AreaId.HasValue && employee.AreaId != dto.AreaId.Value)
            {
                var areaExists = await _db.Areas
                    .Where(a => a.AreaId == dto.AreaId.Value && a.DeletedAt == null)
                    .AnyAsync();

                if (!areaExists)
                {
                    throw new NotFoundException("Khu vực", dto.AreaId.Value);
                }

                employee.AreaId = dto.AreaId.Value;
                hasChanges = true;
            }

            // Position
            if (dto.Position != null && employee.Position != dto.Position)
            {
                employee.Position = string.IsNullOrWhiteSpace(dto.Position) ? null : dto.Position;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated employee {EmpId}", empId);
            }
            else
            {
                _logger.LogInformation("No changes detected for employee {EmpId}", empId);
            }

            return _mapper.MapToEmployeeDto(employee);
        }

        /// <summary>
        /// Delete single employee
        /// Pattern: RESTful single resource delete (returns void, throws on error)
        /// Reference: Microsoft REST API Guidelines - DELETE returns 204 No Content
        /// Business Rules: 
        /// - Cannot delete Super_Admin
        /// - Cannot delete if employee has IssueLogs
        /// - Cascade delete associated Account
        /// </summary>
        public async Task DeleteEmployeeAsync(Guid empId, bool softDelete = true)
        {
            _logger.LogInformation("Deleting employee {EmpId} | SoftDelete: {SoftDelete}",
                empId, softDelete);

            var employee = await _db.Employees
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.EmpId == empId && e.DeletedAt == null);

            if (employee == null)
            {
                _logger.LogWarning("Employee {EmpId} not found", empId);
                throw new NotFoundException("Nhân viên", empId);
            }

            // ✅ Business rule 1: Protect Super_Admin
            if (employee.Position == "Super_Admin")
            {
                _logger.LogWarning("Attempted to delete Super_Admin {EmpId}:{FullName}",
                    employee.EmpId, employee.FullName);
                throw new BusinessRuleException(
                    "Không thể xóa tài khoản Super Admin",
                    "SUPER_ADMIN_PROTECTED");
            }

            // ✅ Business rule 2: Check IssueLogs references
            var hasIssueLogs = await _db.IssueLogs
                .Where(il => il.DeletedAt == null && 
                           il.Operator.Contains(empId.ToString()))
                .AnyAsync();

            if (hasIssueLogs)
            {
                var issueLogCount = await _db.IssueLogs
                    .CountAsync(il => il.DeletedAt == null && 
                                    il.Operator.Contains(empId.ToString()));

                _logger.LogWarning("Employee {EmpId}:{FullName} has {Count} issue logs",
                    employee.EmpId, employee.FullName, issueLogCount);

                throw new BusinessRuleException(
                    $"Không thể xóa nhân viên '{employee.FullName}' vì có {issueLogCount} nhật ký sự cố liên quan",
                    "EMPLOYEE_HAS_ISSUE_LOGS");
            }

            // ✅ Perform delete with account cascade
            if (softDelete)
            {
                employee.DeletedAt = DateTime.UtcNow;

                // Cascade soft delete associated account
                if (employee.Account != null)
                {
                    employee.Account.DeletedAt = DateTime.UtcNow;
                }

                _db.Employees.Update(employee);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Soft deleted employee {EmpId}:{FullName}",
                    employee.EmpId, employee.FullName);
            }
            else
            {
                // Hard delete with transaction
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    // Delete account first (FK constraint)
                    if (employee.Account != null)
                    {
                        _db.Accounts.Remove(employee.Account);
                    }

                    _db.Employees.Remove(employee);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Hard deleted employee {EmpId}:{FullName}",
                        employee.EmpId, employee.FullName);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete multiple employees with all-or-nothing transaction
        /// Pattern: Microsoft Dynamics 365 bulk operations
        /// Strategy: Validate ALL → Delete ALL → Return summary
        /// Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/bulk-operations
        /// </summary>
        public async Task<BulkDeleteResultDto> DeleteEmployeesAsync(List<Guid> empIds, bool softDelete = true)
        {
            _logger.LogInformation("Batch delete started | Count: {Count} | SoftDelete: {SoftDelete}",
                empIds?.Count ?? 0, softDelete);

            // ✅ Input validation
            ArgumentNullException.ThrowIfNull(empIds);

            if (empIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("empIds",
                    "Vui lòng chọn ít nhất một nhân viên để xóa");
            }

            // ✅ Remove duplicates
            var uniqueIds = empIds.Distinct().ToList();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // ✅ Step 1: Validate ALL items BEFORE any deletion
                var existing = await _db.Employees
                    .Include(e => e.Account)
                    .Where(e => uniqueIds.Contains(e.EmpId) && e.DeletedAt == null)
                    .ToListAsync();

                var notFoundIds = uniqueIds.Except(existing.Select(e => e.EmpId)).ToList();
                if (notFoundIds.Any())
                {
                    _logger.LogWarning("Employees not found: {Ids}", 
                        string.Join(", ", notFoundIds));
                    throw new NotFoundException(
                        $"Nhân viên không tồn tại: {string.Join(", ", notFoundIds)}");
                }

                // ✅ Step 2: Check business rules for ALL items

                // Rule 1: Check for Super_Admin
                var superAdmins = existing.Where(e => e.Position == "Super_Admin").ToList();
                if (superAdmins.Any())
                {
                    var adminNames = string.Join(", ", superAdmins.Select(e => e.FullName));
                    _logger.LogWarning("Attempted to delete Super_Admin: {Admins}", adminNames);
                    throw new BusinessRuleException(
                        "Không thể xóa tài khoản Super Admin",
                        "SUPER_ADMIN_PROTECTED");
                }

                // Rule 2: Check for IssueLogs references
                var employeeIdsInLogs = await _db.IssueLogs
                    .Where(il => il.DeletedAt == null &&
                           uniqueIds.Any(id => il.Operator.Contains(id.ToString())))
                    .Select(il => il.Operator)
                    .Distinct()
                    .ToListAsync();

                if (employeeIdsInLogs.Any())
                {
                    var employeesWithLogs = existing
                        .Where(e => employeeIdsInLogs.Any(log => log.Contains(e.EmpId.ToString())))
                        .ToList();

                    var errorDetails = new List<string>();
                    foreach (var emp in employeesWithLogs)
                    {
                        var count = await _db.IssueLogs
                            .CountAsync(il => il.DeletedAt == null && 
                                            il.Operator.Contains(emp.EmpId.ToString()));
                        errorDetails.Add($"{emp.FullName} ({count} nhật ký)");
                    }

                    _logger.LogWarning("Employees with issue logs: {Employees}",
                        string.Join(", ", employeesWithLogs.Select(e => $"{e.EmpId}:{e.FullName}")));

                    throw new BusinessRuleException(
                        $"Không thể xóa nhân viên {string.Join(", ", errorDetails)} vì có nhật ký sự cố liên quan",
                        "EMPLOYEE_HAS_ISSUE_LOGS");
                }

                // ✅ Step 3: All checks passed → Delete ALL
                foreach (var employee in existing)
                {
                    if (softDelete)
                    {
                        employee.DeletedAt = DateTime.UtcNow;

                        // Cascade soft delete account
                        if (employee.Account != null)
                        {
                            employee.Account.DeletedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        // Hard delete account first
                        if (employee.Account != null)
                        {
                            _db.Accounts.Remove(employee.Account);
                        }

                        _db.Employees.Remove(employee);
                    }

                    _logger.LogInformation("Deleted employee {EmpId}:{FullName}", 
                        employee.EmpId, employee.FullName);
                }

                if (softDelete)
                {
                    _db.Employees.UpdateRange(existing);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Batch delete SUCCESS | Count: {Count}", existing.Count);

                return new BulkDeleteResultDto
                {
                    Success = true,
                    DeletedCount = existing.Count,
                    TotalRequested = empIds.Count,
                    Message = $"Đã xóa {existing.Count} nhân viên thành công"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProfileDto> GetProfileAsync(Guid empId)
        {
            _logger.LogInformation("Fetching profile for employee {EmpId}", empId);

            var profile = await _mapper.ProjectToProfileDto(_db.Employees
                .Include(e => e.Account)
                .Where(e => e.EmpId == empId && e.DeletedAt == null)
                .AsNoTracking())
                .FirstOrDefaultAsync();

            if (profile is null)
            {
                _logger.LogWarning("Employee {EmpId} not found", empId);
                throw new NotFoundException("Nhân viên", empId);
            }

            return profile;
        }

        public async Task<ProfileDto> UpdateProfileAsync(Guid empId, UpdateProfileDto dto)
        {
            _logger.LogInformation("Updating profile for employee {EmpId}", empId);

            var validationResult = await _profileValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var employee = await _db.Employees
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.EmpId == empId && e.DeletedAt == null);

            if (employee is null)
            {
                _logger.LogWarning("Employee {EmpId} not found", empId);
                throw new NotFoundException("Nhân viên", empId);
            }

            bool hasChanges = false;

            // FullName
            if (employee.FullName != dto.FullName)
            {
                employee.FullName = dto.FullName;
                hasChanges = true;
            }

            // PhoneNumber
            if (dto.PhoneNumber != null && employee.PhoneNumber != dto.PhoneNumber)
            {
                employee.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
                    ? null
                    : dto.PhoneNumber;
                hasChanges = true;
            }

            // Email
            if (dto.Email != null && employee.Email != dto.Email)
            {
                var emailExists = await _db.Employees
                    .Where(e => e.Email == dto.Email && e.EmpId != empId && e.DeletedAt == null)
                    .AnyAsync();

                if (emailExists)
                {
                    throw new ConflictException("Email", dto.Email);
                }

                employee.Email = dto.Email;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated profile for {EmpId}", empId);
            }
            else
            {
                _logger.LogInformation("No changes detected for employee {EmpId}", empId);
            }

            return _mapper.MapToProfileDto(employee);
        }

        public async Task<DetailEmployeeDto> AssignRolesToEmployeeAsync(Guid empId, List<int> roleIds)
        {
            _logger.LogInformation("Assigning roles to employee {EmpId}", empId);

            ArgumentNullException.ThrowIfNull(roleIds);

            var employee = await _db.Employees
                .Include(e => e.Account)
                    .ThenInclude(a => a!.AccountRoles)
                .FirstOrDefaultAsync(e => e.EmpId == empId && e.DeletedAt == null);

            if (employee is null)
            {
                throw new NotFoundException("Nhân viên", empId);
            }

            if (employee.Account is null)
            {
                throw new BusinessRuleException("Nhân viên chưa có tài khoản");
            }

            // Validate all roles exist
            var existingRoles = await _db.Roles
                .Where(r => roleIds.Contains(r.RoleId) && r.DeletedAt == null)
                .Select(r => r.RoleId)
                .ToListAsync();

            var missingRoles = roleIds.Except(existingRoles).ToList();
            if (missingRoles.Any())
            {
                throw new NotFoundException($"Roles không tồn tại: {string.Join(", ", missingRoles)}");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Remove existing roles
                var currentRoles = await _db.AccountRoles
                    .Where(ar => ar.AccountId == employee.EmpId)
                    .ToListAsync();

                _db.AccountRoles.RemoveRange(currentRoles);

                // Add new roles
                var newRoles = roleIds.Select(roleId => new AccountRoles
                {
                    AccountId = employee.EmpId,
                    RoleId = roleId
                });

                await _db.AccountRoles.AddRangeAsync(newRoles);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully assigned {Count} roles to employee {EmpId}",
                    roleIds.Count, empId);

                return await GetEmployeeByIdAsync(empId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
