using FoundationaLLM.Common.Models.Configuration.Branding;

namespace FoundationaLLM.Management.Interfaces
{
    /// <summary>
    /// Provides methods for managing app configuration.
    /// </summary>
    public interface IConfigurationManagementService
    {
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
