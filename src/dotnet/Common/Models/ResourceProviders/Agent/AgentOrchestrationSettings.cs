using FoundationaLLM.Common.Constants.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Provides agent-related orchestration settings.
    /// </summary>
    public class AgentOrchestrationSettings
    {
        /// <summary>
        /// The agent's LLM orchestrator type.
        /// </summary>
        [JsonPropertyName("orchestrator")]
        public string? Orchestrator { get; set; }

        /// <summary>
        /// Dictionary with override values for the agent parameters.
        /// <para>
        /// For the list of supported keys, see <see cref="AgentParametersKeys"/>.
        /// </para>
        /// </summary>
        [JsonPropertyName("agent_parameters")]
        public Dictionary<string, object>? AgentParameters { get; set; }
    }
}
