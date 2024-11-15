﻿using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides an agent workflow configuration for a LangChain Expression Language workflow.
    /// </summary>
    public class LangChainExpressionLanguageAgentWorkflow: AgentWorkflowBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string Type => AgentWorkflowTypes.LangChainExpressionLanguage;
    }
}
