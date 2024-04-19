namespace FoundationaLLM.Gateway.Models.Configuration
{
    /// <summary>
    /// Provides settings for the Gateway core service.
    /// </summary>
    public class GatewayCoreSettings
    {
        /// <summary>
        /// The semicolon separated list of Azure Open AI endpoints used by the Gateway core service.
        /// </summary>
        public required string AzureOpenAIAccounts { get; set; }
    }
}
