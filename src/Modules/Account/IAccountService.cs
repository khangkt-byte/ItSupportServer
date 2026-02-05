using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.Account
{
    public interface IAccountService
    {
        /// <summary>
        /// Get paginated list of accounts
        /// </summary>
        Task<PaginatedResult<ListAccountDto>> GetAccountsAsync(QueryParameters parameters);

        /// <summary>
        /// Get account by ID with full details
        /// </summary>
        Task<AccountDto> GetAccountByIdAsync(Guid accountId);

        /// <summary>
        /// Create new account for employee
        /// </summary>
        Task<AccountDto> CreateAccountAsync(CreateAccountDto dto);

        /// <summary>
        /// Update account (username, lock status)
        /// </summary>
        Task<AccountDto> UpdateAccountAsync(Guid accountId, UpdateAccountDto dto);

        /// <summary>
        /// Delete single account (soft delete)
        /// </summary>
        Task DeleteAccountAsync(Guid accountId, bool softDelete = true);

        /// <summary>
        /// Delete multiple accounts (soft delete)
        /// </summary>
        Task<BulkDeleteResultDto> DeleteAccountsAsync(List<Guid> accountIds, bool softDelete = true);

        /// <summary>
        /// Admin reset password for user
        /// </summary>
        Task<ResetPasswordResultDto> ResetPasswordAsync(Guid accountId);

        /// <summary>
        /// Lock account (manual lock by admin)
        /// </summary>
        Task<AccountDto> LockAccountAsync(Guid accountId, DateTime? lockUntil = null);

        /// <summary>
        /// Unlock account
        /// </summary>
        Task<AccountDto> UnlockAccountAsync(Guid accountId);

        /// <summary>
        /// Change own password (self-service)
        /// </summary>
        Task ChangePasswordAsync(Guid accountId, ChangePasswordDto dto);

        /// <summary>
        /// Get login history for account
        /// </summary>
        Task<List<LoginHistoryDto>> GetLoginHistoryAsync(Guid accountId);

        /// <summary>
        /// Record login attempt (for authentication service)
        /// </summary>
        Task RecordLoginAttemptAsync(Guid accountId, bool success, string? ipAddress = null);

        /// <summary>
        /// Check if account is locked
        /// </summary>
        Task<bool> IsAccountLockedAsync(Guid accountId);
    }
}
