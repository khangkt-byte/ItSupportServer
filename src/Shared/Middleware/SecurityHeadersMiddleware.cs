using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ItSupportServer.src.Shared.Middleware
{
    /// <summary>
    /// Security headers middleware following OWASP recommendations
    /// Reference: OWASP Secure Headers Project
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;

        public SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ✅ CRITICAL: X-Content-Type-Options
            // Prevents MIME-sniffing attacks
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // ✅ CRITICAL: X-Frame-Options
            // Prevents clickjacking attacks
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // ✅ CRITICAL: X-XSS-Protection
            // Legacy XSS protection (CSP is better)
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

            // ✅ CRITICAL: Referrer-Policy
            // Controls referrer information
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // ✅ CRITICAL: Permissions-Policy
            // Controls browser features
            context.Response.Headers["Permissions-Policy"] = 
                "geolocation=(), microphone=(), camera=(), payment=()";

            // ✅ CRITICAL: Content Security Policy
            var cspPolicy = BuildCspPolicy(_env);
            context.Response.Headers["Content-Security-Policy"] = cspPolicy;

            // ✅ CRITICAL: Strict-Transport-Security (HSTS)
            if (context.Request.IsHttps)
            {
                context.Response.Headers["Strict-Transport-Security"] = 
                    "max-age=31536000; includeSubDomains; preload";
            }

            // ✅ Remove server header (don't advertise ASP.NET)
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            context.Response.Headers.Remove("X-AspNet-Version");

            await _next(context);
        }

        private static string BuildCspPolicy(IHostEnvironment env)
        {
            // ✅ STRICT CSP for production
            if (!env.IsDevelopment())
            {
                return string.Join("; ", new[]
                {
                    "default-src 'self'",
                    "script-src 'self'", // ⚠️ Remove 'unsafe-inline' 'unsafe-eval'
                    "style-src 'self' 'unsafe-inline'", // ⚠️ Consider removing 'unsafe-inline'
                    "img-src 'self' data: https:",
                    "font-src 'self' data:",
                    "connect-src 'self' https://api.yourdomain.com",
                    "frame-ancestors 'none'",
                    "base-uri 'self'",
                    "form-action 'self'",
                    "upgrade-insecure-requests"
                });
            }

            // ✅ RELAXED CSP for development
            return string.Join("; ", new[]
            {
                "default-src 'self'",
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'",
                "style-src 'self' 'unsafe-inline'",
                "img-src 'self' data: https:",
                "font-src 'self' data:",
                "connect-src 'self' http://localhost:* ws://localhost:*",
                "frame-ancestors 'none'",
                "base-uri 'self'",
                "form-action 'self'"
            });
        }
    }

    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}