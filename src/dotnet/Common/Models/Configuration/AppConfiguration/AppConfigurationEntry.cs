namespace FoundationaLLM.Common.Models.Configuration.AppConfiguration
{
    /// <summary>
    /// Represents a single entry in the app configuration catalog.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="minimumVersion"></param>
    /// <param name="keyVaultSecretName"></param>
    /// <param name="description"></param>
    /// <param name="defaultValue"></param>
    /// <param name="contentType"></param>
    /// <param name="sampleObject"></param>
    /// <param name="canBeEmpty"></param>
    public class AppConfigurationEntry(string key, string? minimumVersion, string? keyVaultSecretName = null,
        string? description = null, string? defaultValue = null, string? contentType = null,
        object? sampleObject = null, bool canBeEmpty = false)
    {
        /// <summary>
        /// The App Configuration key.
        /// </summary>
        public string Key { get; } = key;
        /// <summary>
        /// The minimum version of the app that is required to use this configuration entry.
        /// </summary>
        public string? MinimumVersion { get; } = minimumVersion;
        /// <summary>
        /// The name of the Key Vault secret that contains the value for this configuration entry.
        /// </summary>
        public string? KeyVaultSecretName { get; } = keyVaultSecretName;
        /// <summary>
        /// A description of the configuration entry.
        /// </summary>
        public string? Description { get; } = description;
        /// <summary>
        /// The default value for the configuration entry.
        /// </summary>
        public string? DefaultValue { get; } = defaultValue;
        /// <summary>
        /// If true, the configuration entry must exist but can be empty.
        /// </summary>
        public bool CanBeEmpty { get; } = canBeEmpty;
        /// <summary>
        /// The content type of the configuration entry.
        /// </summary>
        public string? ContentType { get; } = contentType;
        /// <summary>
        /// A sample object that represents the configuration entry.
        /// </summary>
        public object? SampleObject { get; } = sampleObject;
    }

}
