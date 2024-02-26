using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Metadata;
using System.Text.Json.Serialization;
using FoundationaLLM.Common.Models.Orchestration.DataSources;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Orchestration completion request.
    /// Contains the metadata needed by the orchestration services
    /// to build and execute completions.
    /// </summary>
    public class LegacyCompletionRequest : LLMCompletionRequest
    {
        /// <summary>
        /// Agent metadata
        /// </summary>
        [JsonPropertyName("agent")]
        public LegacyAgentMetadata? Agent { get; set; }

        /// <summary>
        /// Data source metadata
        /// </summary>
        [JsonPropertyName("data_sources")]
        public List<DataSourceBase>? DataSourceMetadata { get; set; }

        /// <summary>
        /// Language model metadata.
        /// </summary>
        [JsonPropertyName("language_model")]
        public LanguageModel? LanguageModel { get; set; }

        /// <summary>
        /// Embedding model metadata.
        /// </summary>
        [JsonPropertyName("embedding_model")]
        public EmbeddingModel? EmbeddingModel { get; set; }

        /// <summary>
        /// Message history list
        /// </summary>
        [JsonPropertyName("message_history")]
        public List<MessageHistoryItem>? MessageHistory { get; set; }
    }
}
