namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Service for authorization and permission checks
    /// Pattern: Interface segregation (SOLID)
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Check if account has a specific permission (claim)
        /// Returns false instead of throwing exception (for authorization handlers)
        /// </summary>
        /// <param name="accountId">Account ID to check</param>
        /// <param name="claimType">Permission/claim type (e.g., "Employee.View")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if account has permission, false otherwise</returns>
        Task<bool> HasPermissionAsync(
            Guid accountId,
            string claimType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if account has permission and throw exception if not
        /// Use this in business logic where you want to throw ForbiddenException
        /// </summary>
        Task EnsureHasPermissionAsync(
            Guid accountId,
            string claimType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if account has ANY of the specified permissions
        /// </summary>
        Task<bool> HasAnyPermissionAsync(
            Guid accountId,
            string[] claimTypes,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if account has ANY of the specified permissions and throw if not
        /// </summary>
        Task EnsureHasAnyPermissionAsync(
            Guid accountId,
            string[] claimTypes,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if account has ALL of the specified permissions
        /// </summary>
        Task<bool> HasAllPermissionsAsync(
            Guid accountId,
            string[] claimTypes,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if account has ALL of the specified permissions and throw if not
        /// </summary>
        Task EnsureHasAllPermissionsAsync(
            Guid accountId,
            string[] claimTypes,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all permissions for an account (direct + role-based)
        /// </summary>
        Task<List<string>> GetAccountPermissionsAsync(
            Guid accountId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if account is admin (has "Admin" claim)
        /// </summary>
        Task<bool> IsAdminAsync(
            Guid accountId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidate permission cache for an account
        /// Call this when account permissions change
        /// </summary>
        void InvalidatePermissionCache(Guid accountId);

        /// <summary>
        /// Invalidate permission cache for multiple accounts
        /// </summary>
        void InvalidatePermissionCache(IEnumerable<Guid> accountIds);
    }
}