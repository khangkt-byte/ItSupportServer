using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace ItSupportServer.src.Shared.Middleware
{
    /// <summary>
    /// CSRF protection middleware using Double Submit Cookie pattern
    /// Reference: OWASP CSRF Prevention Cheat Sheet
    /// </summary>
    public class CsrfValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] SafeMethods = { "GET", "HEAD", "OPTIONS", "TRACE" };
        private static readonly string[] SkipPaths = { "/api/auth/login", "/api/auth/refresh-token" };

        public CsrfValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            // ✅ Skip CSRF for safe methods
            if (SafeMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // ✅ Skip CSRF for specific paths
            if (SkipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // ✅ Validate CSRF token for state-changing requests
            var csrfToken = context.Request.Headers["X-CSRF-Token"].FirstOrDefault();
            var csrfCookie = context.Request.Cookies["XSRF-TOKEN"];

            if (string.IsNullOrEmpty(csrfToken) || string.IsNullOrEmpty(csrfCookie))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = "Missing CSRF token",
                    errorCode = "CSRF_TOKEN_MISSING"
                });
                return;
            }

            // ✅ Validate token matches cookie
            if (!csrfToken.Equals(csrfCookie, StringComparison.Ordinal))
            {
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
        /// Generate cryptographically secure CSRF token
        /// </summary>
        public static string GenerateCsrfToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }
    }

    public static class CsrfMiddlewareExtensions
    {
        public static IApplicationBuilder UseCsrfValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CsrfValidationMiddleware>();
        }
    }
}