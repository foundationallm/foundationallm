using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models
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
