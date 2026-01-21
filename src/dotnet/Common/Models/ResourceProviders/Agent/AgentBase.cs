using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Base agent metadata model.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(GenericAgent), "generic-agent")]
    public class AgentBase : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// Indicates whether sessions are enabled for the agent.
        /// </summary>
        [JsonPropertyName("sessions_enabled")]
        public bool SessionsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the agent's text rewrite settings.
        /// </summary>
        [JsonPropertyName("text_rewrite_settings")]
        public AgentTextRewriteSettings? TextRewriteSettings { get; set; }

        /// <summary>
        /// Gets or sets the agent's caching settings.
        /// </summary>
        [JsonPropertyName("cache_settings")]
        public AgentCacheSettings? CacheSettings { get; set; }

        /// <summary>
        /// The agent's conversation history configuration.
        /// </summary>
        [JsonPropertyName("conversation_history_settings")]
        public AgentConversationHistorySettings? ConversationHistorySettings { get; set; }

        /// <summary>
        /// The agent's Gatekeeper configuration.
        /// </summary>
        [JsonPropertyName("gatekeeper_settings")]
        public AgentGatekeeperSettings? GatekeeperSettings { get; set; }

        /// <summary>
        /// The agent's workflow configuration.
        /// </summary>
        [JsonPropertyName("workflow")]
        public AgentWorkflowBase? Workflow { get; set; }

        /// <summary>
        /// Configuration for realtime speech sessions.
        /// </summary>
        [JsonPropertyName("realtime_speech_settings")]
        public RealtimeSpeechSettings? RealtimeSpeechSettings { get; set; }

        /// <summary>
        /// Configuration for procedural memory capabilities (skill learning).
        /// When null or when Enabled is false, the Code Interpreter tool works in 
        /// backwards-compatible mode without any skill functionality.
        /// </summary>
        [JsonPropertyName("procedural_memory_settings")]
        public ProceduralMemorySettings? ProceduralMemorySettings { get; set; }

        /// <summary>
        /// Gets or sets a list of tools that are registered with the agent.
        /// </summary>
        /// <remarks>
        /// The key is the name of the tool, and the value is the <see cref="AgentTool"/> object.
        /// </remarks>
        [JsonPropertyName("tools")]
        public AgentTool[] Tools { get; set; } = [];

        /// <summary>
        /// Gets or sets the object identifier of the virtual security group associated with the agent.
        /// </summary>\
        /// <remarks>
        /// The virtual security group is used to provide access to the agent for the virtual security principals.
        /// An example of such a virtual security principal is the virtual identity associated with an
        /// agent access token.
        /// </remarks>
        [JsonPropertyName("virtual_security_group_id")]
        public string? VirtualSecurityGroupId { get; set; }

        /// <summary>
        /// Indicates whether to show a token count on the messages.
        /// </summary>
        [JsonPropertyName("show_message_tokens")]
        public bool? ShowMessageTokens { get; set; } = true;

        /// <summary>
        /// Indicates whether to show rating options on the messages.
        /// </summary>
        [JsonPropertyName("show_message_rating")]
        public bool? ShowMessageRating { get; set; } = true;

        /// <summary>
        /// Indicates whether to show a view prompt option on agent messages.
        /// </summary>
        [JsonPropertyName("show_view_prompt")]
        public bool? ShowViewPrompt { get; set; } = true;

        /// <summary>
        /// Indicates whether to show the file upload option on agent message input.
        /// </summary>
        [JsonPropertyName("show_file_upload")]
        public bool? ShowFileUpload { get; set; } = true;

        /// <summary>
        /// Gets or sets the user identifier of the agent owner.
        /// </summary>
        [JsonPropertyName("owner_user_id")]
        public string? OwnerUserId { get; set; }

        /// <summary>
        /// The object type of the agent.
        /// </summary>
        [JsonIgnore]
        public Type AgentType =>
            Type switch
            {
                AgentTypes.GenericAgent => typeof(GenericAgent),
                _ => throw new ResourceProviderException($"The agent type {Type} is not supported.")
            };

        /// <summary>
        /// Checks whether the agent uses an Azure OpenAI Assistants workflow.
        /// </summary>
        /// <returns>True if the agent uses an Azure OpenAI Assistants workflow, False otherwise.</returns>
        public bool HasAzureOpenAIAssistantsWorkflow() =>
            Workflow != null && Workflow is AzureOpenAIAssistantsAgentWorkflow;

        /// <summary>
        /// Checks whether the agent uses an Azure AI Agent Service workflow.
        /// </summary>
        /// <returns></returns>
        public bool HasAzureAIAgentServiceWorkflow() =>
            Workflow != null && Workflow is AzureAIAgentServiceAgentWorkflow;

        /// <summary>
        /// Checks whether the agent uses an external workflow.
        /// </summary>
        /// <returns></returns>
        public bool HasGenericWorkflow() =>
            Workflow != null
            && (
                Workflow is GenericAgentWorkflow
                || Workflow is LangChainAgentWorkflow
                || Workflow is LangChainExpressionLanguageAgentWorkflow
                || Workflow is LangGraphReactAgentWorkflow
                || Workflow is ExternalAgentWorkflow
            );

        /// <summary>
        /// Indicates whether realtime speech capabilities are available for this agent.
        /// </summary>
        [JsonIgnore]
        public bool HasRealtimeSpeechCapabilities =>
            RealtimeSpeechSettings?.Enabled == true &&
            !string.IsNullOrWhiteSpace(RealtimeSpeechSettings?.RealtimeSpeechAIModelObjectId);

        /// <summary>
        /// Indicates whether procedural memory (skill learning) is enabled for this agent.
        /// </summary>
        [JsonIgnore]
        public bool HasProceduralMemoryEnabled =>
            ProceduralMemorySettings?.Enabled == true;
    }
}
