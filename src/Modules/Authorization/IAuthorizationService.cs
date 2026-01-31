namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Authorization service interface
    /// Pattern: Policy-based authorization (Microsoft.AspNetCore.Authorization)
    /// Reference: ASP.NET Core Identity, Clean Architecture, OWASP Authorization
    /// </summary>
    public interface IAuthorizationService
    {
        // ===== PERMISSION CHECKS (Return bool) =====

        /// <summary>
        /// Check if user has specific permission
        /// Use in: Authorization handlers, conditional features
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="permission">Permission string (e.g., "Department.View")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if user has permission or is Admin</returns>
        Task<bool> HasPermissionAsync(
            Guid accountId,
            string permission,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if user has ANY of the specified permissions (OR logic)
        /// </summary>
        Task<bool> HasAnyPermissionAsync(
            Guid accountId,
            string[] permissions,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if user has ALL of the specified permissions (AND logic)
        /// </summary>
        Task<bool> HasAllPermissionsAsync(
            Guid accountId,
            string[] permissions,
            CancellationToken cancellationToken = default);

        // ===== PERMISSION CHECKS (Throw exceptions) =====

        /// <summary>
        /// Check permission and throw ForbiddenException if denied
        /// Use in: Business logic where you want to halt execution
        /// Pattern: Guard clause pattern
        /// </summary>
        Task EnsureHasPermissionAsync(
            Guid accountId,
            string permission,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check ANY permission and throw ForbiddenException if all denied
        /// </summary>
        Task EnsureHasAnyPermissionAsync(
            Guid accountId,
            string[] permissions,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check ALL permissions and throw ForbiddenException if any missing
        /// </summary>
        Task EnsureHasAllPermissionsAsync(
            Guid accountId,
            string[] permissions,
            CancellationToken cancellationToken = default);

        // ===== PERMISSION RETRIEVAL =====

        /// <summary>
        /// Get all permissions for a user (includes direct + role-based)
        /// Use in: Profile API, admin panels
        /// </summary>
        Task<List<string>> GetUserPermissionsAsync(
            Guid accountId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if user is admin (has Admin claim)
        /// Use in: Admin-only features, bypass logic
        /// </summary>
        Task<bool> IsAdminAsync(
            Guid accountId,
            CancellationToken cancellationToken = default);

        // ===== CACHE MANAGEMENT =====

        /// <summary>
        /// Clear permission cache for specific user
        /// Call when: User permissions changed (claim added/removed)
        /// </summary>
        void ClearPermissionCache(Guid accountId);

        /// <summary>
        /// Clear permission cache for multiple users (batch)
        /// Call when: Role permissions changed (affects all users with that role)
        /// </summary>
        void ClearPermissionCache(IEnumerable<Guid> accountIds);

        /// <summary>
        /// Invalidate permission cache for specific user (alias)
        /// Alias for ClearPermissionCache with more descriptive name
        /// </summary>
        void InvalidatePermissionCache(Guid accountId);

        /// <summary>
        /// Invalidate permission cache for multiple users (alias)
        /// </summary>
        void InvalidatePermissionCache(IEnumerable<Guid> accountIds);
    }
}