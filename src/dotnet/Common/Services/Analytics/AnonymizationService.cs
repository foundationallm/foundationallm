using FoundationaLLM.Common.Services.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FoundationaLLM.Common.Services.Analytics
{
    /// <summary>
    /// Service for anonymizing user identifiers using secure hashing.
    /// </summary>
    /// <param name="keyVaultService">The Azure Key Vault service for retrieving the anonymization salt.</param>
    /// <param name="appConfigurationService">The Azure App Configuration service.</param>
    /// <param name="logger">The logger.</param>
    public class AnonymizationService(
        IAzureKeyVaultService keyVaultService,
        IAzureAppConfigurationService appConfigurationService,
        ILogger<AnonymizationService> logger) : IAnonymizationService
    {
        private readonly IAzureKeyVaultService _keyVaultService = keyVaultService;
        private readonly IAzureAppConfigurationService _appConfigurationService = appConfigurationService;
        private readonly ILogger<AnonymizationService> _logger = logger;
        private string? _cachedSalt;

        /// <inheritdoc/>
        public string AnonymizeUPN(string? upn)
        {
            if (string.IsNullOrWhiteSpace(upn) || upn == "N/A")
                return "anonymous";

            try
            {
                var salt = GetAnonymizationSalt();
                return HashValue(upn, salt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anonymizing UPN");
                return "anonymous";
            }
        }

        /// <inheritdoc/>
        public string AnonymizeUserId(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || userId == "N/A")
                return "anonymous";

            try
            {
                var salt = GetAnonymizationSalt();
                return HashValue(userId, salt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anonymizing User ID");
                return "anonymous";
            }
        }

        private string HashValue(string value, string salt)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value + salt));
            var hashString = Convert.ToBase64String(hashBytes);
            // Return first 16 characters for readability while maintaining uniqueness
            return hashString.Substring(0, Math.Min(16, hashString.Length));
        }

        private string GetAnonymizationSalt()
        {
            if (_cachedSalt != null)
                return _cachedSalt;

            try
            {
                var saltKey = _appConfigurationService.GetConfigurationSettingAsync("FoundationaLLM:Analytics:AnonymizationSalt").Result;
                if (string.IsNullOrWhiteSpace(saltKey))
                {
                    _logger.LogWarning("AnonymizationSalt configuration key not found. Using default salt (not recommended for production).");
                    _cachedSalt = "default-salt-not-secure";
                    return _cachedSalt;
                }

                // If it's a Key Vault reference, extract the secret name
                if (saltKey.StartsWith("@Microsoft.KeyVault(SecretUri="))
                {
                    var uriStart = saltKey.IndexOf("SecretUri=") + 10;
                    var uriEnd = saltKey.IndexOf(")", uriStart);
                    var secretUri = saltKey.Substring(uriStart, uriEnd - uriStart);
                    var secretName = secretUri.Split('/').Last();

                    _cachedSalt = _keyVaultService.GetSecretValueAsync(secretName).Result;
                }
                else
                {
                    _cachedSalt = saltKey;
                }

                if (string.IsNullOrWhiteSpace(_cachedSalt))
                {
                    _logger.LogWarning("AnonymizationSalt is empty. Using default salt (not recommended for production).");
                    _cachedSalt = "default-salt-not-secure";
                }

                return _cachedSalt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving anonymization salt from Key Vault");
                _cachedSalt = "default-salt-not-secure";
                return _cachedSalt;
            }
        }
    }
}
