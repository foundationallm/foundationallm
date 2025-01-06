using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Provides agent-related cache settings for the semantic cache.
    /// </summary>
    public class AgentSemanticCacheSettings
    {
        /// <summary>
        /// Gets or sets the maximum number of tokens to use for the conversation context.
        /// </summary>
        [JsonPropertyName("conversation_context_max_tokens")]
        public int ConversationContextMaxTokens { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the AI model to use for the embedding.
        /// </summary>
        [JsonPropertyName("embedding_ai_model_object_id")]
        public required string EmbeddingAIModelObjectId { get; set; }

        /// <summary>
        /// Gets or sets the number of dimensions to use for the embedding.
        /// </summary>
        [JsonPropertyName("embedding_dimensions")]
        public int EmbeddingDimensions { get; set; }

        /// <summary>
        /// Gets or sets the minimum similarity threshold for the semantic cache.
        /// </summary>
        /// <remarks>
        /// This value determines the minimum similarity between the current conversation context
        /// and the context of the item in the cache for the item to be considered a match.
        /// </remarks>
        [JsonPropertyName("minimum_similarity_threshold")]
        public decimal MinimumSimilarityThreshold { get; set; } = 0.975m;
    }
}
