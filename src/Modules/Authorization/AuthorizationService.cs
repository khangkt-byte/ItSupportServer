using ItSupportServer.Data.Models;
using ItSupportServer.src.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Authorization service implementation
    /// Pattern: Service layer with caching for performance
    /// Security: Permission-based authorization (RBAC + ABAC)
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthorizationService> _logger;

        // Constants
        private const string AdminClaim = "Admin";
        private const string CacheKeyPrefix = "permissions:";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public AuthorizationService(
            AppDbContext db,
            IMemoryCache cache,
            ILogger<AuthorizationService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> HasPermissionAsync(
            Guid accountId,
            string claimType,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(claimType);

            _logger.LogDebug(
                "Checking permission '{ClaimType}' for account {AccountId}",
                claimType, accountId);

            try
            {
                // Get all permissions (cached)
                var permissions = await GetAccountPermissionsAsync(accountId, cancellationToken);

                // Check if has Admin claim (superuser)
                if (permissions.Contains(AdminClaim))
                {
                    _logger.LogDebug(
                        "Account {AccountId} has Admin claim - permission granted",
                        accountId);
                    return true;
                }

                // Check specific permission
                if (permissions.Contains(claimType))
                {
                    _logger.LogDebug(
                        "Account {AccountId} has permission '{ClaimType}'",
                        accountId, claimType);
                    return true;
                }

                _logger.LogWarning(
                    "Account {AccountId} denied permission '{ClaimType}'",
                    accountId, claimType);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking permission '{ClaimType}' for account {AccountId}",
                    claimType, accountId);
                
                // Return false on error (fail secure)
                return false;
            }
        }

        /// <summary>
        /// Check if account has permission and throw exception if not
        /// Use this in business logic, NOT in authorization handlers
        /// </summary>
        public async Task EnsureHasPermissionAsync(
            Guid accountId,
            string claimType,
            CancellationToken cancellationToken = default)
        {
            var hasPermission = await HasPermissionAsync(accountId, claimType, cancellationToken);
            
            if (!hasPermission)
            {
                throw new ForbiddenException(
                    $"Bạn không có quyền '{claimType}' để thực hiện thao tác này");
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HasAnyPermissionAsync(
            Guid accountId,
            string[] claimTypes,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(claimTypes);

            if (claimTypes.Length == 0)
            {
                return false;
            }

            _logger.LogDebug(
                "Checking ANY permission {Claims} for account {AccountId}",
                string.Join(", ", claimTypes), accountId);

            try
            {
                // Get all permissions (cached)
                var permissions = await GetAccountPermissionsAsync(accountId, cancellationToken);

                // Admin has all permissions
                if (permissions.Contains(AdminClaim))
                {
                    _logger.LogDebug(
                        "Account {AccountId} has Admin claim - permission granted",
                        accountId);
                    return true;
                }

                // Check if has any of the required permissions
                var hasAny = claimTypes.Any(claim => permissions.Contains(claim));

                if (hasAny)
                {
                    _logger.LogDebug(
                        "Account {AccountId} has at least one required permission",
                        accountId);
                    return true;
                }

                _logger.LogWarning(
                    "Account {AccountId} denied - missing all permissions: {Claims}",
                    accountId, string.Join(", ", claimTypes));

                return false;
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
        public async Task EnsureHasAnyPermissionAsync(
            Guid accountId,
            string[] claimTypes,
            CancellationToken cancellationToken = default)
        {
            var hasAny = await HasAnyPermissionAsync(accountId, claimTypes, cancellationToken);
            
            if (!hasAny)
            {
                throw new ForbiddenException(
                    $"Bạn không có bất kỳ quyền nào trong: {string.Join(", ", claimTypes)}");
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HasAllPermissionsAsync(
            Guid accountId,
            string[] claimTypes,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(claimTypes);

            if (claimTypes.Length == 0)
            {
                return true; // Vacuous truth
            }

            _logger.LogDebug(
                "Checking ALL permissions {Claims} for account {AccountId}",
                string.Join(", ", claimTypes), accountId);

            try
            {
                // Get all permissions (cached)
                var permissions = await GetAccountPermissionsAsync(accountId, cancellationToken);

                // Admin has all permissions
                if (permissions.Contains(AdminClaim))
                {
                    _logger.LogDebug(
                        "Account {AccountId} has Admin claim - permission granted",
                        accountId);
                    return true;
                }

                // Check if has all required permissions
                var missingClaims = claimTypes.Where(claim => !permissions.Contains(claim)).ToList();

                if (missingClaims.Count == 0)
                {
                    _logger.LogDebug(
                        "Account {AccountId} has all required permissions",
                        accountId);
                    return true;
                }

                _logger.LogWarning(
                    "Account {AccountId} denied - missing permissions: {MissingClaims}",
                    accountId, string.Join(", ", missingClaims));

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking ALL permissions for account {AccountId}",
                    accountId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task EnsureHasAllPermissionsAsync(
            Guid accountId,
            string[] claimTypes,
            CancellationToken cancellationToken = default)
        {
            var hasAll = await HasAllPermissionsAsync(accountId, claimTypes, cancellationToken);
            
            if (!hasAll)
            {
                var permissions = await GetAccountPermissionsAsync(accountId, cancellationToken);
                var missingClaims = claimTypes.Where(claim => !permissions.Contains(claim)).ToList();
                
                throw new ForbiddenException(
                    $"Bạn thiếu quyền: {string.Join(", ", missingClaims)}");
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetAccountPermissionsAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}{accountId}";

            // Try get from cache
            if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions))
            {
                _logger.LogDebug(
                    "Retrieved {Count} cached permissions for account {AccountId}",
                    cachedPermissions!.Count, accountId);
                return cachedPermissions;
            }

            _logger.LogDebug("Loading permissions from database for account {AccountId}", accountId);

            // Verify account exists and is active
            var accountExists = await _db.Accounts
                .AsNoTracking()
                .AnyAsync(a => a.AccountId == accountId && a.DeletedAt == null, cancellationToken);

            if (!accountExists)
            {
                _logger.LogWarning("Account {AccountId} not found or deleted", accountId);
                throw new NotFoundException("Tài khoản", accountId);
            }

            // Get all permissions (direct claims + role claims)
            var permissions = await _db.Accounts
                .AsNoTracking()
                .Where(a => a.AccountId == accountId && a.DeletedAt == null)
                .SelectMany(a =>
                    // Direct account claims
                    a.AccountClaims
                        .Where(ac => ac.Claim != null)
                        .Select(ac => ac.Claim.Claim)
                    .Concat(
                        // Role-based claims
                        a.AccountRoles
                            .Where(ar => ar.Role.DeletedAt == null)
                            .SelectMany(ar =>
                                ar.Role.RoleClaims
                                    .Where(rc => rc.Claim != null)
                                    .Select(rc => rc.Claim.Claim)
                            )
                    ))
                .Distinct()
                .ToListAsync(cancellationToken);

            // Cache the result
            _cache.Set(cacheKey, permissions, CacheDuration);

            _logger.LogInformation(
                "Loaded {Count} permissions for account {AccountId} (cached for {Duration})",
                permissions.Count, accountId, CacheDuration);

            return permissions;
        }

        /// <inheritdoc/>
        public async Task<bool> IsAdminAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Checking if account {AccountId} is admin", accountId);

            var permissions = await GetAccountPermissionsAsync(accountId, cancellationToken);
            var isAdmin = permissions.Contains(AdminClaim);

            _logger.LogDebug(
                "Account {AccountId} admin status: {IsAdmin}",
                accountId, isAdmin);

            return isAdmin;
        }

        /// <summary>
        /// Invalidate permission cache for an account
        /// Call this when account permissions change (role assigned, claim added, etc.)
        /// </summary>
        public void InvalidatePermissionCache(Guid accountId)
        {
            var cacheKey = $"{CacheKeyPrefix}{accountId}";
            _cache.Remove(cacheKey);

            _logger.LogInformation(
                "Invalidated permission cache for account {AccountId}",
                accountId);
        }

        /// <summary>
        /// Invalidate permission cache for multiple accounts
        /// </summary>
        public void InvalidatePermissionCache(IEnumerable<Guid> accountIds)
        {
            foreach (var accountId in accountIds)
            {
                InvalidatePermissionCache(accountId);
            }
        }
    }
}
