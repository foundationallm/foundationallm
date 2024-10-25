using FoundationaLLM.Common.Services.Events;

namespace FoundationaLLM.Common.Models.Configuration.Events
{
    /// <summary>
    /// Provides configuration settings to intialize the <see cref="AzureEventGridEventService"/> service.
    /// </summary>
    public class AzureEventGridEventServiceSettings
    {
        /// <summary>
        /// The Azure resource identifier for the Azure Event Grid namespace.
        /// </summary>
        public required string NamespaceId { get; set; }
    }
}
