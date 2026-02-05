using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ItSupportServer.src.Shared.Configuration;

namespace ItSupportServer.src.Modules.Token
{
    /// <summary>
    /// CSRF Token API
    /// Provides CSRF tokens for protecting state-changing requests
    /// 
    /// Reference: OWASP CSRF Prevention Cheat Sheet
    /// Compliance: OpenAPI 3.0, REST API Guidelines
    /// Rate Limiting: Subject to global rate limiter (100 req/min)
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
        /// <remarks>
        /// Generates an HMAC-signed CSRF token for protecting state-changing requests.
        /// 
        /// **Implementation:**
        /// - Token automatically set in `XSRF-TOKEN` cookie
        /// - Include token in `X-CSRF-Token` header for POST/PUT/DELETE/PATCH requests
        /// - Token expires after 1 hour
        /// 
        /// **Security:**
        /// - HMAC-SHA256 signature validation
        /// - SameSite=Lax cookie protection
        /// - HTTPS-only in production
        /// - Cryptographically secure random generation
        /// 
        /// **Rate Limiting:**
        /// - Global limit: 100 requests per minute
        /// - Bot detection: 10 requests per minute for suspicious User-Agents
        /// - Returns 429 if limit exceeded
        /// 
        /// **Usage Example:**
        /// ```javascript
        /// // 1. Get CSRF token
        /// const response = await fetch('/api/csrf/token');
        /// const { csrfToken } = await response.json();
        /// 
        /// // 2. Use in subsequent requests
        /// await fetch('/api/issue-logs', {
        ///   method: 'POST',
        ///   headers: {
        ///     'X-CSRF-Token': csrfToken,
        ///     'Content-Type': 'application/json'
        ///   },
        ///   credentials: 'include',
        ///   body: JSON.stringify(data)
        /// });
        /// ```
        /// 
        /// **Error Handling:**
        /// ```javascript
        /// try {
        ///   const response = await fetch('/api/csrf/token');
        ///   if (response.status === 429) {
        ///     const retryAfter = response.headers.get('Retry-After');
        ///     console.log(`Rate limited. Retry after ${retryAfter} seconds`);
        ///   }
        /// } catch (error) {
        ///   console.error('Failed to get CSRF token:', error);
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">CSRF token generated successfully</response>
        /// <response code="429">Rate limit exceeded - too many requests</response>
        /// <response code="500">Internal server error during token generation</response>
        [HttpGet("token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CsrfTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RateLimitResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult GetCsrfToken()
        {
            try
            {
                var token = _csrfTokenService.GenerateToken();

                // ✅ USE FACTORY - Consistent cookie configuration
                Response.Cookies.Append("XSRF-TOKEN", token,
                    CookieOptionsFactory.CreateCsrfCookieOptions());

                _logger.LogDebug(
                    "CSRF token generated | IP: {IP} | UserAgent: {UserAgent}",
                    HttpContext.Connection.RemoteIpAddress,
                    HttpContext.Request.Headers["User-Agent"].ToString()
                );

                return Ok(new CsrfTokenResponse { CsrfToken = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CSRF token generation failed");

                return Problem(
                    title: "CSRF Token Generation Failed",
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: "An unexpected error occurred while generating the CSRF token. Please try again."
                );
            }
        }
    }
}