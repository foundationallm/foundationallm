using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides an agent workflow configuration for a Generic Agent workflow implemented by a plugin.
    /// </summary>
    public class GenericAgentWorkflow : AgentWorkflowBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string Type => AgentWorkflowTypes.GenericAgentWorkflow;
    }
}
