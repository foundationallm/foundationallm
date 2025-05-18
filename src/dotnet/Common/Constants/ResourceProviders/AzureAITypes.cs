namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Defines the resource types for Azure AI service resources.
    /// </summary>
    public static class AzureAITypes
    {
        /// <summary>
        /// Defines a mapping between Azure AI Agent and FoundationaLLM conversation-related resources.
        /// </summary>        
        public const string AgentConversationMapping = "AzureAIAgentConversationMapping";

        /// <summary>
        /// Defines a mapping between Azure AI Agent and FoundationaLLM file-related resources.
        /// </summary>
        public const string AgentFileMapping = "AzureAIAgentFileMapping";

        /// <summary>
        /// The Azure AI Foundry project type.
        /// </summary>
        /// <remarks>Defined lowercase and hyphenated to be consistent with other storage persisted resources.</remarks>
        public const string Project = "azure-ai-project";
    }
}
