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

        /// <summary>
        /// When using LangGraph, the recursion limit sets the number of supersteps that the graph is allowed
        /// to execute before it raises an error. The default value is 25. Set this value to null to use the default.
        /// </summary>
        [JsonPropertyName("graph_recursion_limit")]
        public int? GraphRecursionLimit { get; set; }
    }
}
