﻿using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
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
                AgentTypes.KnowledgeManagement => typeof(KnowledgeManagementAgent),
                _ => throw new ResourceProviderException($"The agent type {Type} is not supported.")
            };
    }
}
