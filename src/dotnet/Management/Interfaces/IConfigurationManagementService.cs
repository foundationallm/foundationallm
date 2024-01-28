using FoundationaLLM.Common.Models.Configuration.Branding;

namespace FoundationaLLM.Management.Interfaces
{
    /// <summary>
    /// Provides methods for managing app configuration.
    /// </summary>
    public interface IConfigurationManagementService
    {
        /// <summary>
        /// Retrieves a list of public agent hints from app configuration.
        /// </summary>
        /// <returns></returns>
        Task<List<string>?> GetAgentHintsAsync();

        /// <summary>
        /// Creates or updates app configuration setting that lists the public agent hints.
        /// </summary>
        /// <param name="agentHints">Set the value to null if you wish to clear the list of
        /// agent hints.</param>
        /// <returns></returns>
        Task UpdateAgentHintsAsync(IEnumerable<string>? agentHints);

        /// <summary>
        /// Retrieves the allow agent selection feature flag value from app configuration.
        /// </summary>
        /// <returns></returns>
        Task<bool> GetAllowAgentSelectionAsync();

        /// <summary>
        /// Sets the allow agent selection feature flag value in app configuration.
        /// </summary>
        /// <param name="allowAgentSelection">Indicates whether to enable or disable the feature flag.</param>
        /// <returns></returns>
        Task SetAllowAgentSelectionAsync(bool allowAgentSelection);

        /// <summary>
        /// Retrieves the branding configuration from app configuration.
        /// </summary>
        /// <returns></returns>
        Task<ClientBrandingConfiguration> GetBrandingConfigurationAsync();

        /// <summary>
        /// Sets the branding configuration in app configuration.
        /// </summary>
        /// <param name="brandingConfiguration">The branding configuration settings to apply.</param>
        /// <returns></returns>
        Task SetBrandingConfiguration(ClientBrandingConfiguration brandingConfiguration);
    }
}
