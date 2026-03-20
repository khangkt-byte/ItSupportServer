using System.Security.Cryptography;
using System.Text;

namespace ItSupportServer.src.Shared.Helpers
{
    public static class RandomString
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_@#=.";

        /// <summary>
        /// Generate a cryptographically secure random alphanumeric string.
        /// Used for: temporary passwords (AccountService.ResetPasswordAsync)
        /// </summary>
        public static string GenerateRandomString(int length = 6)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(Chars[RandomNumberGenerator.GetInt32(Chars.Length)]);

            return sb.ToString();
        }
    }
}
