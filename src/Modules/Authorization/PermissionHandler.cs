using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ItSupportServer.src.Modules.Authorization
{
    public class PermissionHandler(IServiceProvider serviceProvider) : AuthorizationHandler<PermissionRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Lấy EmployeeId từ Claims của User hiện tại (thường là NameIdentifier)
            var employeeId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeId)) return;

            // Vì PermissionHandler là Singleton/Scoped, ta dùng Scope để gọi Service
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<AuthorizationService>();

            // Gọi hàm của bạn
            var result = await authService.RoleHasClaimAsync(employeeId, requirement.Permission);

            if (result.Success && result.Item)
            {
                context.Succeed(requirement);
            }
        }
    }
}
