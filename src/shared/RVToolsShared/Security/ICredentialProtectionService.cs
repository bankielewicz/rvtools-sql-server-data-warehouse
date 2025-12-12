namespace RVToolsShared.Security;

/// <summary>
/// Service for encrypting and decrypting SQL credentials using Data Protection API.
/// Shared between web app (encrypt on save) and service (decrypt on use).
/// </summary>
public interface ICredentialProtectionService
{
    /// <summary>
    /// Encrypts a username/password pair for secure storage.
    /// </summary>
    /// <param name="username">SQL username</param>
    /// <param name="password">SQL password</param>
    /// <returns>Encrypted credential string (base64 encoded)</returns>
    string Protect(string username, string password);

    /// <summary>
    /// Decrypts a stored credential string.
    /// </summary>
    /// <param name="encrypted">Encrypted credential string from database</param>
    /// <returns>Tuple of (username, password)</returns>
    (string username, string password) Unprotect(string encrypted);

    /// <summary>
    /// Validates that a credential string can be decrypted.
    /// </summary>
    /// <param name="encrypted">Encrypted credential string</param>
    /// <returns>True if credential is valid and can be decrypted</returns>
    bool TryValidate(string encrypted);
}
