
using System.Security.Cryptography;
using System.Text;

namespace WEBBANDIENTHOAI.Helpers

{
    public static class SecurityHelper
    {


        public static byte[] HashPassword(string plain)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(plain ?? ""));
        }

        public static string ToHex(this byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "").ToLowerInvariant();
        }
    }
}

