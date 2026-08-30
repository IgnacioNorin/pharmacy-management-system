using System;
using System.Security.Cryptography;
using System.Text;

namespace PharmacySystem.Helpers
{
    // Temporary passwords handed out by an admin reset. Cryptographically random, from a
    // reduced alphabet with no visually ambiguous characters (no 0/O, 1/l/I), grouped with a
    // dash so it can be read aloud or written down without mistakes. Always well over the
    // configured minimum length; the user is forced to replace it on first login anyway.
    public static class PasswordGenerator
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        private const int GroupSize = 4;
        private const int Groups = 2; // 8 significant characters

        public static string Generate()
        {
            byte[] bytes = new byte[GroupSize * Groups];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0 && i % GroupSize == 0)
                {
                    sb.Append('-');
                }
                sb.Append(Alphabet[bytes[i] % Alphabet.Length]);
            }
            return sb.ToString();
        }
    }
}
