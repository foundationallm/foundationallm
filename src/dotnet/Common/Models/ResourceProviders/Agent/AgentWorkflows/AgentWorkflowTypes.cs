﻿namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    ///  Contains constants for the types of agent workflows.
    /// </summary>
    public static class AgentWorkflowTypes
    {
        /// <summary>
        /// The OpenAI Assistants agent workflow.
        /// </summary>
        public const string AzureOpenAIAssistants = "azure-openai-assistants-workflow";

        /// <summary>
        /// The LangChain Expression Language agent workflow.
        /// </summary>
        public const string LangChainExpressionLanguage = "langchain-expression-language-workflow";
    }
}
