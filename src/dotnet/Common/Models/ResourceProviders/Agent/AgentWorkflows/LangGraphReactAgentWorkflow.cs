using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides an agent workflow configuration for a LangGraph ReAct Agent workflow.
    /// </summary>
    public class LangGraphReactAgentWorkflow : AgentWorkflowBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string Type => AgentWorkflowTypes.LangGraphReactAgent;
    }
}
