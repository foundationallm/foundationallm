namespace FoundationaLLM.Common.Models.Configuration.Graph
{
    /// <summary>
    /// Provides configuration for Graph Service.
    /// </summary>
    public class GraphServiceSettings
    {
        /// <summary>
        /// The Azure tenant identifier where the app was registered.
        /// </summary>
        public required string TenantId { get; set; }
        /// <summary>
        /// The application (client) identifier of the Azure app registration.
        /// </summary>
        public required string ClientId { get; set; }
        /// <summary>
        /// The application (client) secret of the Azure app registration.
        /// </summary>
        public required string ClientSecret { get; set; }
    }
}
