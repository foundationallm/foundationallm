namespace FoundationaLLM.Context.Models.Configuration
{
    /// <summary>
    /// Provides settings for the Azure Container Apps code execution service.
    /// </summary>
    public class AzureContainerAppsCodeSessionProviderServiceSettings
    {
        /// <summary>
        /// Get or sets the list of Azure Container Apps Dynamic Sessions endpoints.
        /// </summary>
        public List<string> DynamicSessionsEndpoints { get; set; } = [];
    }
}
