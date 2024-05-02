namespace FoundationaLLM.Orchestration.Core.Models
{
    /// <summary>
    /// Provides details about a single step in an agent conversation flow.
    /// </summary>
    public class AgentConversationStep
    {
        /// <summary>
        /// The unique name of the agent whose turn is to respond.
        /// </summary>
        public required string AgentName { get; set; }

        /// <summary>
        /// The user prompt to be used in the conversation step.
        /// </summary>
        public required string UserPrompt { get; set; }
    }
}
