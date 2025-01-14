using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using OpenAI.Chat;

namespace FoundationaLLM.Orchestration.Core.Models
{
    /// <summary>
    /// Provide the capability to rewrite user prompts for agents.
    /// </summary>
    public class AgentUserPromptRewriter
    {
        /// <summary>
        /// Gets or sets the agent's user prompt rewrite settings.
        /// </summary>
        public required AgentUserPromptRewriteSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the system prompt to be used for rewriting user prompts.
        /// </summary>
        public required string RewriterSystemPrompt { get; set; }

        /// <summary>
        /// Gets or sets the Azure OpenAI chat client used for rewriting.
        /// </summary>
        public required ChatClient ChatClient { get; set; }
    }
}
