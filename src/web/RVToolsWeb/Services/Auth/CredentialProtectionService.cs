namespace RVToolsWeb.Services.Auth;

using Microsoft.AspNetCore.DataProtection;

/// <summary>
/// Service for encrypting and decrypting sensitive credentials using ASP.NET Core Data Protection API.
///
/// Security Notes:
/// - Keys are stored in the default location (%LOCALAPPDATA%\ASP.NET\DataProtection-Keys on Windows)
/// - For multi-server deployments, configure shared key storage (Azure Blob, Redis, etc.)
/// - Keys are automatically rotated every 90 days by default
/// - Old keys are retained for decryption (key ring)
/// </summary>
public class CredentialProtectionService : ICredentialProtectionService
{
    private const string Purpose = "RVToolsDW.Credentials.v1";
    private const string EncryptedPrefix = "ENC:";

    private readonly IDataProtector _protector;
    private readonly ILogger<CredentialProtectionService> _logger;

    public CredentialProtectionService(
        IDataProtectionProvider dataProtectionProvider,
        ILogger<CredentialProtectionService> logger)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
        _logger = logger;
    }

    public string? Encrypt(string? plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            return null;
        }

        try
        {
            var encrypted = _protector.Protect(plaintext);
            // Prefix with marker so we can identify encrypted values
            return EncryptedPrefix + encrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt credential");
            throw new InvalidOperationException("Failed to encrypt credential", ex);
        }
    }

    public string? Decrypt(string? encrypted)
    {
        if (string.IsNullOrEmpty(encrypted))
        {
            return null;
        }

        try
        {
            // Handle legacy unencrypted values during migration
            if (!encrypted.StartsWith(EncryptedPrefix))
            {
                _logger.LogWarning(
                    "Attempted to decrypt a value that doesn't appear to be encrypted. " +
                    "This may be a legacy plaintext value that needs migration.");
                return encrypted; // Return as-is for backwards compatibility
            }

            var encryptedValue = encrypted.Substring(EncryptedPrefix.Length);
            return _protector.Unprotect(encryptedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt credential. The key may have been rotated or the value corrupted.");
            return null;
        }
    }

    public bool IsEncrypted(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return value.StartsWith(EncryptedPrefix);
    }
}
