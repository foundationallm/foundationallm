using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.ResourceProvider;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Agent.Models.Resources
{
    /// <summary>
    /// Provides details about an agent.
    /// </summary>
    public class AgentReference : ResourceReference
    {
        /// <summary>
        /// The object type of the agent.
        /// </summary>
        [JsonIgnore]
        public Type AgentType =>
            Type switch
            {
                AgentTypes.Basic => typeof(AgentBase),
                AgentTypes.KnowledgeManagement => typeof(KnowledgeManagementAgent),
                AgentTypes.InternalContext => typeof(InternalContextAgent),
                _ => throw new ResourceProviderException($"The agent type {Type} is not supported.")
            };
    }
}
