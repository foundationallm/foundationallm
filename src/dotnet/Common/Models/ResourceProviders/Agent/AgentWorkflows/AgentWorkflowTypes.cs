namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    ///  Contains constants for the types of agent workflows.
    /// </summary>
    public static class AgentWorkflowTypes
    {
        /// <summary>
        /// The generic agent workflow.
        /// </summary>
        public const string GenericAgentWorkflow = "generic-agent-workflow";

        /// <summary>
        /// The Azure AI Agent Service agent workflow.
        /// </summary>
        public const string AzureAIAgentService = "azure-ai-agent-service-workflow";

        /// <summary>
        /// The OpenAI Assistants agent workflow.
        /// </summary>
        public const string AzureOpenAIAssistants = "azure-openai-assistants-workflow";

        /// <summary>
        /// The LangChain built-in ReAct Agent workflow.
        /// </summary>
        public const string LangChainAgentWorkflow = "langchain-agent-workflow";

        /// <summary>
        /// The LangChain Expression Language agent workflow.
        /// </summary>
        public const string LangChainExpressionLanguage = "langchain-expression-language-workflow";

        /// <summary>
        /// The LangGraph ReAct agent workflow.
        /// </summary>
        [Obsolete("This workflow type is deprecated and will be removed in future versions. Please use LangChainAgentWorkflow instead.")]
        public const string LangGraphReactAgent = "langgraph-react-agent-workflow";

        /// <summary>
        /// The External Agent workflow.
        /// </summary>
        public const string ExternalAgentWorkflow = "external-agent-workflow";
    }
}
