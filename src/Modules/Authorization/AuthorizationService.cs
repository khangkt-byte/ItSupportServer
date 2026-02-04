using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Authorization service implementation
    /// Pattern: Policy-based authorization with caching (RBAC + ABAC)
    /// Performance: In-memory cache (30 min absolute, 15 min sliding)
    /// Security: Admin bypass, fail-closed on errors, batch invalidation
    /// Reference: Microsoft.AspNetCore.Authorization, OWASP Authorization
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthorizationService> _logger;

        // ===== CONSTANTS =====

        /// <summary>
        /// Cache duration (30 minutes for production performance)
        /// Pattern: Microsoft Azure recommendations (15-60 minutes for auth data)
        /// Source: https://learn.microsoft.com/en-us/azure/architecture/best-practices/caching
        /// </summary>
        private const int CacheDurationMinutes = 30;

        /// <summary>
        /// Sliding expiration (15 minutes - refresh if accessed)
        /// Pattern: Keep active users cached, expire inactive
        /// </summary>
        private const int CacheSlidingMinutes = 15;

        // ===== CONSTRUCTOR =====

        public AuthorizationService(
            AppDbContext db,
            IMemoryCache cache,
            ILogger<AuthorizationService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        // ===== PERMISSION CHECK METHODS (Return bool) =====

        /// <inheritdoc/>
        public async Task<bool> HasPermissionAsync(
            Guid accountId,
            string permission,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(permission);

            _logger.LogDebug(
                "Checking permission '{Permission}' for account {AccountId}",
                permission, accountId);

            try
            {
                // Get all permissions (cached)
                var userPermissions = await GetUserPermissionsCachedAsync(accountId, cancellationToken);

                // ✅ USE constant from Permissions.cs (DRY)
                if (userPermissions.Contains(Permissions.AdminClaim))
                {
                    _logger.LogDebug("Admin access granted for {AccountId}", accountId);
                    return true;
                }

                // Check specific permission
                var hasPermission = userPermissions.Contains(permission);

                if (!hasPermission)
                {
                    _logger.LogWarning(
                        "Permission denied: '{Permission}' for account {AccountId}",
                        permission, accountId);
                }

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking permission '{Permission}' for account {AccountId}",
                    permission, accountId);

                // Fail-closed: Deny on error (OWASP security best practice)
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HasAnyPermissionAsync(
            Guid accountId,
            string[] permissions,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(permissions);

            if (permissions.Length == 0)
            {
                _logger.LogWarning("HasAnyPermissionAsync called with empty permissions");
                return false;
            }

            _logger.LogDebug(
                "Checking ANY of {Count} permissions for account {AccountId}",
                permissions.Length, accountId);

            try
            {
                var userPermissions = await GetUserPermissionsCachedAsync(accountId, cancellationToken);

                // ✅ USE constant
                if (userPermissions.Contains(Permissions.AdminClaim))
                {
                    _logger.LogDebug("Admin access granted for {AccountId}", accountId);
                    return true;
                }

                var hasAny = permissions.Any(p => userPermissions.Contains(p));

                if (!hasAny)
                {
                    _logger.LogWarning(
                        "Account {AccountId} denied - missing all permissions: {Permissions}",
                        accountId, string.Join(", ", permissions));
                }

                return hasAny;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking ANY permissions for account {AccountId}",
                    accountId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HasAllPermissionsAsync(
            Guid accountId,
            string[] permissions,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(permissions);

            if (permissions.Length == 0)
            {
                _logger.LogWarning("HasAllPermissionsAsync called with empty permissions");
                return true; // Vacuous truth
            }

            _logger.LogDebug(
                "Checking ALL of {Count} permissions for account {AccountId}",
                permissions.Length, accountId);

            try
            {
                var userPermissions = await GetUserPermissionsCachedAsync(accountId, cancellationToken);

                // ✅ USE constant
                if (userPermissions.Contains(Permissions.AdminClaim))
                {
                    _logger.LogDebug("Admin access granted for {AccountId}", accountId);
                    return true;
                }

                var missingPermissions = permissions.Where(p => !userPermissions.Contains(p)).ToList();

                if (missingPermissions.Count > 0)
                {
                    _logger.LogWarning(
                        "Account {AccountId} denied - missing permissions: {MissingPermissions}",
                        accountId, string.Join(", ", missingPermissions));
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking ALL permissions for account {AccountId}",
                    accountId);
                return false;
            }
        }

        // ===== PERMISSION CHECK WITH EXCEPTIONS =====

        /// <inheritdoc/>
        public async Task EnsureHasPermissionAsync(
            Guid accountId,
            string permission,
            CancellationToken cancellationToken = default)
        {
            var hasPermission = await HasPermissionAsync(accountId, permission, cancellationToken);

            if (!hasPermission)
            {
                throw new ForbiddenException(
                    $"Bạn không có quyền '{permission}' để thực hiện thao tác này");
            }
        }

        /// <inheritdoc/>
        public async Task EnsureHasAnyPermissionAsync(
            Guid accountId,
            string[] permissions,
            CancellationToken cancellationToken = default)
        {
            var hasAny = await HasAnyPermissionAsync(accountId, permissions, cancellationToken);

            if (!hasAny)
            {
                throw new ForbiddenException(
                    $"Bạn không có bất kỳ quyền nào trong: {string.Join(", ", permissions)}");
            }
        }

        /// <inheritdoc/>
        public async Task EnsureHasAllPermissionsAsync(
            Guid accountId,
            string[] permissions,
            CancellationToken cancellationToken = default)
        {
            var hasAll = await HasAllPermissionsAsync(accountId, permissions, cancellationToken);

            if (!hasAll)
            {
                var userPermissions = await GetUserPermissionsCachedAsync(accountId, cancellationToken);
                var missingPermissions = permissions.Where(p => !userPermissions.Contains(p)).ToList();

                throw new ForbiddenException(
                    $"Bạn thiếu quyền: {string.Join(", ", missingPermissions)}");
            }
        }

        // ===== PERMISSION RETRIEVAL =====

        /// <inheritdoc/>
        public async Task<List<string>> GetUserPermissionsAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all permissions for account {AccountId}", accountId);

            return await GetUserPermissionsCachedAsync(accountId, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> IsAdminAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Checking if account {AccountId} is admin", accountId);

            try
            {
                var permissions = await GetUserPermissionsCachedAsync(accountId, cancellationToken);
                
                // ✅ USE constant
                var isAdmin = permissions.Contains(Permissions.AdminClaim);

                _logger.LogDebug(
                    "Account {AccountId} admin status: {IsAdmin}",
                    accountId, isAdmin);

                return isAdmin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking admin status for account {AccountId}", accountId);
                return false; // Fail-closed
            }
        }

        // ===== CACHE INVALIDATION =====

        /// <inheritdoc/>
        public void ClearPermissionCache(Guid accountId)
        {
            var cacheKey = GetCacheKey(accountId);
            _cache.Remove(cacheKey);

            _logger.LogInformation(
                "Permission cache cleared for account {AccountId}",
                accountId);
        }

        /// <inheritdoc/>
        public void ClearPermissionCache(IEnumerable<Guid> accountIds)
        {
            var count = 0;

            foreach (var accountId in accountIds)
            {
                var cacheKey = GetCacheKey(accountId);
                _cache.Remove(cacheKey);
                count++;
            }

            _logger.LogInformation(
                "Permission cache cleared for {Count} accounts (batch operation)",
                count);
        }

        /// <inheritdoc/>
        public void InvalidatePermissionCache(Guid accountId)
        {
            ClearPermissionCache(accountId);
        }

        /// <inheritdoc/>
        public void InvalidatePermissionCache(IEnumerable<Guid> accountIds)
        {
            ClearPermissionCache(accountIds);
        }

        // ===== PRIVATE HELPER METHODS =====

        /// <summary>
        /// Get user permissions with caching
        /// Pattern: Cache-aside pattern (Microsoft Azure pattern)
        /// Performance: 1 DB query cached for 30 minutes (15 min sliding)
        /// Source: https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside
        /// </summary>
        private async Task<List<string>> GetUserPermissionsCachedAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = GetCacheKey(accountId);

            // Try get from cache
            if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions))
            {
                _logger.LogDebug("Cache HIT for account {AccountId}", accountId);
                return cachedPermissions!;
            }

            _logger.LogDebug("Cache MISS for account {AccountId}, fetching from DB", accountId);

            // Load from database
            var permissions = await LoadUserPermissionsFromDbAsync(accountId, cancellationToken);

            // Cache with optimized expiration strategy
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(CacheSlidingMinutes),
                Priority = CacheItemPriority.High // High priority for auth data
            };

            _cache.Set(cacheKey, permissions, cacheOptions);

            _logger.LogInformation(
                "Loaded {Count} permissions for account {AccountId} (cached for {Duration} min)",
                permissions.Count, accountId, CacheDurationMinutes);

            return permissions;
        }

        /// <summary>
        /// Load permissions from database
        /// Pattern: Single query with eager loading (N+1 prevention)
        /// Includes: Direct account claims + Role-based claims
        /// Performance: Single SQL query with LEFT JOINs
        /// Security: Filters deleted accounts/roles (OWASP best practice)
        /// </summary>
        private async Task<List<string>> LoadUserPermissionsFromDbAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            var permissions = new HashSet<string>();

            // Single optimized query with eager loading
            var account = await _db.Accounts
                .AsNoTracking()
                .Include(a => a.AccountClaims)
                    .ThenInclude(ac => ac.Claim)
                .Include(a => a.AccountRoles)
                    .ThenInclude(ar => ar.Role)
                        .ThenInclude(r => r.RoleClaims)
                            .ThenInclude(rc => rc.Claim)
                .Where(a => a.AccountId == accountId && a.DeletedAt == null)
                .FirstOrDefaultAsync(cancellationToken);

            // Account not found or deleted
            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found or deleted", accountId);
                throw new NotFoundException("Tài khoản", accountId);
            }

            var directClaimCount = 0;

            // Collect direct account claims
            foreach (var accountClaim in account.AccountClaims)
            {
                if (!string.IsNullOrEmpty(accountClaim.Claim?.Claim))
                {
                    permissions.Add(accountClaim.Claim.Claim);
                    directClaimCount++;
                }
            }

            // Collect role-based claims (only from active roles)
            foreach (var accountRole in account.AccountRoles)
            {
                // Skip deleted roles (security: prevent using deleted roles)
                if (accountRole.Role?.DeletedAt != null)
                    continue;

                foreach (var roleClaim in accountRole.Role.RoleClaims)
                {
                    if (!string.IsNullOrEmpty(roleClaim.Claim?.Claim))
                    {
                        permissions.Add(roleClaim.Claim.Claim);
                    }
                }
            }

            var roleClaimCount = permissions.Count - directClaimCount;

            _logger.LogDebug(
                "Loaded {DirectClaims} direct + {RoleClaims} role = {Total} permissions for account {AccountId}",
                directClaimCount, roleClaimCount, permissions.Count, accountId);

            return permissions.ToList();
        }

        /// <summary>
        /// Generate cache key for account permissions
        /// Pattern: Centralized key generation (DRY principle)
        /// Format: "UserPermissions_{accountId}"
        /// </summary>
        private static string GetCacheKey(Guid accountId)
        {
            return $"UserPermissions_{accountId}";
        }
    }
}