namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides the parameter names for the FoundationaLLM.AzureOpenAI resource provider upsert operations.
    /// </summary>
    public class AzureOpenAIResourceProviderUpsertParameterNames
    {
        /// <summary>
        /// The FoundationaLLM object identifier of the agent.
        /// </summary>
        public const string AgentObjectId = "agent-object-id";

        /// <summary>
        /// The FoundationaLLM conversation identifier.
        /// </summary>
        public const string ConversationId = "conversation-id";

        /// <summary>
        /// The identifier of the OpenAI assistant.
        /// </summary>
        public const string OpenAIAssistantId = "openai-assistant-id";

        /// <summary>
        /// Indicates whether the assistant thread associated with the <see cref="ConversationId"/> must be created.
        /// </summary>
        public const string MustCreateOpenAIAssistantThread = "must-create-openai-assistant-thread";

        /// <summary>
        /// The FoundationaLLM identifier of the attachment.
        /// </summary>
        public const string AttachmentObjectId = "attachment-object-id";

        /// <summary>
        /// Indicates whether the attachment identified by <see cref="AttachmentObjectId"/> must be added to the assistant file store.
        /// </summary>
        public const string MustCreateOpenAIFile = "must-create-openai-file";
    }
}
