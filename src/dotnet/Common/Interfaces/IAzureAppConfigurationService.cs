namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Provides access to and management of Azure App Configuration.
    /// </summary>
    public interface IAzureAppConfigurationService
    {
        /// <summary>
        /// Gets the value of a configuration setting from Azure App Configuration.
        /// </summary>
        /// <param name="key">The App Configuration setting key.</param>
        /// <returns></returns>
        Task<string?> GetConfigurationSettingAsync(string key);

        /// <summary>
        /// Gets the values of configuration settings from Azure App Configuration.
        /// </summary>
        /// <param name="keyFilter">The key name filter used to select the settings keys for which values are retrieved.</param>
        /// <returns></returns>
        Task<List<(string Key, string? Value, string ContentType)>> GetConfigurationSettingsAsync(string keyFilter);

        /// <summary>
        /// Sets the value of a configuration setting in Azure App Configuration.
        /// </summary>
        /// <param name="key">The App Configuration setting key.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="contentType">The content type of the value.</param>
        /// <returns></returns>
        Task SetConfigurationSettingAsync(string key, string value, string contentType);

        /// <summary>
        /// Gets the value of a feature flag from Azure App Configuration.
        /// </summary>
        /// <param name="key">The App Configuration feature flag key.</param>
        /// <returns></returns>
        Task<bool> GetFeatureFlagAsync(string key);

        /// <summary>
        /// Sets the value of a feature flag in Azure App Configuration.
        /// </summary>
        /// <param name="key">The App Configuration feature flag key.</param>
        /// <param name="flagEnabled">Whether to enable the feature flag.</param>
        /// <returns></returns>
        Task SetFeatureFlagAsync(string key, bool flagEnabled);

        /// <summary>
        /// Returns a map of configuration setting keys and whether they exist in Azure App Configuration.
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, bool>> CheckAppConfigurationSettingsExistAsync();
    }
}
