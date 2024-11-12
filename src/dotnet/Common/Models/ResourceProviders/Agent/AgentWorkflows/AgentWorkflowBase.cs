using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides a workflow configuration for an agent.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(AzureOpenAIAssistantsAgentWorkflow), AgentWorkflowTypes.AzureOpenAIAssistants)]
    [JsonDerivedType(typeof(LangChainLCELAgentWorkflow), AgentWorkflowTypes.LangChainLCEL)]
    public class AgentWorkflowBase
    {        
        /// <summary>
        /// The workflow resource associated with the agent.
        /// </summary>
        [JsonPropertyName("type")]
        public virtual string? Type { get; set; }

        /// <summary>
        /// The workflow resource associated with the agent.
        /// </summary>
        [JsonPropertyName("workflow_object_id")]
        public required string WorkflowObjectId { get; set; }

        /// <summary>
        /// The name of the workflow resource associated with the agent.
        /// </summary>
        [JsonPropertyName("workflow_name")]
        public required string WorkflowName { get; set; }

        /// <summary>
        /// The collection of AI models available to the workflow.
        /// The well-known key "main-model" is used to specify the model for the main workflow.
        /// </summary>
        [JsonPropertyName("ai_model_object_ids")]
        public Dictionary<string, string> AIModelObjectIds { get; set; } = [];

        /// <summary>
        /// The collection of prompt resources available to the workflow.
        /// The well-known key "main-prompt" is used to specify the prompt for the main workflow.
        /// </summary>
        [JsonPropertyName("prompt_object_ids")]
        public Dictionary<string, string> PromptObjectIds { get; set; } = [];
    }
}
