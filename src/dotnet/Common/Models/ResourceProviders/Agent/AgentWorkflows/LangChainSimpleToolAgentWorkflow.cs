using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides an agent workflow configuration for a LangChain simple tool workflow.
    /// </summary>
    public class LangChainSimpleToolAgentWorkflow : AgentWorkflowBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string Type => AgentWorkflowTypes.LangChainSimpleTool;
    }
}
