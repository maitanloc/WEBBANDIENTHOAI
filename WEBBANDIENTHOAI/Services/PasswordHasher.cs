using System;
using System.Security.Cryptography;
using System.Text;

namespace WEBBANDIENTHOAI.Services
{
    public static class PasswordHasher
    {
        public static byte[] Hash(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static bool Verify(string password, byte[] passwordHash)
        {
            var hashedInput = Hash(password);
            return CompareByteArrays(hashedInput, passwordHash);
        }

        private static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }

            return true;
        }
    }
}