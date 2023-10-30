using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Orchestration;
using System.ComponentModel;

namespace FoundationaLLM.SemanticKernel.Skills.Core
{
    /// <summary>
    /// Generic Summarizer Skill class.
    /// </summary>
    public class GenericSummarizerSkill
    {
        private readonly IKernel _kernel;
        private readonly ISKFunction _summarizeConversation;

        /// <summary>
        /// Constructor for the Generic Summarizer Skill.
        /// </summary>
        /// <param name="promptTemplate">The prompt template.</param>
        /// <param name="maxTokens">The maximum number of tokens.</param>
        /// <param name="kernel">The Semantic Kernel service.</param>
        public GenericSummarizerSkill(
            string promptTemplate,
            int maxTokens,
            IKernel kernel)
        {
            _kernel = kernel;

            _summarizeConversation = kernel.CreateSemanticFunction(
                promptTemplate,
                requestSettings: new OpenAIRequestSettings() { MaxTokens = maxTokens, Temperature = 0.1, TopP = 0.5 },
                functionName: nameof(GenericSummarizerSkill),
                pluginName: nameof(GenericSummarizerSkill),
                description: "Given a section of a conversation transcript, summarize the part of the conversation");
        }

        /// <summary>
        /// Gets the summary of a conversation transcript.
        /// </summary>
        /// <param name="input">A short or long conversation transcript.</param>
        /// <param name="context">The Semantic Kernel context.</param>
        /// <returns>The updated Semantic Kernel context.</returns>
        [SKFunction()]
        public async Task<string> SummarizeConversationAsync(
            [Description("A short or long conversation transcript.")] string input,
            SKContext context)
        {
            var result1 = await _kernel.RunAsync(input, _summarizeConversation);

            var result2 = await _summarizeConversation.InvokeAsync(input, _kernel);

            return result1.ToString() ?? result2.ToString();
        }
    }
}
