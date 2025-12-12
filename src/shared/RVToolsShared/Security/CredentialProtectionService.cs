using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace RVToolsShared.Security;

/// <summary>
/// Encrypts/decrypts SQL credentials using Windows Data Protection API.
/// Both web app and service must use the same key store path to share credentials.
/// </summary>
public class CredentialProtectionService : ICredentialProtectionService
{
    private const string Purpose = "RVTools.SqlCredentials.v1";
    private readonly IDataProtector _protector;

    public CredentialProtectionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(Purpose);
    }

    /// <inheritdoc/>
    public string Protect(string username, string password)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        var credential = new StoredCredential
        {
            Username = username,
            Password = password,
            CreatedUtc = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(credential);
        return _protector.Protect(json);
    }

    /// <inheritdoc/>
    public (string username, string password) Unprotect(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted))
            throw new ArgumentNullException(nameof(encrypted));

        try
        {
            var json = _protector.Unprotect(encrypted);
            var credential = JsonSerializer.Deserialize<StoredCredential>(json)
                ?? throw new InvalidOperationException("Failed to deserialize credential.");

            return (credential.Username, credential.Password);
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new InvalidOperationException(
                "Failed to decrypt credential. The key may have been rotated or the data is corrupt.", ex);
        }
    }

    /// <inheritdoc/>
    public bool TryValidate(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted))
            return false;

        try
        {
            var json = _protector.Unprotect(encrypted);
            var credential = JsonSerializer.Deserialize<StoredCredential>(json);
            return credential != null
                && !string.IsNullOrEmpty(credential.Username)
                && !string.IsNullOrEmpty(credential.Password);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Internal class for JSON serialization of credentials.
    /// </summary>
    private class StoredCredential
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
    }
}
