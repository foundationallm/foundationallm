using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.AgentFactory.Core.Orchestration
{
    /// <summary>
    /// Knowledge Management orchestration.
    /// </summary>
    /// <remarks>
    /// Constructor for default agent.
    /// </remarks>
    /// <param name="agent">The <see cref="InternalContextAgent"/> agent.</param>
    /// <param name="callContext">The call context of the request being handled.</param>
    /// <param name="orchestrationService"></param>
    /// <param name="promptHubService"></param>
    /// <param name="dataSourceHubService"></param>
    /// <param name="logger">The logger used for logging.</param>
    public class InternalContextOrchestration(
        InternalContextAgent agent,
        ICallContext callContext,
        ILLMOrchestrationService orchestrationService,
        IPromptHubAPIService promptHubService,
        IDataSourceHubAPIService dataSourceHubService,
        ILogger<OrchestrationBase> logger) : OrchestrationBase(null, orchestrationService, promptHubService, dataSourceHubService)
    {
        private readonly ICallContext _callContext = callContext;
        private readonly ILogger<OrchestrationBase> _logger = logger;
        private readonly InternalContextAgent _agent = agent;

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

            return new CompletionResponse
            {
                Completion = result.Completion!,
                UserPrompt = completionRequest.UserPrompt!,
                FullPrompt = result.FullPrompt,
                PromptTemplate = result.PromptTemplate,
                AgentName = result.AgentName,
                PromptTokens = result.PromptTokens,
                CompletionTokens = result.CompletionTokens,
            };
        }
    }
}
