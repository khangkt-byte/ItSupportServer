using Microsoft.AspNetCore.Http;

namespace ItSupportServer.src.Shared.Configuration
{
    /// <summary>
    /// Factory for creating standardized cookie options
    /// Pattern: Factory Method for consistent configuration
    /// Reference: Microsoft ASP.NET Core Best Practices
    /// </summary>
    public static class CookieOptionsFactory
    {
        /// <summary>
        /// Create CSRF token cookie options
        /// Security: SameSite=Lax, Secure=true, HttpOnly=false
        /// </summary>
        public static CookieOptions CreateCsrfCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = false, // JavaScript access required for X-CSRF-Token header
                Secure = true,     // HTTPS only (enforced in production)
                SameSite = SameSiteMode.Lax, // CSRF protection with better compatibility
                Path = "/",        // Site-wide availability
                Expires = DateTimeOffset.UtcNow.AddHours(1) // 1 hour expiry
            };
        }

        /// <summary>
        /// Create refresh token cookie options
        /// Security: SameSite=Strict, Secure=true, HttpOnly=true
        /// </summary>
        public static CookieOptions CreateRefreshTokenCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,  // No JavaScript access (XSS protection)
                Secure = true,     // HTTPS only
                SameSite = SameSiteMode.Strict, // Strict CSRF protection
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7) // 7 day expiry
            };
        }
    }
}