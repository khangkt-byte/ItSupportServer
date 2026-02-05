using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ItSupportServer.src.Shared.Middleware
{
    /// <summary>
    /// Enhanced Security headers with CSP nonces
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
            // ✅ Generate nonce for this request
            var nonce = GenerateNonce();
            context.Items["csp-nonce"] = nonce; // Store for use in views

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
                "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=()";

            // ✅ ENHANCED: Content Security Policy with nonce
            var cspPolicy = BuildCspPolicyWithNonce(_env, nonce);
            context.Response.Headers["Content-Security-Policy"] = cspPolicy;

            // ✅ CRITICAL: Strict-Transport-Security (HSTS)
            if (context.Request.IsHttps)
            {
                context.Response.Headers["Strict-Transport-Security"] =
                    "max-age=31536000; includeSubDomains; preload";
            }

            // ✅ Remove server identification headers
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            context.Response.Headers.Remove("X-AspNet-Version");
            context.Response.Headers.Remove("X-AspNetMvc-Version");

            await _next(context);
        }

        /// <summary>
        /// Generate cryptographically secure nonce
        /// </summary>
        private static string GenerateNonce()
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Build CSP policy with nonce support
        /// </summary>
        private static string BuildCspPolicyWithNonce(IHostEnvironment env, string nonce)
        {
            if (!env.IsDevelopment())
            {
                // ✅ PRODUCTION: Strict CSP with nonce
                return string.Join("; ", new[]
                {
                    "default-src 'self'",
                    $"script-src 'self' 'nonce-{nonce}' 'strict-dynamic'", // ✅ Nonce-based scripts
                    "style-src 'self' 'unsafe-inline'", // ⚠️ Consider nonce for styles too
                    "img-src 'self' data: https:",
                    "font-src 'self' data:",
                    "connect-src 'self' https://api.yourdomain.com",
                    "frame-ancestors 'none'",
                    "base-uri 'self'",
                    "form-action 'self'",
                    "upgrade-insecure-requests",
                    "block-all-mixed-content" // ✅ Prevent mixed content
                });
            }

            // ✅ DEVELOPMENT: Relaxed CSP
            return string.Join("; ", new[]
            {
                "default-src 'self'",
                $"script-src 'self' 'unsafe-inline' 'unsafe-eval' 'nonce-{nonce}'",
                "style-src 'self' 'unsafe-inline'",
                "img-src 'self' data: https:",
                "font-src 'self' data:",
                "connect-src 'self' http://localhost:* ws://localhost:* https://localhost:*",
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