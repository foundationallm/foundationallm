namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Represents the result of an upsert operation for an <see cref="AssistantUserContext"/> object.
    /// </summary>
    public class AssistantUserContextUpsertResult : ResourceProviderUpsertResult
    {
        /// <summary>
        /// The identifier of the newly created OpenAI assistant (if any).
        /// </summary>
        public string? NewOpenAIAssistantId { get; set; }

        /// <summary>
        /// The identifier of the newly created OpenAI assistant thread id (if any).
        /// </summary>
        public string? NewOpenAIAssistantThreadId { get; set; }

        /// <summary>
        /// The identifier of the newly created OpenAI assistant vector store id (if any).
        /// </summary>
        public string? NewOpenAIAssistantVectorStoreId { get; set; }
    }
}
