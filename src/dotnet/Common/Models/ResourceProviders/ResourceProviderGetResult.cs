using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents the result of a fetch operation.
    /// </summary>
    public class ResourceProviderGetResult<T> where T : ResourceBase
    {
        /// <summary>
        /// The resource resulting from the fetch operation.
        /// </summary>
        [JsonPropertyName("resource")]
        public required T Resource { get; set; }

        /// <summary>
        /// List of roles on the resource.
        /// </summary>
        [JsonPropertyName("roles")]
        public required List<string> Roles { get; set; }
    }
}
