using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Orchestration.Core.Orchestration
{
    /// <summary>
    /// Knowledge Management orchestration.
    /// </summary>
    /// <remarks>
    /// Constructor for default agent.
    /// </remarks>
    /// <param name="agent">The <see cref="KnowledgeManagementAgent"/> agent.</param>
    /// <param name="callContext">The call context of the request being handled.</param>
    /// <param name="orchestrationService"></param>
    /// <param name="logger">The logger used for logging.</param>
    public class KnowledgeManagementOrchestration(
        KnowledgeManagementAgent agent,
        ICallContext callContext,
        ILLMOrchestrationService orchestrationService,
        ILogger<OrchestrationBase> logger) : OrchestrationBase(orchestrationService)
    {
        private readonly ICallContext _callContext = callContext;
        private readonly ILogger<OrchestrationBase> _logger = logger;
        private readonly KnowledgeManagementAgent _agent = agent;

        /// <inheritdoc/>
        public override Task Configure(CompletionRequest completionRequest) =>
            base.Configure(completionRequest);

        /// <inheritdoc/>
        public override async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            var result = await _orchestrationService.GetCompletion(
                new LLMCompletionRequest
                {
                    UserPrompt = completionRequest.UserPrompt!,
                    Agent = _agent,
                    MessageHistory = completionRequest.MessageHistory,
                    Settings = completionRequest.Settings
                });

            if (result.Citations != null)
            {
                result.Citations = result.Citations
                    .GroupBy(c => c.Filepath)
                    .Select(g => g.First())
                    .ToArray();
            }

            return new CompletionResponse
            {
                Completion = result.Completion!,
                UserPrompt = completionRequest.UserPrompt!,
                Citations = result.Citations,
                FullPrompt = result.FullPrompt,
                PromptTemplate = result.PromptTemplate,
                AgentName = result.AgentName,
                PromptTokens = result.PromptTokens,
                CompletionTokens = result.CompletionTokens,
            };
        }
    }
}
