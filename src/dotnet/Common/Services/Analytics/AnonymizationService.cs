using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Analytics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace FoundationaLLM.Common.Services.Analytics
{
    /// <summary>
    /// Service for anonymizing user identifiers using secure hashing.
    /// </summary>
    /// <param name="analyticsSettings">The analytics settings.</param>
    /// <param name="logger">The logger.</param>
    public class AnonymizationService(
        IOptions<AnalyticsSettings> analyticsSettings,
        ILogger<AnonymizationService> logger) : IAnonymizationService
    {
        private readonly AnalyticsSettings _analyticsSettings = analyticsSettings.Value;
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
                _cachedSalt = _analyticsSettings.AnonymizationSalt;
                if (string.IsNullOrWhiteSpace(_cachedSalt))
                {
                    _logger.LogWarning("AnonymizationSalt configuration key not found. Using default salt (not recommended for production).");
                    _cachedSalt = "default-salt-not-secure";
                    return _cachedSalt;
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
