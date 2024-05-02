using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents the result of a fetch operation.
    /// </summary>
    public class ResourceProviderGetResult
    {
        /// <summary>
        /// List of authorized actions.
        /// </summary>
        [JsonPropertyName("actions")]
        public required List<string> Actions { get; set; }

        /// <summary>
        /// List of role names.
        /// </summary>
        [JsonPropertyName("roles")]
        public required List<string> Roles { get; set; }
    }
}
