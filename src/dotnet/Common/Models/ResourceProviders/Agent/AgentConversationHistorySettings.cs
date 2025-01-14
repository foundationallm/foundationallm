using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Provides agent-related conversation history settings.
    /// </summary>
    public class AgentConversationHistorySettings
    {
        /// <summary>
        /// Indicates whether the conversation history is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// The maximum number of turns to store in the conversation history.
        /// </summary>
        [JsonPropertyName("max_history")]
        public int MaxHistory { get; set; }

        /// <summary>
        /// Gets or sets the comma separated list of content artifact types to store in the conversation history.
        /// </summary>
        [JsonPropertyName("history_content_artifact_types")]
        public string? HistoryContentArtifactTypes { get; set; }
    }
}
