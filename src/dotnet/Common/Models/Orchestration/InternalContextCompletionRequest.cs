using FoundationaLLM.Common.Models.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// The Internal Context Completion Request model.
    /// </summary>
    public class InternalContextCompletionRequest : LLMCompletionRequest
    {
        /// <summary>
        /// The Internal Context agent metadata.
        /// </summary>
        [JsonPropertyName("agent")]
        public required InternalContextAgent Agent { get; set; }
    }
}
