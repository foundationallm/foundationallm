using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Global
{
    /// <summary>
    /// Represents a management action to send an event to a resource provider.
    /// </summary>
    public class ResourceProviderSendEventAction : ResourceProviderManagementAction
    {
        /// <summary>
        /// Gets or sets the type of the event to be sent.
        /// </summary>
        [JsonPropertyName("event_type")]
        [JsonPropertyOrder(1)]
        public required string EventType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceProviderSendEventAction"/> class.
        /// </summary>
        public ResourceProviderSendEventAction() =>
            Type = ResourceProviderManagementActionTypes.SendEvent;
    }
}
