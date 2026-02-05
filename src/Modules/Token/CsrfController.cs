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

            // ✅ Set CSRF cookie
            Response.Cookies.Append("XSRF-TOKEN", token, new CookieOptions
            {
                HttpOnly = false, // ⚠️ Must be accessible to JavaScript
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return Ok(new { csrfToken = token });
        }
    }
}