using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace EdgePMO.API.Services
{
    public static class PasswordHasher
    {
        public static string Hash(string password, byte[] salt)
        {
            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 50000,
                    numBytesRequested: 32
                )
            );
        }

        public static bool Verify(string password, string storedHash, byte[] salt)
        {
            string? hash = Hash(password, salt);
            return hash == storedHash;
        }

        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using RandomNumberGenerator? rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
    }
}
