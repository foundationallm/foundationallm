using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents the result of a fetch operation.
    /// </summary>
    public class ResourceProviderGetResult<T> where T : ResourceBase
    {
        /// <summary>
        /// Gets or sets the resource resulting from the fetch operation.
        /// </summary>
        [JsonPropertyName("resource")]
        public required T Resource { get; set; }

        /// <summary>
        /// Gets or sets the list of roles assigned directly or indirectly to the resource.
        /// </summary>
        [JsonPropertyName("roles")]
        public required List<string> Roles { get; set; }

        /// <summary>
        /// Gets or sets the list of authorizable actions assigned directly or indirectly to the resource.
        /// </summary>
        [JsonPropertyName("actions")]
        public required List<string> Actions { get; set; }

        /// <summary>
        /// Gets or sets a dictionary property bag associated with the resource.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = [];
    }
}
