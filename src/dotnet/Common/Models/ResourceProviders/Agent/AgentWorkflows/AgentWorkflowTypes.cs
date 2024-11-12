namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    ///  Contains constants for the types of agent workflows.
    /// </summary>
    public static class AgentWorkflowTypes
    {
        /// <summary>
        /// The OpenAI Assistants agent workflow.
        /// </summary>
        public const string OpenAIAssistants = "openai-assistants-workflow";

        /// <summary>
        /// The LangChain LCEL agent workflow.
        /// </summary>
        public const string LangChainLCEL = "lang-chain-lcel-workflow";
    }
}
