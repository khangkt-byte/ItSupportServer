using FluentValidation;
using ItSupportServer.Data.Models;
using ItSupportServer.Data.Models.Entities;
using ItSupportServer.src.Shared.Base;
using ItSupportServer.src.Shared.Exceptions;
using ItSupportServer.src.Shared.Extensions;
using ItSupportServer.src.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.Account
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _db;
        private readonly AccountMapper _mapper;
        private readonly ILogger<AccountService> _logger;
        private readonly IValidator<CreateAccountDto> _createValidator;
        private readonly IValidator<UpdateAccountDto> _updateValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;

        // Constants
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 30;

        public AccountService(
            AppDbContext db,
            AccountMapper mapper,
            ILogger<AccountService> logger,
            IValidator<CreateAccountDto> createValidator,
            IValidator<UpdateAccountDto> updateValidator,
            IValidator<ChangePasswordDto> changePasswordValidator)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _changePasswordValidator = changePasswordValidator;
        }

        public async Task<PaginatedResult<ListAccountDto>> GetAccountsAsync(QueryParameters parameters)
        {
            _logger.LogInformation("Fetching accounts with search: {Search}, page: {Page}",
                parameters.Search, parameters.Page);

            var query = _mapper.ProjectToListAccountDto(_db.Accounts
                .AsNoTracking());

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(a =>
                    a.Username.Contains(parameters.Search) ||
                    (a.EmpName != null && a.EmpName.Contains(parameters.Search)) ||
                    (a.EmpCode != null && a.EmpCode.Contains(parameters.Search)));
            }

            var result = await query.ToPaginatedResultAsync(parameters, defaultSortField: "CreatedAt");

            _logger.LogInformation("Retrieved {Count} accounts", result.TotalCount);

            return result;
        }

        public async Task<AccountDto> GetAccountByIdAsync(Guid accountId)
        {
            _logger.LogInformation("Fetching account {AccountId}", accountId);

            var account = await _mapper.ProjectToAccountDto(_db.Accounts
                .Include(a => a.Employee)
                .Include(a => a.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                    .ThenInclude(r => r.RoleClaims)
                    .ThenInclude(rc => rc.Claim)
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

            if (hasChanges)
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Successfully updated account {AccountId}", accountId);
            }

            return await GetAccountByIdAsync(accountId);
        }

        public async Task<bool> DeleteAccountsAsync(List<Guid> accountIds)
        {
            _logger.LogInformation("Deleting {Count} accounts", accountIds?.Count ?? 0);

            ArgumentNullException.ThrowIfNull(accountIds);

            if (accountIds.Count == 0)
            {
                throw new Shared.Exceptions.ValidationException("accountIds",
                    "Vui lòng chọn tài khoản để xóa");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var existing = await _db.Accounts
                    .Include(a => a.Employee)
                    .Where(a => accountIds.Contains(a.AccountId) && a.DeletedAt == null)
                    .ToListAsync();

                if (existing.Count == 0)
                {
                    throw new NotFoundException("Không tìm thấy tài khoản để xóa");
                }

                var systemAccounts = existing.Where(a =>
                    a.Employee?.Position == "Super_Admin").ToList();

                if (systemAccounts.Any())
                {
                    throw new BusinessRuleException("Không thể xóa tài khoản Super Admin");
                }

                foreach (var account in existing)
                {
                    account.DeletedAt = DateTime.UtcNow;
                }

                _db.Accounts.UpdateRange(existing);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully deleted {Count} accounts", existing.Count);

                return true;
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

            var tempPassword = GenerateTemporaryPassword();

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

            // TODO: Implement LoginHistory table and query
            var history = new List<LoginHistoryDto>();

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
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.DeletedAt == null);

            if (account is null) return true;

            // Check if temporary lock has expired
            if (account.IsLocked && account.LockedUntil.HasValue && account.LockedUntil.Value < DateTime.UtcNow)
            {
                // Auto-unlock if lock period expired
                account.IsLocked = false;
                account.LockedUntil = null;
                account.FailedLoginAttempts = 0;
                _db.Accounts.Update(account);
                await _db.SaveChangesAsync();
                return false;
            }

            return account.IsLocked;
        }

        private static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
