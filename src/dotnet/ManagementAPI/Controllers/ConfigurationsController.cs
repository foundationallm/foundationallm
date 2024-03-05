using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Management.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Management.API.Controllers
{
    /// <summary>
    /// Provides methods for interacting with the Configuration Management service.
    /// </summary>
    /// <remarks>
    /// Constructor for the Configurations Controller.
    /// </remarks>
    /// <param name="configurationManagementService">The Configuration Management service
    /// provides methods for managing configurations for FoundationaLLM.</param>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route($"instances/{{instanceId}}/providersX/{ResourceProviderNames.FoundationaLLM_Configuration}/configurations")]
    public class ConfigurationsController(
        IConfigurationManagementService configurationManagementService) : ControllerBase
    {
        /// <summary>
        /// Returns the branding configuration from app configuration.
        /// </summary>
        [HttpGet("branding", Name = "GetBrandingConfigurations")]
        public async Task<ClientBrandingConfiguration> GetBrandingConfigurations() =>
            await configurationManagementService.GetBrandingConfigurationAsync();

        /// <summary>
        /// Updates the branding configuration in app configuration.
        /// </summary>
        /// <param name="brandingConfiguration"></param>
        /// <returns></returns>
        [HttpPut("branding", Name = "UpdateBrandingConfigurations")]
        public async Task UpdateBrandingConfigurations([FromBody] ClientBrandingConfiguration brandingConfiguration) =>
            await configurationManagementService.SetBrandingConfiguration(brandingConfiguration);
    }
}
