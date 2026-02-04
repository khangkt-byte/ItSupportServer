using System.Security.Claims;
using ItSupportServer.src.Shared.Exceptions;

namespace ItSupportServer.src.Modules.Authorization
{
    /// <summary>
    /// Extension methods for authorization
    /// </summary>
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Get AccountId from ClaimsPrincipal
        /// </summary>
        public static Guid GetAccountId(this ClaimsPrincipal principal)
        {
            var accountIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? principal.FindFirst("AccountId")?.Value
                              ?? principal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                throw new UnauthorizedException("Không tìm thấy thông tin tài khoản");
            }

            return accountId;
        }

        /// <summary>
        /// Get Username from ClaimsPrincipal
        /// </summary>
        public static string GetUsername(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Name)?.Value
                ?? principal.FindFirst("Username")?.Value
                ?? throw new UnauthorizedException("Không tìm thấy tên người dùng");
        }

        /// <summary>
        /// Check if user is admin
        /// </summary>
        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin")
                || principal.HasClaim(c => c.Value == "Admin");
        }
    }
}