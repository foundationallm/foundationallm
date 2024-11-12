namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides an agent workflow configuration for an OpenAI Assistants workflow.
    /// </summary>
    public class OpenAIAssistantsAgentWorkflow: AgentWorkflowBase
    {
        /// <summary>
        /// The OpenAI Assistant ID for the agent workflow.
        /// </summary>
        public required string AssistantId { get; set; }
    }
}
