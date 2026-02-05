using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Middleware;

namespace ItSupportServer.src.Modules.Token
{
    [ApiController]
    [Route("api/csrf")]
    public class CsrfController : ControllerBase
    {
        /// <summary>
        /// Get CSRF token for frontend
        /// </summary>
        [HttpGet("token")]
        [AllowAnonymous]
        public IActionResult GetCsrfToken()
        {
            var token = CsrfValidationMiddleware.GenerateCsrfToken();

            // ✅ FIX: Use SameSite=Lax for better compatibility
            Response.Cookies.Append("XSRF-TOKEN", token, new CookieOptions
            {
                HttpOnly = false, // ✅ Required for JavaScript access
                Secure = true,     // ✅ HTTPS only
                SameSite = SameSiteMode.Lax, // ✅ FIXED: Allow safe cross-site GET
                Path = "/",        // ✅ Add: Explicit path
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return Ok(new { csrfToken = token });
        }
    }
}