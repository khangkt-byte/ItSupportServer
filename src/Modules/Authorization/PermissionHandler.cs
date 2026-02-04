using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Authorization handler for permission-based access control
    /// Pattern: Policy-based authorization with dependency injection
    /// Security: Check claims from account directly OR from account roles
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PermissionHandler> _logger;

        public PermissionHandler(
            IServiceProvider serviceProvider,
            ILogger<PermissionHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Get AccountId from claims
            var accountIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? context.User.FindFirst("AccountId")?.Value
                              ?? context.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                _logger.LogWarning(
                    "Authorization failed: No valid AccountId claim found for permission '{Permission}'",
                    requirement.Permission);
                return;
            }

            // Create scope for scoped services
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

            try
            {
                // Check if account has the required permission
                // This will check:
                // 1. Direct account claims (AccountClaims)
                // 2. Role-based claims (AccountRoles -> RoleClaims)
                // 3. Admin claim (bypass)
                var hasPermission = await authService.HasPermissionAsync(
                    accountId,
                    requirement.Permission);

                if (hasPermission)
                {
                    _logger.LogDebug(
                        "Authorization succeeded: Account {AccountId} has permission '{Permission}'",
                        accountId, requirement.Permission);
                    
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning(
                        "Authorization failed: Account {AccountId} lacks permission '{Permission}'",
                        accountId, requirement.Permission);
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - authorization should fail gracefully
                _logger.LogError(ex,
                    "Authorization error for Account {AccountId}, Permission '{Permission}'",
                    accountId, requirement.Permission);
            }
        }
    }
}
