namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Provides the names of the properties allowed in the agent resource property bag.
    /// </summary>
    public static class AgentPropertyNames
    {
        /// <summary>
        /// The identifier of the Azure OpenAI Assistants assistant.
        /// </summary>
        public const string AzureOpenAIAssistantId = "Azure.OpenAI.Assistant.Id";

        /// <summary>
        /// The identifier of the Azure OpenAI Assistant level vector store (global at the agent level).
        /// </summary>
        public const string AzureOpenAIAssistantVectorStoreId = "Azure.OpenAI.Assistant.VectorStoreId";

        /// <summary>
        /// The name of the property that indicates whether the agent is enabled by the user.
        /// </summary>
        public const string Enabled = "enabled";
    }
}
