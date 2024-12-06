using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides a workflow configuration for an agent.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(AzureOpenAIAssistantsAgentWorkflow), AgentWorkflowTypes.AzureOpenAIAssistants)]
    [JsonDerivedType(typeof(LangChainExpressionLanguageAgentWorkflow), AgentWorkflowTypes.LangChainExpressionLanguage)]
    [JsonDerivedType(typeof(LangGraphReactAgentWorkflow), AgentWorkflowTypes.LangGraphReactAgent)]
    public class AgentWorkflowBase
    {        
        /// <summary>
        /// The workflow resource associated with the agent.
        /// </summary>
        [JsonPropertyName("type")]
        public virtual string? Type { get; set; }

        /// <summary>
        /// The name of the workflow resource associated with the agent.
        /// </summary>
        [JsonPropertyName("workflow_name")]
        public required string WorkflowName { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of resource objects.
        /// </summary>
        [JsonPropertyName("resource_object_ids")]
        public Dictionary<string, ResourceObjectIdProperties> ResourceObjectIds { get; set; } = [];

        /// <summary>
        /// The collection of prompt resources available to the workflow.
        /// The well-known key "main-prompt" is used to specify the prompt for the main workflow.
        /// </summary>
        [JsonPropertyName("prompt_object_ids")]
        public Dictionary<string, string> PromptObjectIds { get; set; } = [];
    }
}
