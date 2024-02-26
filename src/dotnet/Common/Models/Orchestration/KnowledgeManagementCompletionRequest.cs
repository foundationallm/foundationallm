using FoundationaLLM.Common.Models.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// The Knowledge Management Completion Request model.
    /// </summary>
    public class KnowledgeManagementCompletionRequest : LLMCompletionRequest
    {
        /// <summary>
        /// The Knowledge Management agent metadata.
        /// </summary>
        [JsonPropertyName("agent")]
        public required KnowledgeManagementAgent Agent { get; set; }
    }
}
