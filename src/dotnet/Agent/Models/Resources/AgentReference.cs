using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentTemplates;
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
        public override Type ResourceType =>
            Type switch
            {
                AgentTypes.Basic => typeof(AgentBase),
                AgentTypes.GenericAgent => typeof(GenericAgent),
                AgentTypes.Workflow => typeof(Workflow),
                AgentTypes.Tool => typeof(Tool),
                AgentTypes.AgentTemplate => typeof(AgentTemplate),
                _ => throw new ResourceProviderException($"The agent type {Type} is not supported.")
            };
    }
}
