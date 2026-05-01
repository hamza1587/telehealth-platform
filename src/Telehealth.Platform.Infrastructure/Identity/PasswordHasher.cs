using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Telehealth.Platform.Application.Abstractions.Identity;

namespace Telehealth.Platform.Infrastructure.Identity;

/// <summary>
/// Production-grade password hasher using PBKDF2 with SHA-512.
/// Implements secure password storage with salt and iteration count.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    // Current version of the hash format
    private const byte CurrentVersion = 1;

    // Number of iterations for PBKDF2 (OWASP recommends 600,000 for PBKDF2-SHA512)
    private const int Iterations = 600000;

    // Size of the salt in bytes
    private const int SaltSize = 32;

    // Size of the derived key in bytes
    private const int KeySize = 64;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        // Generate a random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Derive the key using PBKDF2
        var key = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA512,
            iterationCount: Iterations,
            numBytesRequested: KeySize);

        // Format: version:salt:hash (all base64 encoded except version)
        var versionByte = new[] { CurrentVersion };
        var hashBytes = versionByte.Concat(salt).Concat(key).ToArray();

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
        {
            return false;
        }

        try
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);

            // Check version
            if (hashBytes.Length < 1 || hashBytes[0] != CurrentVersion)
            {
                return false;
            }

            // Extract salt and stored key
            var salt = hashBytes[1..(SaltSize + 1)];
            var storedKey = hashBytes[(SaltSize + 1)..];

            // Derive key from provided password
            var derivedKey = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(storedKey, derivedKey);
        }
        catch
        {
            return false;
        }
    }

    public bool NeedsRehash(string hashedPassword)
    {
        // Check if the hash uses the current version
        try
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            return hashBytes.Length < 1 || hashBytes[0] != CurrentVersion;
        }
        catch
        {
            return true;
        }
    }
}
