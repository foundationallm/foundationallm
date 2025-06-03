using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Global
{
    /// <summary>
    /// Represents a management action for a resource provider.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(ResourceProviderSendEventAction), ResourceProviderManagementActionTypes.SendEvent)]
    public class ResourceProviderManagementAction
    {
        /// <summary>
        /// Gets or sets the type of the management action.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(-100)]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the target service for the management action.
        /// </summary>
        /// <remarks>
        /// If this property is set, it indicates that the action is intended for a specific service.
        /// The management be handled only by instances of the resource provider that are hosted by the specified service.
        /// </remarks>
        [JsonPropertyName("target_service_name")]
        [JsonPropertyOrder(-99)]
        public string? TargetServiceName { get; set; }
    }
}
