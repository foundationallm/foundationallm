using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Provides details about a conversation mapping between FoundationaLLM and Azure OpenAI Assistants.
    /// </summary>
    public class AzureOpenAIConversationMapping : AzureOpenAIResourceBase
    {
        /// <summary>
        /// The FoundationaLLM conversation (session) id.
        /// </summary>
        [JsonPropertyName("conversation_id")]
        public required string ConversationId { get; set; }

        /// <summary>
        /// The Azure OpenAI Assistants assistant id.
        /// </summary>
        [JsonPropertyName("openai_assistants_assistant_id")]
        public required string OpenAIAssistantsAssistantId { get; set; }

        /// <summary>
        /// The Azure OpenAI Assistants thread id associated with the FoundationaLLM conversation (session) id.
        /// </summary>
        [JsonPropertyName("openai_assistants_thread_id")]
        public string? OpenAIAssistantsThreadId { get; set; }

        /// <summary>
        /// The time at which the Azure OpenAI Assistants thread was created.
        /// </summary>
        [JsonPropertyName("openai_assistants_thread_created_on")]
        public DateTimeOffset? OpenAIAssistantsThreadCreatedOn { get; set; }

        /// <summary>
        /// The Azure OpenAI vector store id associated with the FoundationaLLM session (conversation) id.
        /// </summary>
        [JsonPropertyName("openai_vector_store_id")]
        public string? OpenAIVectorStoreId { get; set; }

        /// <summary>
        /// The time at which the Azure OpenAI vector store was created.
        /// </summary>
        [JsonPropertyName("openai_vector_store_created_on")]
        public DateTimeOffset? OpenAIVectorStoreCreatedOn { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public AzureOpenAIConversationMapping() =>
            Type = AzureOpenAITypes.ConversationMapping;
    }
}
