using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Configuration settings for realtime speech functionality.
    /// Follows the same pattern as AgentSemanticCacheSettings and AgentUserPromptRewriteSettings.
    /// </summary>
    public class RealtimeSpeechSettings
    {
        /// <summary>
        /// Whether realtime speech is enabled for this agent.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the object identifier of the AI model to use for realtime speech.
        /// This follows the same pattern as EmbeddingAIModelObjectId in AgentSemanticCacheSettings.
        /// </summary>
        [JsonPropertyName("realtime_speech_ai_model_object_id")]
        public required string RealtimeSpeechAIModelObjectId { get; set; }

        /// <summary>
        /// Stop words that will terminate the realtime session.
        /// </summary>
        [JsonPropertyName("stop_words")]
        public List<string> StopWords { get; set; } = new() { "stop", "end conversation", "goodbye" };

        /// <summary>
        /// Maximum duration of a realtime session in seconds (0 = unlimited).
        /// </summary>
        [JsonPropertyName("max_session_duration_seconds")]
        public int MaxSessionDurationSeconds { get; set; } = 0;

        /// <summary>
        /// Whether to show transcriptions in the chat thread.
        /// </summary>
        [JsonPropertyName("show_transcriptions")]
        public bool ShowTranscriptions { get; set; } = true;

        /// <summary>
        /// Whether to include conversation history in the realtime session context.
        /// </summary>
        [JsonPropertyName("include_conversation_history")]
        public bool IncludeConversationHistory { get; set; } = true;
    }
}
