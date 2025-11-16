using System.Security.Cryptography;
using System.Text;

namespace WEBBANDIENTHOAI.Services
{
    // Hàm giúp hash mật khẩu tương thích với HASHBYTES('SHA2_256', ...)
    public static class PasswordHasher
    {
        public static byte[] Hash(string plain)
        {
            if (plain == null) plain = string.Empty;
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
        }

        public static bool Verify(string plain, byte[] hash)
        {
            var h = Hash(plain);
            if (hash == null) return false;
            if (h.Length != hash.Length) return false;
            for (int i = 0; i < h.Length; i++)
                if (h[i] != hash[i]) return false;
            return true;
        }
    }
}
