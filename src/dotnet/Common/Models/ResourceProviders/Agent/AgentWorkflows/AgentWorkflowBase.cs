namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides a workflow configuration for an agent.
    /// </summary>
    public class AgentWorkflowBase
    {
        /// <summary>
        /// The workflow resource associated with the agent.
        /// </summary>
        public required string WorkflowObjectId { get; set; }

        /// <summary>
        /// The collection of AI models available to the workflow.
        /// The well-known key "main-model" is used to specify the model for the main workflow.
        /// </summary>
        public Dictionary<string, string> AIModelObjectIds { get; set; } = [];

        /// <summary>
        /// The collection of prompt resources available to the workflow.
        /// The well-known key "main-prompt" is used to specify the prompt for the main workflow.
        /// </summary>
        public Dictionary<string, string> PromptObjectIds { get; set; } = [];
    }
}
