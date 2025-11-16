using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides a workflow configuration for an agent.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(GenericAgentWorkflow), AgentWorkflowTypes.GenericAgentWorkflow)]
    [JsonDerivedType(typeof(AzureAIAgentServiceAgentWorkflow), AgentWorkflowTypes.AzureAIAgentService)]
    [JsonDerivedType(typeof(AzureOpenAIAssistantsAgentWorkflow), AgentWorkflowTypes.AzureOpenAIAssistants)]
    [JsonDerivedType(typeof(LangChainAgentWorkflow), AgentWorkflowTypes.LangChainAgentWorkflow)]
    [JsonDerivedType(typeof(LangChainExpressionLanguageAgentWorkflow), AgentWorkflowTypes.LangChainExpressionLanguage)]
    [JsonDerivedType(typeof(LangGraphReactAgentWorkflow), AgentWorkflowTypes.LangGraphReactAgent)]
    [JsonDerivedType(typeof(ExternalAgentWorkflow), AgentWorkflowTypes.ExternalAgentWorkflow)]
    public class AgentWorkflowBase
    {        
        /// <summary>
        /// The workflow resource associated with the agent.
        /// </summary>
        [JsonPropertyName("type")]
        public virtual string? Type { get; set; }

        /// <summary>
        /// The name of the workflow.
        /// </summary>        
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the package name of the workflow.
        /// For internal workflows, this value will be FoundationaLLM
        /// For external workflows, this value will be the name of the package.
        /// </summary>
        [JsonPropertyName("package_name")]
        public required string PackageName { get; set; }

        /// <summary>
        /// The class name of the workflow.
        /// For internal workflows, this value will be FoundationaLLM
        /// For external workflows, this value will be the name of the implementation class.
        /// </summary>
        [JsonPropertyName("class_name")]
        public string? ClassName
        {
            get => string.IsNullOrWhiteSpace(_className) ? Name : _className;
            set => _className = value ?? string.Empty;
        }
        private string _className = string.Empty;

        /// <summary>
        /// The host of the workflow environment.
        /// </summary>
        [JsonPropertyName("workflow_host")]
        public string? WorkflowHost { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of resource objects.
        /// </summary>
        [JsonPropertyName("resource_object_ids")]
        public Dictionary<string, ResourceObjectIdProperties> ResourceObjectIds { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of properties that are specific to the workflow.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object>? Properties { get; set; } = [];

        /// <summary>
        /// Gets the main AI model object identifier.
        /// </summary>
        [JsonIgnore]
        public string? MainAIModelObjectId =>
            ResourceObjectIds.Values
                .FirstOrDefault(
                    roid => roid.HasObjectRole(ResourceObjectIdPropertyValues.MainModel))
                ?.ObjectId;

        /// <summary>
        /// Gets the main prompt object identifier.
        /// </summary>
        [JsonIgnore]
        public string? MainPromptObjectId =>
            ResourceObjectIds.Values
                .FirstOrDefault(
                    roid => roid.HasObjectRole(ResourceObjectIdPropertyValues.MainPrompt))
                ?.ObjectId;

        /// <summary>
        /// Gets the router prompt object identifier.
        /// </summary>
        [JsonIgnore]
        public string? RouterPromptObjectId =>
            ResourceObjectIds.Values
                .FirstOrDefault(
                    roid => roid.HasObjectRole(ResourceObjectIdPropertyValues.RouterPrompt))
                ?.ObjectId;

        /// <summary>
        /// Gets the files prompt object identifier.
        /// </summary>
        [JsonIgnore]
        public string? FilesPromptObjectId =>
            ResourceObjectIds.Values
                .FirstOrDefault(
                    roid => roid.HasObjectRole(ResourceObjectIdPropertyValues.FilesPrompt))
                ?.ObjectId;

        /// <summary>
        /// Gets the final prompt object identifier.
        /// </summary>
        [JsonIgnore]
        public string? FinalPromptObjectId =>
            ResourceObjectIds.Values
                .FirstOrDefault(
                    roid => roid.HasObjectRole(ResourceObjectIdPropertyValues.FinalPrompt))
                ?.ObjectId;

        /// <summary>
        /// Gets the AI object identifier.
        /// </summary>
        [JsonIgnore]
        public string? AIProjectObjectId =>
            ResourceObjectIds.Values
                .FirstOrDefault(
                    roid => roid.HasObjectRole(ResourceObjectIdPropertyValues.AIProject))
                ?.ObjectId;
    }
}
