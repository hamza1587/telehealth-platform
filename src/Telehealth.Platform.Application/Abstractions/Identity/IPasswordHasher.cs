namespace Telehealth.Platform.Application.Abstractions.Identity;

/// <summary>
/// Service for secure password hashing using modern algorithms.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password for storage.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    /// <returns>True if the password matches, false otherwise.</returns>
    bool VerifyPassword(string password, string hashedPassword);

    /// <summary>
    /// Checks if the password needs to be rehashed (e.g., algorithm upgrade).
    /// </summary>
    bool NeedsRehash(string hashedPassword);
}
