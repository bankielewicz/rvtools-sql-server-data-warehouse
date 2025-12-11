namespace RVToolsWeb.Services.Auth;

/// <summary>
/// Service for encrypting and decrypting sensitive credentials using Data Protection API.
/// Used to protect LDAP bind passwords and other sensitive configuration values.
/// </summary>
public interface ICredentialProtectionService
{
    /// <summary>
    /// Encrypts a plaintext credential value.
    /// </summary>
    /// <param name="plaintext">The plaintext value to encrypt</param>
    /// <returns>Base64-encoded encrypted value, or null if input is null/empty</returns>
    string? Encrypt(string? plaintext);

    /// <summary>
    /// Decrypts an encrypted credential value.
    /// </summary>
    /// <param name="encrypted">The Base64-encoded encrypted value</param>
    /// <returns>Decrypted plaintext, or null if input is null/empty or decryption fails</returns>
    string? Decrypt(string? encrypted);

    /// <summary>
    /// Checks if a value appears to be encrypted (vs plaintext).
    /// Used during migration to detect unencrypted legacy values.
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <returns>True if the value appears to be encrypted</returns>
    bool IsEncrypted(string? value);
}
