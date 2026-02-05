using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Token
{
    /// <summary>
    /// CSRF Token API
    /// Reference: OWASP CSRF Prevention Cheat Sheet
    /// </summary>
    [ApiController]
    [Route("api/csrf")]
    public class CsrfController : ControllerBase
    {
        private readonly ICsrfTokenService _csrfTokenService;
        private readonly ILogger<CsrfController> _logger;

        public CsrfController(
            ICsrfTokenService csrfTokenService,
            ILogger<CsrfController> logger)
        {
            _csrfTokenService = csrfTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Get CSRF token for frontend
        /// </summary>
        [HttpGet("token")]
        [AllowAnonymous]
        public IActionResult GetCsrfToken()
        {
            // ✅ FIX: Use injected service instead of static method
            var token = _csrfTokenService.GenerateToken();

            // ✅ Set token in cookie (SameSite=Lax for better compatibility)
            Response.Cookies.Append("XSRF-TOKEN", token, new CookieOptions
            {
                HttpOnly = false, // ✅ Required for JavaScript access
                Secure = true,     // ✅ HTTPS only
                SameSite = SameSiteMode.Lax, // ✅ Allow safe cross-site GET
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            _logger.LogDebug("CSRF token generated for IP: {IP}", 
                HttpContext.Connection.RemoteIpAddress);

            return Ok(new { csrfToken = token });
        }
    }
}