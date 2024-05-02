using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Represents a vectorization step in a vectorization request.
    /// </summary>
    public class VectorizationStep
    {
        /// <summary>
        /// The identifier of the step.
        /// </summary>
        [JsonPropertyOrder(0)]
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// Dictionary-based configuration for the step.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("parameters")]
        public required Dictionary<string, string> Parameters { get; set; }
    }
}
