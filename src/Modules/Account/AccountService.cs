using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Modules.Authorization;
using ItSupportServer.src.Modules.Role;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Dto;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Extensions;
using ItSupportServer.src.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static ItSupportServer.src.Shared.Base.BaseEnum;

namespace ItSupportServer.src.Modules.Account
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _db;
        private readonly AccountMapper _accountMapper;
        private readonly RoleMapper _roleMapper;
        private readonly ILogger<AccountService> _logger;
        private readonly IValidator<CreateAccountDto> _createValidator;
        private readonly IValidator<UpdateAccountDto> _updateValidator;
        private readonly IValidator<AssignRolesDto> _assignRolesValidator;
        private readonly IValidator<AssignClaimsDto> _assignClaimsValidator;
        private readonly IAuthorizationService _authorizationService;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;

        // Constants
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 30;

        public AccountService(
            AppDbContext db,
            AccountMapper accountMapper,
            RoleMapper roleMapper,
            ILogger<AccountService> logger,
            IValidator<CreateAccountDto> createValidator,
            IValidator<UpdateAccountDto> updateValidator,
            IValidator<AssignRolesDto> assignRolesValidator,
            IValidator<AssignClaimsDto> assignClaimsValidator,
            IAuthorizationService authorizationService,
            IValidator<ChangePasswordDto> changePasswordValidator)
        {
            _db = db;
            _accountMapper = accountMapper;
            _roleMapper = roleMapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _assignRolesValidator = assignRolesValidator;
            _assignClaimsValidator = assignClaimsValidator;
            _authorizationService = authorizationService;
            _changePasswordValidator = changePasswordValidator;
        }

        public async Task<PaginatedResult<ListAccountDto>> GetAccountsAsync(QueryParameters parameters)
        {
            _logger.LogInformation("Fetching accounts with search: {Search}, page: {Page}",
                parameters.Search, parameters.Page);

            var query = _accountMapper.ProjectToListAccountDto(_db.Accounts
                .Include(a => a.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                        .ThenInclude(r => r.RoleClaims)
                .Include(a => a.AccountClaims)
                .AsNoTracking());

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(a =>
                    a.Username.Contains(parameters.Search) ||
                    (a.EmpName != null && a.EmpName.Contains(parameters.Search)) ||
                    (a.EmpCode != null && a.EmpCode.Contains(parameters.Search)));
            }

            if (parameters.IsLocked.HasValue)
            {
                query = query.Where(a => a.IsLocked == parameters.IsLocked.Value);
            }

            var result = await query.ToPaginatedResultAsync(parameters, defaultSortField: "CreatedAt");

            _logger.LogInformation("Retrieved {Count} accounts", result.TotalCount);

            return result;
        }

        public async Task<AccountDto> GetAccountByIdAsync(Guid accountId)
        {
            _logger.LogInformation("Fetching account {AccountId}", accountId);

            var account = await _accountMapper.ProjectToAccountDto(_db.Accounts
                .Include(a => a.Employee)
                .Include(a => a.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                    .ThenInclude(r => r.RoleClaims)
                    .ThenInclude(rc => rc.Claim)
                .Include(a => a.AccountClaims)
                    .ThenInclude(ac => ac.Claim)
                .Where(a => a.AccountId == accountId && a.DeletedAt == null)
                .AsNoTracking())
                .FirstOrDefaultAsync();

            if (account is null)
            {
                _logger.LogWarning("Account {AccountId} not found", accountId);
                throw new NotFoundException("Tài khoản", accountId);
            }

            return account;
        }

        public async Task<AccountDto> CreateAccountAsync(CreateAccountDto dto)
        {
            _logger.LogInformation("Creating account for employee {EmployeeId}", dto.EmpId);

            var validationResult = await _createValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            if (dto.Password != dto.ConfirmPassword)
            {
                throw new Shared.Exceptions.ValidationException("ConfirmPassword",
                    "Mật khẩu xác nhận không khớp");
            }

            var employee = await _db.Employees
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.EmpId == dto.EmpId && e.DeletedAt == null);

            if (employee is null)
            {
                throw new NotFoundException("Nhân viên", dto.EmpId);
            }

            if (employee.Account != null && employee.Account.DeletedAt == null)
            {
                throw new BusinessRuleException($"Nhân viên {employee.FullName} đã có tài khoản");
            }

            var usernameExists = await _db.Accounts
                .Where(a => a.Username == dto.Username && a.DeletedAt == null)
                .AnyAsync();

            if (usernameExists)
            {
                throw new ConflictException("Username", dto.Username);
            }

            if (dto.RoleIds != null && dto.RoleIds.Any())
            {
                var existingRoles = await _db.Roles
                    .Where(r => dto.RoleIds.Contains(r.RoleId) && r.DeletedAt == null)
                    .Select(r => r.RoleId)
                    .ToListAsync();

                var missingRoles = dto.RoleIds.Except(existingRoles).ToList();
                if (missingRoles.Any())
                {
                    throw new NotFoundException($"Roles không tồn tại: {string.Join(", ", missingRoles)}");
                }
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var newAccount = new Accounts
                {
                    AccountId = dto.EmpId,
                    Username = dto.Username,
                    Password = PasswordHelper.HashPassword(dto.Password),
                    IsLocked = false,
                    FailedLoginAttempts = 0
                };

                await _db.Accounts.AddAsync(newAccount);
                await _db.SaveChangesAsync();

                if (dto.RoleIds != null && dto.RoleIds.Any())
                {
                    var accountRoles = dto.RoleIds.Distinct().Select(roleId => new AccountRoles
                    {
                        AccountId = newAccount.AccountId,
                        RoleId = roleId
                    });

                    await _db.AccountRoles.AddRangeAsync(accountRoles);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created account {AccountId}", newAccount.AccountId);

                return await GetAccountByIdAsync(newAccount.AccountId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AccountDto> UpdateAccountAsync(Guid accountId, UpdateAccountDto dto)
        {
            _logger.LogInformation("Updating account {AccountId}", accountId);

            var validationResult = await _updateValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var account = await _db.Accounts.FindAsync(accountId);

            if (account is null || account.DeletedAt != null)
            {
                throw new NotFoundException("Tài khoản", accountId);
            }

            bool hasChanges = false;

            if (dto.Username != null && account.Username != dto.Username)
            {
                var usernameExists = await _db.Accounts
                    .Where(a => a.Username == dto.Username && a.AccountId != accountId && a.DeletedAt == null)
                    .AnyAsync();

                if (usernameExists)
                {
                    throw new ConflictException("Username", dto.Username);
                }

                account.Username = dto.Username;
                hasChanges = true;
            }

            if (dto.IsLocked.HasValue && account.IsLocked != dto.IsLocked.Value)
            {
                account.IsLocked = dto.IsLocked.Value;

                if (!dto.IsLocked.Value)
                {
                    account.FailedLoginAttempts = 0;
                    account.LockedUntil = null;
                }

                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    throw new Shared.Exceptions.ValidationException("ConfirmPassword",
                        "Mật khẩu xác nhận không khớp");
                }

                if (PasswordHelper.VerifyPassword(dto.NewPassword, account.Password))
                {
                    throw new BusinessRuleException("Mật khẩu mới phải khác mật khẩu hiện tại");
                }

                account.Password = PasswordHelper.HashPassword(dto.NewPassword);
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated account {AccountId}", accountId);
            }

            return await GetAccountByIdAsync(accountId);
        }

        public async Task DeleteAccountAsync(Guid accountId, bool softDelete = true)
        {
            _logger.LogInformation("Deleting account {AccountId} | SoftDelete: {SoftDelete}",
                accountId, softDelete);

            // ✅ Only include what we need
            var account = await _db.Accounts
                .Include(a => a.Employee)  // For Super_Admin check
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.DeletedAt == null);

            if (account is null)
            {
                _logger.LogWarning("Account {AccountId} not found", accountId);
                throw new NotFoundException("Tài khoản", accountId);
            }

            // ✅ Business rules validation
            if (account.Employee?.Position == "Super_Admin")
            {
                _logger.LogWarning("Attempt to delete Super Admin account {AccountId}:{Username}",
                    accountId, account.Username);
                throw new BusinessRuleException(
                    "Không thể xóa tài khoản Super Admin",
                    "CANNOT_DELETE_SUPER_ADMIN");  // ✅ Added error code
            }

            // TODO: Check if account has critical dependencies
            // var hasTickets = await _db.Tickets
            //     .AnyAsync(t => t.CreatedById == accountId || t.AssignedToId == accountId);
            // if (hasTickets)
            // {
            //     _logger.LogWarning("Account {AccountId} has {Count} related tickets", 
            //         accountId, ticketCount);
            //     throw new BusinessRuleException(
            //         "Không thể xóa tài khoản đang có ticket liên quan",
            //         "ACCOUNT_HAS_DEPENDENCIES");
            // }

            // ✅ Perform delete
            if (softDelete)
            {
                // Soft delete - no transaction needed (single operation)
                account.DeletedAt = DateTime.UtcNow;
                _db.Accounts.Update(account);

                await _db.SaveChangesAsync();

                _logger.LogInformation("Soft deleted account {AccountId}:{Username}",
                    accountId, account.Username);
            }
            else
            {
                // Hard delete - use transaction for multiple operations
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    // ✅ Option 1: Rely on DB cascade delete (if configured)
                    // Just remove the account, FK constraints handle the rest
                    _db.Accounts.Remove(account);

                    // ✅ Option 2: Manual cascade (if not using DB cascade)
                    // Use ExecuteDelete for better performance
                    // await _db.AccountRoles
                    //     .Where(ar => ar.AccountId == accountId)
                    //     .ExecuteDeleteAsync();
                    // await _db.AccountClaims
                    //     .Where(ac => ac.AccountId == accountId)
                    //     .ExecuteDeleteAsync();
                    // _db.Accounts.Remove(account);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Hard deleted account {AccountId}:{Username}",
                        accountId, account.Username);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<BulkDeleteResultDto> DeleteAccountsAsync(List<Guid> accountIds, bool softDelete = true)
        {
            _logger.LogInformation("Batch delete started | Count: {Count} | SoftDelete: {SoftDelete}",
                accountIds?.Count ?? 0, softDelete);

            // ✅ Step 0: Input validation
            ArgumentNullException.ThrowIfNull(accountIds);

            if (accountIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("accountIds",
                    "Vui lòng chọn ít nhất một tài khoản để xóa");
            }

            // Remove duplicates
            var uniqueIds = accountIds.Distinct().ToList();

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // ✅ Step 1: Validate ALL items BEFORE any deletion
                var accounts = await _db.Accounts
                    .Include(a => a.Employee)
                    .Include(a => a.AccountRoles)
                    .Where(a => uniqueIds.Contains(a.AccountId) && a.DeletedAt == null)
                    .ToListAsync();

                if (accounts.Count == 0)
                {
                    throw new NotFoundException("Không tìm thấy tài khoản nào để xóa");
                }

                // Check if ALL requested IDs exist (strict validation)
                var foundIds = accounts.Select(a => a.AccountId).ToList();
                var missingIds = uniqueIds.Except(foundIds).ToList();

                if (missingIds.Any())
                {
                    _logger.LogWarning("Accounts not found: {Ids}", string.Join(", ", missingIds));
                    throw new NotFoundException(
                        $"Không tìm thấy {missingIds.Count} tài khoản: {string.Join(", ", missingIds)}");
                }

                // ✅ Step 2: Check business rules for ALL items
                var systemAccounts = accounts
                    .Where(a => a.Employee?.Position == "Super_Admin")
                    .ToList();

                if (systemAccounts.Any())
                {
                    var systemUsernames = string.Join(", ", systemAccounts.Select(a => a.Username));
                    _logger.LogWarning("System accounts detected: {Usernames}", systemUsernames);
                    throw new BusinessRuleException(
                        $"Không thể xóa tài khoản Super Admin: {systemUsernames}",
                        "CANNOT_DELETE_SUPER_ADMIN");  // ✅ Added error code
                }

                // TODO: Add check to prevent self-deletion
                // if (uniqueIds.Contains(currentUserId))
                // {
                //     throw new BusinessRuleException(
                //         "Không thể tự xóa tài khoản của chính mình",
                //         "CANNOT_DELETE_SELF");
                // }

                // ✅ Step 3: All checks passed → Delete ALL
                var deletedAt = DateTime.UtcNow;
                foreach (var account in accounts)
                {
                    if (softDelete)
                    {
                        account.DeletedAt = deletedAt;
                    }
                    else
                    {
                        // Hard delete - cascade manually if needed
                        var accountRoles = await _db.AccountRoles
                            .Where(ar => ar.AccountId == account.AccountId)
                            .ToListAsync();
                        var accountClaims = await _db.AccountClaims
                            .Where(ac => ac.AccountId == account.AccountId)
                            .ToListAsync();

                        _db.AccountRoles.RemoveRange(accountRoles);
                        _db.AccountClaims.RemoveRange(accountClaims);
                        _db.Accounts.Remove(account);
                    }

                    // ✅ Added detailed logging for audit trail
                    _logger.LogInformation("Deleted account {AccountId}:{Username} | SoftDelete: {SoftDelete}",
                        account.AccountId, account.Username, softDelete);
                }

                if (softDelete)
                {
                    _db.Accounts.UpdateRange(accounts);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Batch delete SUCCESS | Count: {Count}/{Total}",
                    accounts.Count, uniqueIds.Count);

                return new BulkDeleteResultDto
                {
                    Success = true,
                    DeletedCount = accounts.Count,
                    TotalRequested = uniqueIds.Count,
                    Message = $"Đã xóa {accounts.Count} tài khoản thành công"
                };
            }
            catch
            {
                await transaction.RollbackAsync();  // ✅ Rollback on ANY error
                throw;
            }
        }

        public async Task<AccountRolesDto> AssignRolesToAccountAsync(Guid accountId, List<int> roleIds)
        {
            AssignRolesDto assignRolesDto = new AssignRolesDto
            {
                AccountId = accountId,
                RoleIds = roleIds
            };

            _logger.LogInformation("Assigning roles to account {AccountId}", assignRolesDto.AccountId);

            var validationResult = await _assignRolesValidator.ValidateAsync(assignRolesDto);
            validationResult.ThrowIfInvalid();

            var account = await _db.Accounts
                .Include(a => a.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                .FirstOrDefaultAsync(a => a.AccountId == assignRolesDto.AccountId && a.DeletedAt == null);

            if (account is null)
            {
                throw new NotFoundException("Tài khoản", assignRolesDto.AccountId);
            }

            // Validate all roles exist
            var existingRoles = await _db.Roles
                .Where(r => assignRolesDto.RoleIds.Contains(r.RoleId) && r.DeletedAt == null)
                .Select(r => r.RoleId)
                .ToListAsync();

            var missingRoles = assignRolesDto.RoleIds.Except(existingRoles).ToList();
            if (missingRoles.Any())
            {
                throw new NotFoundException($"Roles không tồn tại: {string.Join(", ", missingRoles)}");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Get current role IDs
                var currentRoleIds = account.AccountRoles
                    .Select(ar => ar.RoleId)
                    .ToList();

                var selected = assignRolesDto.RoleIds.Distinct().ToList();

                var toAdd = selected.Except(currentRoleIds).ToList();
                var toRemove = currentRoleIds.Except(selected).ToList();

                // Add new roles
                if (toAdd.Any())
                {
                    var newAccountRoles = toAdd.Select(roleId => new AccountRoles
                    {
                        AccountId = assignRolesDto.AccountId,
                        RoleId = roleId
                    });

                    await _db.AccountRoles.AddRangeAsync(newAccountRoles);
                }

                // Remove old roles
                if (toRemove.Any())
                {
                    var removeAccountRoles = await _db.AccountRoles
                        .Where(ar => ar.AccountId == assignRolesDto.AccountId && toRemove.Contains(ar.RoleId))
                        .ToListAsync();

                    _db.AccountRoles.RemoveRange(removeAccountRoles);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ Invalidate permission cache (already has field now)
                _authorizationService.InvalidatePermissionCache(assignRolesDto.AccountId);
                _logger.LogInformation(
                    "Invalidated permission cache for account {AccountId} after role assignment",
                    assignRolesDto.AccountId);

                _logger.LogInformation("Successfully assigned {Count} roles to account {AccountId}",
                    assignRolesDto.RoleIds.Count, assignRolesDto.AccountId);

                // Return updated account with roles
                var updatedRoles = await _roleMapper.ProjectToRoleDto(_db.Roles
                    .Where(r => assignRolesDto.RoleIds.Contains(r.RoleId))
                    .AsNoTracking())
                    .ToListAsync();

                return new AccountRolesDto
                {
                    AccountId = assignRolesDto.AccountId,
                    Username = account.Username,
                    Roles = updatedRoles
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AccountClaimsDto> AssignClaimsToAccountAsync(Guid accountId, List<int> claimIds)
        {
            AssignClaimsDto assignClaimsDto = new AssignClaimsDto
            {
                AccountId = accountId,
                ClaimIds = claimIds
            };

            _logger.LogInformation("Assigning direct claims to account {AccountId}", assignClaimsDto.AccountId);

            var validationResult = await _assignClaimsValidator.ValidateAsync(assignClaimsDto);
            validationResult.ThrowIfInvalid();

            var account = await _db.Accounts
                .Include(a => a.AccountClaims)
                .FirstOrDefaultAsync(a => a.AccountId == assignClaimsDto.AccountId && a.DeletedAt == null);

            if (account is null)
            {
                throw new NotFoundException("Tài khoản", assignClaimsDto.AccountId);
            }

            var existingClaimIds = await _db.Claims
                .Where(c => assignClaimsDto.ClaimIds.Contains(c.ClaimId))
                .Select(c => c.ClaimId)
                .ToListAsync();

            var missingClaimIds = assignClaimsDto.ClaimIds.Except(existingClaimIds).ToList();
            if (missingClaimIds.Any())
            {
                throw new NotFoundException($"Claims không tồn tại: {string.Join(", ", missingClaimIds)}");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var currentClaimIds = account.AccountClaims
                    .Select(ac => ac.ClaimId)
                    .ToList();

                var selected = assignClaimsDto.ClaimIds.Distinct().ToList();

                var toAdd = selected.Except(currentClaimIds).ToList();
                var toRemove = currentClaimIds.Except(selected).ToList();

                if (toAdd.Any())
                {
                    var newAccountClaims = toAdd.Select(claimId => new AccountClaims
                    {
                        AccountId = assignClaimsDto.AccountId,
                        ClaimId = claimId
                    });

                    await _db.AccountClaims.AddRangeAsync(newAccountClaims);
                }

                if (toRemove.Any())
                {
                    var removeAccountClaims = await _db.AccountClaims
                        .Where(ac => ac.AccountId == assignClaimsDto.AccountId && toRemove.Contains(ac.ClaimId))
                        .ToListAsync();

                    _db.AccountClaims.RemoveRange(removeAccountClaims);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _authorizationService.InvalidatePermissionCache(assignClaimsDto.AccountId);
                _logger.LogInformation(
                    "Invalidated permission cache for account {AccountId} after direct claim assignment",
                    assignClaimsDto.AccountId);

                _logger.LogInformation("Successfully assigned {Count} direct claims to account {AccountId}",
                    selected.Count, assignClaimsDto.AccountId);

                var updatedClaims = await _roleMapper.ProjectToClaimDto(_db.Claims
                    .Where(c => selected.Contains(c.ClaimId))
                    .AsNoTracking())
                    .ToListAsync();

                return new AccountClaimsDto
                {
                    AccountId = assignClaimsDto.AccountId,
                    Username = account.Username,
                    Claims = updatedClaims
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ResetPasswordResultDto> ResetPasswordAsync(Guid accountId)
        {
            _logger.LogInformation("Resetting password for account {AccountId}", accountId);

            var account = await _db.Accounts
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.DeletedAt == null);

            if (account is null)
            {
                throw new NotFoundException("Tài khoản", accountId);
            }

            var tempPassword = RandomString.GenerateRandomString(10);

            account.Password = PasswordHelper.HashPassword(tempPassword);
            account.FailedLoginAttempts = 0;
            account.IsLocked = false;
            account.LockedUntil = null;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Successfully reset password for account {AccountId}", accountId);

            return new ResetPasswordResultDto
            {
                TemporaryPassword = tempPassword,
                Message = $"Mật khẩu tạm thời cho {account.Username}: {tempPassword}. Vui lòng gửi cho nhân viên và yêu cầu đổi mật khẩu ngay."
            };
        }

        public async Task<AccountDto> LockAccountAsync(Guid accountId, DateTime? lockUntil = null)
        {
            _logger.LogInformation("Locking account {AccountId}", accountId);

            var account = await _db.Accounts.FindAsync(accountId);

            if (account is null || account.DeletedAt != null)
            {
                throw new NotFoundException("Tài khoản", accountId);
            }

            account.IsLocked = true;
            account.LockedUntil = lockUntil ?? DateTime.UtcNow.AddMinutes(LockoutMinutes);

            await _db.SaveChangesAsync();

            _logger.LogInformation("Account {AccountId} locked until {LockedUntil}",
                accountId, account.LockedUntil);

            return await GetAccountByIdAsync(accountId);
        }

        public async Task<AccountDto> UnlockAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Unlocking account {AccountId}", accountId);

            var account = await _db.Accounts.FindAsync(accountId);

            if (account is null || account.DeletedAt != null)
            {
                throw new NotFoundException("Tài khoản", accountId);
            }

            account.IsLocked = false;
            account.FailedLoginAttempts = 0;
            account.LockedUntil = null;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Account {AccountId} unlocked", accountId);

            return await GetAccountByIdAsync(accountId);
        }

        public async Task ChangePasswordAsync(Guid accountId, ChangePasswordDto dto)
        {
            _logger.LogInformation("Changing password for account {AccountId}", accountId);

            var validationResult = await _changePasswordValidator.ValidateAsync(dto);
            validationResult.ThrowIfInvalid();

            var account = await _db.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.DeletedAt == null);

            if (account is null)
            {
                throw new NotFoundException("Tài khoản", accountId);
            }

            if (!PasswordHelper.VerifyPassword(dto.CurrentPassword, account.Password))
            {
                throw new BusinessRuleException("Mật khẩu hiện tại không đúng");
            }

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                throw new Shared.Exceptions.ValidationException("ConfirmPassword",
                    "Mật khẩu xác nhận không khớp");
            }

            if (PasswordHelper.VerifyPassword(dto.NewPassword, account.Password))
            {
                throw new BusinessRuleException("Mật khẩu mới phải khác mật khẩu hiện tại");
            }

            account.Password = PasswordHelper.HashPassword(dto.NewPassword);

            await _db.SaveChangesAsync();

            _logger.LogInformation("Successfully changed password for account {AccountId}", accountId);
        }

        public async Task<List<LoginHistoryDto>> GetLoginHistoryAsync(Guid accountId)
        {
            _logger.LogInformation("Fetching login history for account {AccountId}", accountId);

            var accountExists = await _db.Accounts
                .AnyAsync(a => a.AccountId == accountId && a.DeletedAt == null);

            if (!accountExists)
            {
                throw new NotFoundException("Tài khoản", accountId);
            }

            var history = await _db.AccountTokens
                .AsNoTracking()
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(50)
                .Select(t => new LoginHistoryDto
                {
                    LoginAt = t.CreatedAt,
                    IpAddress = t.IpAddress,
                    UserAgent = t.UserAgent,
                    Success = true,
                    FailureReason = null
                })
                .ToListAsync();

            return history;
        }

        public async Task RecordLoginAttemptAsync(Guid accountId, bool success, string? ipAddress = null)
        {
            var account = await _db.Accounts.FindAsync(accountId);

            if (account is null) return;

            if (success)
            {
                account.FailedLoginAttempts = 0;
                account.LastLoginAt = DateTime.UtcNow;
                account.IsLocked = false;
                account.LockedUntil = null;
            }
            else
            {
                account.FailedLoginAttempts++;

                if (account.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    account.IsLocked = true;
                    account.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    _logger.LogWarning("Account {AccountId} locked due to {Attempts} failed attempts",
                        accountId, account.FailedLoginAttempts);
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<bool> IsAccountLockedAsync(Guid accountId)
        {
            var account = await _db.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account is null)
            {
                return false;
            }

            if (!account.IsLocked)
            {
                return false;
            }

            if (account.LockedUntil.HasValue && account.LockedUntil.Value < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        // ✅ ADD THIS METHOD
        /// <summary>
        /// Get all permissions for an account
        /// Pattern: Flattened permission list for frontend consumption
        /// Reference: Auth0 RBAC, Azure AD App Roles
        /// </summary>
        public async Task<List<string>> GetPermissionsAsync(Guid accountId)
        {
            _logger.LogInformation("Fetching permissions for account {AccountId}", accountId);

            var rolePermissions = _db.Accounts
                .AsNoTracking()
                .Where(a => a.AccountId == accountId && a.DeletedAt == null)
                .SelectMany(a => a.AccountRoles
                    .Where(ar => ar.Role.DeletedAt == null)
                    .SelectMany(ar => ar.Role.RoleClaims
                        .Select(rc => rc.Claim.Claim)));

            var directPermissions = _db.AccountClaims
                .AsNoTracking()
                .Where(ac => ac.AccountId == accountId)
                .Select(ac => ac.Claim.Claim);

            var permissions = await rolePermissions
                .Concat(directPermissions)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

            permissions = permissions
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            _logger.LogInformation("Retrieved {Count} permissions for account {AccountId}",
                permissions.Count, accountId);

            return permissions;
        }
    }
}
