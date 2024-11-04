namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Represents the result of an upsert operation for an <see cref="AzureOpenAIConversationMapping"/> object.
    /// </summary>
    public class AzureOpenAIConversationMappingUpsertResult : ResourceProviderUpsertResult<AzureOpenAIConversationMapping>
    {
        /// <summary>
        /// The identifier of the newly created OpenAI assistant thread id (if any).
        /// </summary>
        public string? NewOpenAIAssistantThreadId { get; set; }

        /// <summary>
        /// The identifier of the newly created OpenAI assistant vector store id (if any).
        /// </summary>
        public string? NewOpenAIVectorStoreId { get; set; }
    }
}
