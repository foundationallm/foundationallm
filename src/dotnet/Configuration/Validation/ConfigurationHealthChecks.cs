using FoundationaLLM.Configuration.Catalog;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Services;
using Microsoft.Extensions.Logging;
using FoundationaLLM.Configuration.Interfaces;

namespace FoundationaLLM.Configuration.Validation
{
    /// <summary>
    /// Provides health checks for the application's configuration settings.
    /// </summary>
    public class ConfigurationHealthChecks(
        IAzureAppConfigurationService azureAppConfigurationService,
        IAzureKeyVaultService azureKeyVaultService,
        ILogger<ConfigurationHealthChecks> logger) : IConfigurationHealthChecks
    {
        private readonly IAzureAppConfigurationService _azureAppConfigurationService = azureAppConfigurationService;
        private readonly IAzureKeyVaultService _azureKeyVaultService = azureKeyVaultService;
        private readonly ILogger<ConfigurationHealthChecks> _logger = logger;

        /// <inheritdoc/>
        public async Task ValidateConfigurationsAsync(string version)
        {
            var requiredEntries = AppConfigurationCatalog.GetRequiredConfigurationsForVersion(version);

            // Fetch existence maps for App Configuration settings and Key Vault secrets.
            var configExistenceMap = await _azureAppConfigurationService.CheckAppConfigurationSettingsExistAsync();
            var appConfigurationEntries = requiredEntries.ToList();
            var secretExistenceMap = await _azureKeyVaultService.CheckKeyVaultSecretsExistAsync(appConfigurationEntries
                .Where(entry => !string.IsNullOrEmpty(entry.KeyVaultSecretName))
                .Select(entry => entry.KeyVaultSecretName)
                .Distinct());

            // Identify missing App Configuration settings by checking the configExistenceMap,
            // considering whether the setting can be empty.
            var missingConfigurations = appConfigurationEntries
                .Where(entry =>
                {
                    // Check if the setting exists in the map.
                    var exists = configExistenceMap.TryGetValue(entry.Key, out var isPresentAndNotEmpty);

                    // If the setting must not be empty and either doesn't exist or exists as empty, it's missing.
                    // If the setting can be empty, it's only missing if it doesn't exist at all.
                    return !exists || (!isPresentAndNotEmpty && !entry.CanBeEmpty);
                })
                .Select(entry => entry.Key);

            // Identify missing Key Vault secrets by checking the secretExistenceMap.
            var missingKeyVaultSecrets = appConfigurationEntries
                .Where(entry => !string.IsNullOrEmpty(entry.KeyVaultSecretName) &&
                                (!secretExistenceMap.TryGetValue(entry.KeyVaultSecretName, out var secretExists) || !secretExists))
                .Select(entry => entry.KeyVaultSecretName);

            // Prepare lists of missing configurations and secrets for logging and exception handling.
            var configurations = missingConfigurations.ToList();
            var keyVaultSecrets = missingKeyVaultSecrets.ToList();

            if (configurations.Count != 0 || keyVaultSecrets.Count != 0)
            {
                var errorMessage = $"Missing or empty app configurations: {string.Join(", ", configurations)}" +
                                   $"{(configurations.Count != 0 && keyVaultSecrets.Count != 0 ? "\n" : "")}" +
                                   $"{(keyVaultSecrets.Count != 0 ? "Missing or empty Key Vault secrets for app configurations: " + string.Join(", ", keyVaultSecrets) : "\n")}";
                _logger.LogError(errorMessage);

                throw new ConfigurationValidationException(configurations, keyVaultSecrets, null);
            }
        }

        /// <inheritdoc/>
        public async Task ValidateKeyVaultSecretsAsync(string version)
        {
            var requiredSecrets = KeyVaultSecretsCatalog.GetRequiredKeyVaultSecretsForVersion(version);
            var keyVaultSecretEntries = requiredSecrets.ToList();
            var secretExistenceMap = await _azureKeyVaultService.CheckKeyVaultSecretsExistAsync(keyVaultSecretEntries
                .Select(entry => entry.SecretName));

            var missingSecrets = keyVaultSecretEntries
                .Where(entry => !secretExistenceMap.TryGetValue(entry.SecretName, out var secretExists) || !secretExists)
                .Select(entry => entry.SecretName);

            var missingKeyVaultSecrets = missingSecrets.ToList();
            if (missingKeyVaultSecrets.Count != 0)
            {
                var errorMessage = $"Missing or empty Key Vault secrets: {string.Join(", ", missingKeyVaultSecrets)}";
                _logger.LogError(errorMessage);
                throw new ConfigurationValidationException(null, missingKeyVaultSecrets, null);
            }
        }

        /// <inheritdoc/>
        public void ValidateEnvironmentVariables()
        {
            var missingVariables = new List<string>();
            var requiredVariables = EnvironmentVariablesCatalog.GetRequiredEnvironmentVariables();

            foreach (var variable in requiredVariables)
            {
                var value = Environment.GetEnvironmentVariable(variable.Name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                missingVariables.Add(variable.Name);
                _logger.LogError($"Missing environment variable: {variable.Name}");
            }

            if (missingVariables.Count != 0)
            {
                throw new ConfigurationValidationException(null, null, missingVariables);
            }
        }
    }
}
