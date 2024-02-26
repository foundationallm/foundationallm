using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Provides access to and management of Azure App Configuration.
    /// </summary>
    /// <param name="configurationClient">The Azure App Configuration <see cref="ConfigurationClient"/>.</param>
    /// <param name="logger">The logger.</param>
    public class AzureAppConfigurationService(
        ConfigurationClient configurationClient,
        ILogger<AzureAppConfigurationService> logger) : IAzureAppConfigurationService
    {
        private readonly ConfigurationClient _configurationClient = configurationClient;
        private readonly ILogger<AzureAppConfigurationService> _logger = logger;

        /// <inheritdoc/>
        public async Task<string?> GetConfigurationSettingAsync(string key)
        {
            var setting = await _configurationClient.GetConfigurationSettingAsync(key, null);
            return setting.Value?.Value;
        }

        /// <inheritdoc/>
        public async Task SetConfigurationSettingAsync(string key, string value)
        {
            var setting = new ConfigurationSetting(key, value);
            await _configurationClient.SetConfigurationSettingAsync(setting);
        }

        /// <inheritdoc/>
        public async Task<bool> GetFeatureFlagAsync(string key)
        {
            var allowAgentSelection = false;
            try
            {
                if (!key.StartsWith(FeatureFlagConfigurationSetting.KeyPrefix))
                {
                    key = FeatureFlagConfigurationSetting.KeyPrefix + key;
                }
                var allowAgentSelectionSetting = await _configurationClient
                    .GetConfigurationSettingAsync(key);
                if (allowAgentSelectionSetting.HasValue && allowAgentSelectionSetting.Value is FeatureFlagConfigurationSetting featureFlag)
                {
                    allowAgentSelection = featureFlag.IsEnabled;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting feature flag ({Key}) from app configuration.", key);
            }

            return allowAgentSelection;
        }

        /// <inheritdoc/>
        public async Task SetFeatureFlagAsync(string key, bool flagEnabled)
        {
            try
            {
                var allowAgentSelectionSetting = new FeatureFlagConfigurationSetting(
                    key, isEnabled: flagEnabled);
                await _configurationClient.SetConfigurationSettingAsync(allowAgentSelectionSetting);
                // TODO: Restart the Core API and Agent API services to apply the new feature flag.
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error setting feature flag ({Key}) in app configuration.", key);
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, bool>> CheckAppConfigurationSettingsExistAsync()
        {
            var settings = _configurationClient.GetConfigurationSettingsAsync(new SettingSelector { KeyFilter = "*" });

            var existenceMap = new Dictionary<string, bool>();
            await foreach (var setting in settings)
            {
                existenceMap[setting.Key] = !string.IsNullOrWhiteSpace(setting.Value);
            }
            return existenceMap;
        }
    }
}
