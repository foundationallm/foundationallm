using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Provides information about the OpenAI artifacts associated with a user.
    /// </summary>
    public class AssistantUserContext : AzureOpenAIResourceBase
    {
        /// <summary>
        /// Set default property values.
        /// </summary>
        public AssistantUserContext() =>
            Type = AzureOpenAITypes.AssistantUserContext;

        /// <summary>
        /// The UPN (user principal name) to which the context is associated.
        /// </summary>
        [JsonPropertyName("user_principal_name")]
        [JsonPropertyOrder(100)]
        public required string UserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of <see cref="AgentAssistantUserContext"/> objects providing information about the OpenAI assistants associated with the user.
        /// </summary>
        /// <remarks>
        /// The keys in the dictionary are the object identifiers of the FoundationaLLM agents that are backed by Azure OpenAI Assistants capabilities.
        /// </remarks>
        [JsonPropertyName("agent_assistants")]
        [JsonPropertyOrder(101)]
        public Dictionary<string, AgentAssistantUserContext> AgentAssistants { get; set; } = [];

        /// <summary>
        /// Gets the Azure OpenAI assistant identifier for the specified agent.
        /// </summary>
        /// <param name="agentObjectId">The object identifier of the agent for which were are retrieving the Azure OpenAI assistant.</param>
        /// <param name="openAIAssistantId">The identifier of the Azure OpenAI assistant associated with the agent.</param>
        /// <returns>True if a valid Azure OpenAI assistant identifier is found, False otherwise.</returns>
        public bool TryGetOpenAIAssistantId(string agentObjectId, out string? openAIAssistantId)
        {
            if (AgentAssistants.TryGetValue(agentObjectId, out var agentAssistantUserContext))
            {
                openAIAssistantId = agentAssistantUserContext.OpenAIAssistantId;
                return !string.IsNullOrWhiteSpace(openAIAssistantId);
            }

            openAIAssistantId = null;
            return false;
        }

        /// <summary>
        /// Gets the single <see cref="ConversationMapping"/> that is incomplete (i.e., has no OpenAI thread id).
        /// </summary>
        /// <param name="agentAssistantUserContext">The <see cref="AgentAssistantUserContext"/> containing the incomplete conversation mapping.</param>
        /// <param name="conversationMapping">The <see cref="ConversationMapping"/> that is incomplete.</param>
        /// <returns>True if the incomplete conversation mapping was retrieved, False otherwise.</returns>
        /// <remarks>
        /// The method fails if the assistant user context contains anything else than exactly one incomplete conversation mapping.
        /// It is the responsibility of the caller to decide whether an exception needs to be thrown in case of failure.
        /// </remarks>
        public bool TryGetIncompleteConversation(out AgentAssistantUserContext? agentAssistantUserContext, out ConversationMapping? conversationMapping)
        {
            var contextsWithIncompleteConversations =
                AgentAssistants
                    .Select(a => new
                    {
                        AgentObjectId = a.Key,
                        AgentAssistantUserContext = a.Value,
                        IncompleteConversationMappings = a.Value.GetIncompleteConversationMappings()
                    })
                    .Where(x => x.IncompleteConversationMappings.Count > 0)
                    .ToList();

            if (contextsWithIncompleteConversations.Count != 1
                || contextsWithIncompleteConversations[0].IncompleteConversationMappings.Count != 1)
            {
                agentAssistantUserContext = null;
                conversationMapping = null;
                return false;
            }

            agentAssistantUserContext = contextsWithIncompleteConversations[0].AgentAssistantUserContext;
            conversationMapping = null;
            return false;
        }
    }
}
