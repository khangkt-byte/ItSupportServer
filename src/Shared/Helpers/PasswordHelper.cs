using BCrypt.Net;

namespace ItSupportServer.src.Shared.Helpers
{
    /// <summary>
    /// Password hashing and verification using BCrypt
    /// BCrypt is the industry standard for password hashing (OWASP recommended)
    /// </summary>
    public static class PasswordHelper
    {
        // BCrypt work factor (default: 11)
        // Higher = more secure but slower
        // Recommended range: 10-12 for modern systems
        private const int WorkFactor = 11;

        /// <summary>
        /// Hash a password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>BCrypt hashed password (60 characters)</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty", nameof(password));
            }

            // BCrypt.Net automatically generates salt
            // Result is always 60 characters: $2a$11$[salt][hash]
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        /// <summary>
        /// Verify password against BCrypt hash
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
                // BCrypt.Verify is timing-safe (prevents timing attacks)
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (SaltParseException)
            {
                // Invalid hash format
                return false;
            }
        }

        /// <summary>
        /// Check if password hash needs rehashing (work factor changed)
        /// </summary>
        public static bool NeedsRehash(string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                return true;
            }

            try
            {
                // Check if current work factor matches desired work factor
                return !BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor);
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Generate random password for temporary use
        /// </summary>
        /// <param name="length">Password length (default: 12)</param>
        /// <returns>Random password with letters, numbers, and symbols</returns>
        public static string GenerateRandomPassword(int length = 12)
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string numbers = "23456789";
            const string symbols = "@#$%";

            var random = new Random();
            var password = new List<char>();

            // Ensure at least one of each type
            password.Add(lowercase[random.Next(lowercase.Length)]);
            password.Add(uppercase[random.Next(uppercase.Length)]);
            password.Add(numbers[random.Next(numbers.Length)]);
            password.Add(symbols[random.Next(symbols.Length)]);

            // Fill remaining with random characters
            var allChars = lowercase + uppercase + numbers + symbols;
            for (int i = password.Count; i < length; i++)
            {
                password.Add(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
    }
}