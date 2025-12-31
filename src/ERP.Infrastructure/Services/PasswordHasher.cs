using System.Security.Cryptography;
using ERP.Application.Common.Interfaces;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Service for hashing and verifying passwords using PBKDF2.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        // Generate a random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash the password
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize
        );

        // Combine salt and hash: [salt][hash]
        var hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

        // Return as Base64 string
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            // Decode the hash
            var hashBytes = Convert.FromBase64String(passwordHash);

            if (hashBytes.Length != SaltSize + KeySize)
                return false;

            // Extract salt and hash
            var salt = new byte[SaltSize];
            var hash = new byte[KeySize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            Array.Copy(hashBytes, SaltSize, hash, 0, KeySize);

            // Hash the input password with the same salt
            var inputHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize
            );

            // Compare the hashes
            return CryptographicOperations.FixedTimeEquals(hash, inputHash);
        }
        catch
        {
            return false;
        }
    }
}
