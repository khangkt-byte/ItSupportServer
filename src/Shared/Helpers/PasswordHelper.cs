using BCrypt.Net;

namespace ItSupportServer.src.Shared.Helpers
{
    /// <summary>
    /// Password hashing and verification using BCrypt
    /// BCrypt is the industry standard for password hashing (OWASP recommended)
    /// Reference: OWASP Password Storage Cheat Sheet
    /// </summary>
    public static class PasswordHelper
    {
        // ✅ BCrypt work factor (RECOMMENDED: 12 for 2024+)
        // Higher = more secure but slower
        // Work factor 12 = ~250ms per hash (acceptable for login UX)
        // Work factor 13 = ~500ms per hash
        // Reference: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
        private const int WorkFactor = 12; // ✅ UPGRADED FROM 11 TO 12

        /// <summary>
        /// Hash a password using BCrypt with work factor 12
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>BCrypt hashed password (60 characters)</returns>
        /// <exception cref="ArgumentException">Thrown when password is empty</exception>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty", nameof(password));
            }

            // ✅ BCrypt.Net automatically generates cryptographically secure salt
            // Result format: $2a$12$[22-char salt][31-char hash]
            // Total length: 60 characters
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        /// <summary>
        /// Verify password against BCrypt hash (constant-time comparison)
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="hashedPassword">BCrypt hashed password from database</param>
        /// <returns>True if password matches</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            try
            {
                // ✅ BCrypt.Net uses constant-time comparison (prevents timing attacks)
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // ✅ Invalid hash format (e.g., not a BCrypt hash)
                return false;
            }
        }

        /// <summary>
        /// Check if password hash needs rehashing (if work factor changed)
        /// </summary>
        /// <param name="hashedPassword">Existing BCrypt hash</param>
        /// <returns>True if rehashing is needed</returns>
        public static bool NeedsRehash(string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                return true;
            }

            try
            {
                // ✅ Extract work factor from hash (format: $2a$WF$...)
                var parts = hashedPassword.Split('$');
                if (parts.Length >= 3 && int.TryParse(parts[2], out var hashWorkFactor))
                {
                    return hashWorkFactor < WorkFactor;
                }
                return true;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Get current work factor configuration
        /// </summary>
        public static int GetWorkFactor() => WorkFactor;
    }
}