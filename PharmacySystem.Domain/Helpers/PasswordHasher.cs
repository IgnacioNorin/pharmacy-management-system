using System;
using System.Security.Cryptography;

namespace PharmacySystem.Helpers
{
    // PBKDF2-HMACSHA256 password hashing. Stored format: "v1$<iterations>$<saltBase64>$<hashBase64>".
    public static class PasswordHasher
    {
        private const string FormatTag = "v1";
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = DeriveKey(password, salt, Iterations);

            return string.Join("$", FormatTag, Iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public static bool Verify(string password, string hashedValue)
        {
            if (!IsHashed(hashedValue))
            {
                return false;
            }

            string[] parts = hashedValue.Split('$');
            int iterations = int.Parse(parts[1]);
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);

            byte[] actualHash = DeriveKey(password, salt, iterations);

            return FixedTimeEquals(actualHash, expectedHash);
        }

        // Distinguishes an already-hashed value from a plain-text legacy password, so callers
        // can avoid re-hashing a hash when a form round-trips an unchanged password field.
        public static bool IsHashed(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            string[] parts = value.Split('$');
            int iterations;
            return parts.Length == 4 && parts[0] == FormatTag && int.TryParse(parts[1], out iterations);
        }

        private static byte[] DeriveKey(string password, byte[] salt, int iterations)
        {
            return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, HashSize);
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
