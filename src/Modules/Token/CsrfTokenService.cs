using System.Security.Cryptography;

namespace ItSupportServer.src.Modules.Token
{
    /// <summary>
    /// CSRF Token Generation Service
    /// Pattern: Single Responsibility - Token generation logic
    /// Reference: OWASP CSRF Prevention Cheat Sheet
    /// </summary>
    public interface ICsrfTokenService
    {
        string GenerateToken();
        bool VerifyToken(string headerToken, string cookieToken);
    }

    public class CsrfTokenService : ICsrfTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CsrfTokenService> _logger;

        public CsrfTokenService(
            IConfiguration configuration,
            ILogger<CsrfTokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Generate HMAC-signed CSRF token
        /// Format: {randomToken}.{hmacSignature}
        /// </summary>
        public string GenerateToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            
            var randomToken = Convert.ToBase64String(randomBytes);
            var signature = ComputeHmac(randomToken);

            return $"{randomToken}.{signature}";
        }

        /// <summary>
        /// Verify CSRF token (header vs cookie + HMAC signature)
        /// </summary>
        public bool VerifyToken(string headerToken, string cookieToken)
        {
            try
            {
                var parts = headerToken.Split('.');
                if (parts.Length != 2) return false;

                var token = parts[0];
                var signature = parts[1];

                // ✅ Verify token matches cookie
                if (!headerToken.Equals(cookieToken, StringComparison.Ordinal))
                    return false;

                // ✅ Verify HMAC signature
                var expectedSignature = ComputeHmac(token);
                return signature.Equals(expectedSignature, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CSRF token verification failed");
                return false;
            }
        }

        /// <summary>
        /// Compute HMAC-SHA256 signature
        /// </summary>
        private string ComputeHmac(string data)
        {
            var secret = _configuration["AppSettings:CsrfSecret"];
            
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException(
                    "CSRF secret not configured. Add 'AppSettings:CsrfSecret' to appsettings.json");
            }

            using var hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }
}