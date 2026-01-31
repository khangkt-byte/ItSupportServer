using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Authorization handler tối ưu: Kết hợp linh hoạt Claim và xử lý lỗi chặt chẽ
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IAuthorizationService _authService;
        private readonly ILogger<PermissionHandler> _logger;

        public PermissionHandler(
            IAuthorizationService authService,
            ILogger<PermissionHandler> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // 1. Lấy AccountId linh hoạt (Ưu điểm Bản 1)
            var accountIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? context.User.FindFirst("AccountId")?.Value
                              ?? context.User.FindFirst("sub")?.Value;

            // 2. Tách biệt kiểm tra lỗi (Ưu điểm Bản 2)
            if (string.IsNullOrEmpty(accountIdClaim))
            {
                _logger.LogWarning(
                    "Authorization failed: No valid AccountId/Sub claim found for permission '{Permission}'",
                    requirement.Permission);
                return; // Fail-closed
            }

            if (!Guid.TryParse(accountIdClaim, out var accountId))
            {
                _logger.LogWarning(
                    "Authorization failed: AccountId '{RawValue}' has invalid format for permission '{Permission}'",
                    accountIdClaim, requirement.Permission);
                return;
            }

            try
            {
                // 3. Kiểm tra quyền qua Service (Sử dụng Cache bên trong Service)
                var hasPermission = await _authService.HasPermissionAsync(
                    accountId,
                    requirement.Permission);

                if (hasPermission)
                {
                    _logger.LogDebug(
                        "Authorization succeeded: Account {AccountId} granted '{Permission}'",
                        accountId, requirement.Permission);

                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning(
                        "Authorization denied: Account {AccountId} does not have permission '{Permission}'",
                        accountId, requirement.Permission);
                }
            }
            catch (Exception ex)
            {
                // Fail-secure: Không gọi context.Succeed() khi có lỗi hệ thống
                _logger.LogError(ex,
                    "Authorization error for Account {AccountId} during check for '{Permission}'",
                    accountId, requirement.Permission);
            }
        }
    }
}