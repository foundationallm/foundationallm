using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Provides information about a conversation driven by an OpenAI assistant.
    /// </summary>
    public class ConversationMapping
    {
        /// <summary>
        /// The FoundationaLLM session (conversation) id.
        /// </summary>
        [JsonPropertyName("foundationallm_session_id")]
        public required string FoundationaLLMSessionId { get; set; }

        /// <summary>
        /// The OpenAI thread id associated with the FoundationaLLM session (conversation) id.
        /// </summary>
        [JsonPropertyName("openai_thread_id")]
        public string? OpenAIThreadId { get; set; }

        /// <summary>
        /// The time at which the thread was created.
        /// </summary>
        [JsonPropertyName("openai_thread_created_on")]
        public DateTimeOffset? OpenAIThreadCreatedOn { get; set; }

        /// <summary>
        /// The OpenAI vector store id associated with the FoundationaLLM session (conversation) id.
        /// </summary>
        [JsonPropertyName("openai_vector_store_id")]
        public string? OpenAIVectorStoreId { get; set; }
    }
}
