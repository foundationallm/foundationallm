using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace FoundationaLLM.SemanticKernel.Core.Plugins
{
    /// <summary>
    /// Provides the capability to provide details about agents.
    /// </summary>
    /// <param name="agentDescriptions">A dictionary with agent names as keys and agent descriptions as values.</param>
    public class AgentConversationPlugin(
        Dictionary<string, string> agentDescriptions)
    {
        private readonly Dictionary<string, string> _agentDescriptions = agentDescriptions;

        /// <summary>
        /// Solves a request by involving other agents into the conversation.
        /// </summary>
        /// <example>
        /// <param name="userPrompt">The text of the request.</param>
        [KernelFunction(name: "AgentDescriptions")]
        public string BuildContextAsync(
            [Description("The text of the request for which we are asking other agents for help.")] string userPrompt)
        {
            var result = new StringBuilder();

            foreach (var agentDescription in _agentDescriptions)
            {
                result.AppendLine($"Name: {agentDescription.Key}");
                result.AppendLine($"Description: {agentDescription.Value}");
            }

            return result.ToString();
        }
    }
}
