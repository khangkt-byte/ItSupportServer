using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Shared.Base;
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

            var query = _mapper.ProjectToListEmployeeDto(_db.Employees
                .Where(e => e.DeletedAt == null)
                .AsNoTracking());

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

        public async Task<bool> DeleteEmployeesAsync(List<Guid> empIds, bool softDelete = true)
        {
            _logger.LogInformation("Deleting {Count} employees (soft: {SoftDelete})",
                empIds?.Count ?? 0, softDelete);

            ArgumentNullException.ThrowIfNull(empIds);

            if (empIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("empIds",
                    "Vui lòng chọn nhân viên để xóa");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var existing = await _db.Employees
                    .Include(e => e.Account)
                    .Where(e => empIds.Contains(e.EmpId) && e.DeletedAt == null)
                    .ToListAsync();

                if (existing.Count == 0)
                {
                    throw new NotFoundException("Không tìm thấy nhân viên để xóa");
                }

                // Check for Super_Admin protection
                var hasSuperAdmin = existing.Any(e => e.Position == "Super_Admin");
                if (hasSuperAdmin)
                {
                    throw new BusinessRuleException("Không thể xóa tài khoản Super Admin");
                }

                // Check if employee has related data
                var hasIssueLogs = await _db.IssueLogs
                    .Where(il => il.DeletedAt == null &&
                           empIds.Any(id => il.Operator.Contains(id.ToString())))
                    .AnyAsync();

                if (hasIssueLogs)
                {
                    throw new BusinessRuleException(
                        "Không thể xóa nhân viên đang có liên kết với nhật ký sự cố");
                }

                if (softDelete)
                {
                    foreach (var item in existing)
                    {
                        item.DeletedAt = DateTime.UtcNow;

                        // Also soft delete associated account
                        if (item.Account != null)
                        {
                            item.Account.DeletedAt = DateTime.UtcNow;
                        }
                    }
                    _db.Employees.UpdateRange(existing);
                }
                else
                {
                    // Hard delete accounts first (FK constraint)
                    var accountsToDelete = existing
                        .Where(e => e.Account != null)
                        .Select(e => e.Account!)
                        .ToList();

                    if (accountsToDelete.Any())
                    {
                        _db.Accounts.RemoveRange(accountsToDelete);
                    }

                    _db.Employees.RemoveRange(existing);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully deleted {Count} employees", existing.Count);

                return true;
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
