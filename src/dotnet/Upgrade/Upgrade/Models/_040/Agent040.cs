﻿using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Utility.Upgrade.Models._040
{

    /// <summary>
    /// Base agent metadata model.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(KnowledgeManagementAgent040), "knowledge-management")]
    [JsonDerivedType(typeof(InternalContextAgent040), "internal-context")]
    public class Agent040 : ResourceBase040
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// The agent's language model configuration.
        /// </summary>
        [JsonPropertyName("language_model")]
        public LanguageModel040? LanguageModel { get; set; }
        /// <summary>
        /// Indicates whether sessions are enabled for the agent.
        /// </summary>
        [JsonPropertyName("sessions_enabled")]
        public bool SessionsEnabled { get; set; }
        /// <summary>
        /// The agent's conversation history configuration.
        /// </summary>
        [JsonPropertyName("conversation_history")]
        public ConversationHistory040? ConversationHistory { get; set; }
        /// <summary>
        /// The agent's Gatekeeper configuration.
        /// </summary>
        [JsonPropertyName("gatekeeper")]
        public Gatekeeper040? Gatekeeper { get; set; }
        /// <summary>
        /// The agent's LLM orchestrator type.
        /// </summary>
        [JsonPropertyName("orchestrator")]
        public string? Orchestrator { get; set; }
        /// <summary>
        /// The agent's prompt.
        /// </summary>
        [JsonPropertyName("prompt_object_id")]
        public string? PromptObjectId { get; set; }

        /// <summary>
        /// The object type of the agent.
        /// </summary>
        [JsonIgnore]
        public Type AgentType =>
            Type switch
            {
                AgentTypes040.KnowledgeManagement => typeof(KnowledgeManagementAgent),
                AgentTypes040.InternalContext => typeof(InternalContextAgent040),
                _ => throw new ResourceProviderException($"The agent type {Type} is not supported.")
            };
    }

    /// <summary>
    /// Agent conversation history settings.
    /// </summary>
    public class ConversationHistory040
    {
        /// <summary>
        /// Indicates whether the conversation history is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        /// <summary>
        /// The maximum number of turns to store in the conversation history.
        /// </summary>
        [JsonPropertyName("max_history")]
        public int MaxHistory { get; set; }
    }

    /// <summary>
    /// Agent Gatekeeper settings.
    /// </summary>
    public class Gatekeeper040
    {
        /// <summary>
        /// Indicates whether to abide by or override the system settings for the Gatekeeper.
        /// </summary>
        [JsonPropertyName("use_system_setting")]
        public bool UseSystemSetting { get; set; }
        /// <summary>
        /// If <see cref="UseSystemSetting"/> is false, provides Gatekeeper feature selection.
        /// </summary>
        [JsonPropertyName("options")]
        public string[]? Options { get; set; }
    }
}
