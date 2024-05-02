namespace FoundationaLLM.Gateway.Models.Configuration
{
    /// <summary>
    /// The configuration for the Gateway API service.
    /// </summary>
    public class GatewayServiceSettings
    {
        /// <summary>
        /// The URL of the Gateway API.
        /// </summary>
        public required string APIUrl { get; set; }

        /// <summary>
        /// The API key of the Gateway API.
        /// </summary>
        public required string APIKey { get; set; }
    }
}
