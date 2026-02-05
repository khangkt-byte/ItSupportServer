using ItSupportServer.src.Modules.Token;

namespace ItSupportServer.src.Shared.Middleware
{
    /// <summary>
    /// Multi-layer CSRF protection (OWASP Defense in Depth)
    /// Combines: Token validation + Origin check + Custom headers
    /// Reference: OWASP CSRF Prevention Cheat Sheet
    /// Performance: 3 middleware → 1 middleware (-66% overhead)
    /// </summary>
    public class EnhancedCsrfProtectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EnhancedCsrfProtectionMiddleware> _logger;
        private readonly ICsrfTokenService _csrfTokenService;
        private readonly HashSet<string> _allowedOrigins;

        private static readonly string[] SafeMethods = { "GET", "HEAD", "OPTIONS", "TRACE" };
        private static readonly string[] SkipPaths = { "/api/auth/login", "/api/auth/refresh-token", "/api/csrf/token" };

        public EnhancedCsrfProtectionMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<EnhancedCsrfProtectionMiddleware> logger,
            ICsrfTokenService csrfTokenService)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
            _csrfTokenService = csrfTokenService;

            _allowedOrigins = new HashSet<string>(
                configuration.GetSection("Security:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase
            );
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            // ✅ Skip for safe methods
            if (SafeMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // ✅ Skip for specific paths
            if (SkipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // ===== LAYER 1: ORIGIN VALIDATION (OWASP Recommended) =====
            
            if (!ValidateOrigin(context))
            {
                _logger.LogWarning(
                    "CSRF Blocked: Invalid Origin | IP: {IP} | Origin: {Origin}",
                    context.Connection.RemoteIpAddress,
                    context.Request.Headers["Origin"].ToString()
                );

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid request origin",
                    errorCode = "INVALID_ORIGIN"
                });
                return;
            }

            // ===== LAYER 2: CUSTOM HEADER VALIDATION (Defense in Depth) =====
            
            if (!ValidateCustomHeaders(context))
            {
                _logger.LogWarning(
                    "CSRF Blocked: Missing custom headers | IP: {IP}",
                    context.Connection.RemoteIpAddress
                );

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid request headers",
                    errorCode = "MISSING_CUSTOM_HEADER"
                });
                return;
            }

            // ===== LAYER 3: CSRF TOKEN VALIDATION (Primary Defense) =====
            
            var csrfToken = context.Request.Headers["X-CSRF-Token"].FirstOrDefault();
            var csrfCookie = context.Request.Cookies["XSRF-TOKEN"];

            if (string.IsNullOrEmpty(csrfToken) || string.IsNullOrEmpty(csrfCookie))
            {
                _logger.LogWarning("CSRF Blocked: Missing token | IP: {IP}", 
                    context.Connection.RemoteIpAddress);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Missing CSRF token",
                    errorCode = "CSRF_TOKEN_MISSING"
                });
                return;
            }

            // ✅ ENHANCED: HMAC signature validation using service
            if (!_csrfTokenService.VerifyToken(csrfToken, csrfCookie))
            {
                _logger.LogWarning("CSRF Blocked: Invalid token | IP: {IP}", 
                    context.Connection.RemoteIpAddress);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid CSRF token",
                    errorCode = "CSRF_TOKEN_INVALID"
                });
                return;
            }

            await _next(context);
        }

        /// <summary>
        /// Layer 1: Origin validation
        /// </summary>
        private bool ValidateOrigin(HttpContext context)
        {
            var origin = context.Request.Headers["Origin"].FirstOrDefault();
            var referer = context.Request.Headers["Referer"].FirstOrDefault();

            var sourceOrigin = origin ?? ExtractOriginFromReferer(referer);

            if (string.IsNullOrEmpty(sourceOrigin))
                return false; // Block requests without Origin/Referer

            sourceOrigin = sourceOrigin.TrimEnd('/');

            // Check whitelist
            if (_allowedOrigins.Contains(sourceOrigin))
                return true;

            // Allow same-origin
            var requestOrigin = $"{context.Request.Scheme}://{context.Request.Host}";
            return sourceOrigin.Equals(requestOrigin, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Layer 2: Custom header validation
        /// </summary>
        private bool ValidateCustomHeaders(HttpContext context)
        {
            var requestedWith = context.Request.Headers["X-Requested-With"].FirstOrDefault();

            // Allow if X-Requested-With header exists
            if (!string.IsNullOrEmpty(requestedWith) && 
                requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Allow if Content-Type is application/json (modern SPAs)
            var contentType = context.Request.ContentType ?? "";
            return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
        }

        private string? ExtractOriginFromReferer(string? referer)
        {
            if (string.IsNullOrEmpty(referer)) return null;

            try
            {
                var uri = new Uri(referer);
                return $"{uri.Scheme}://{uri.Authority}";
            }
            catch
            {
                return null;
            }
        }
    }

    public static class EnhancedCsrfProtectionMiddlewareExtensions
    {
        public static IApplicationBuilder UseEnhancedCsrfProtection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EnhancedCsrfProtectionMiddleware>();
        }
    }
}