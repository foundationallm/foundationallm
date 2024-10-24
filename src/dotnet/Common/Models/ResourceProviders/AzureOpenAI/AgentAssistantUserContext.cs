using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Provides information about the OpenAI assistant associated with an agent.
    /// </summary>
    public class AgentAssistantUserContext
    {
        /// <summary>
        /// The Azure OpenAI endpoint used to manage the assistant.
        /// </summary>
        [JsonPropertyName("endpoint")]
        [JsonPropertyOrder(101)]
        public required string Endpoint { get; set; }

        /// <summary>
        /// The Azure OpenAI model deployment name used by the assistant.
        /// </summary>
        [JsonPropertyName("model_name")]
        [JsonPropertyOrder(102)]
        public required string ModelDeploymentName { get; set; }

        /// <summary>
        /// The time at which the assistant was created.
        /// </summary>
        [JsonPropertyName("prompt")]
        [JsonPropertyOrder(103)]
        public required string Prompt { get; set; }

        /// <summary>
        /// The OpenAI identifier of the assistant.
        /// </summary>
        [JsonPropertyName("openai_assistant_id")]
        [JsonPropertyOrder(104)]
        public string? OpenAIAssistantId { get; set; }

        /// <summary>
        /// The time at which the assistant was created.
        /// </summary>
        [JsonPropertyName("openai_assistant_created_on")]
        [JsonPropertyOrder(105)]
        public DateTimeOffset? OpenAIAssistantCreatedOn { get; set; }

        /// <summary>
        /// The dictionary of <see cref="ConversationMapping"/> objects providing information about the conversations driven by the OpenAI assistant. 
        /// </summary>
        /// <remarks>
        /// The keys of the dictionary are the FoundationaLLM session identifiers.
        /// </remarks>
        [JsonPropertyName("conversations")]
        [JsonPropertyOrder(106)]
        public Dictionary<string, ConversationMapping> Conversations { get; set; } = [];

        /// <summary>
        /// Gets the incomplete conversations.
        /// </summary>
        /// <returns></returns>
        public List<ConversationMapping> GetIncompleteConversationMappings() =>
            Conversations.Values
                .Where(c => string.IsNullOrWhiteSpace(c.OpenAIThreadId))
                .ToList();
    }
}
