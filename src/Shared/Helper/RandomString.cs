using System.Security.Cryptography;
using System.Text;

namespace ItSupportServer.src.Shared.Helper
{
    public class RandomString
    {
        public static string GenerateRandomString(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_@#=.";

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
            var stringBuilder = new StringBuilder(length);


            for (int i = 0; i < length; i++)
            {
                int index = RandomNumberGenerator.GetInt32(chars.Length);
                stringBuilder.Append(chars[index]);
            }

            return stringBuilder.ToString();
        }


        public static string GenerateRandomNumericString(int length = 6)
        {
            const string chars = "0123456789";
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
            var stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int index = RandomNumberGenerator.GetInt32(chars.Length);
                stringBuilder.Append(chars[index]);
            }
            return stringBuilder.ToString();
        }
    }
}
