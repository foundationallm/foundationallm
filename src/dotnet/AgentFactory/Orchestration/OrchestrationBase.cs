using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Hubs;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Orchestration
{
    /// <summary>
    /// Base class for an orchestration involving a FoundationaLLM agent.
    /// </summary>
    /// <remarks>
    /// Constructor for the AgentBase class.
    /// </remarks>
    /// <param name="agentMetadata"></param>
    /// <param name="orchestrationService"></param>
    /// <param name="promptHubService"></param>
    /// <param name="dataSourceHubService"></param>
    public class OrchestrationBase(
        AgentMetadata? agentMetadata,
        ILLMOrchestrationService orchestrationService,
        IPromptHubAPIService promptHubService,
        IDataSourceHubAPIService dataSourceHubService)
    {
        /// <summary>
        /// The agent metadata.
        /// </summary>
        protected readonly AgentMetadata? _agentMetadata = agentMetadata;

        /// <summary>
        /// The orchestration service for the agent.
        /// </summary>
        protected readonly ILLMOrchestrationService _orchestrationService = orchestrationService;

        /// <summary>
        /// The prompt hub for the agent.
        /// </summary>
        protected readonly IPromptHubAPIService _promptHubService = promptHubService;

        /// <summary>
        /// The data source hub for the agent.
        /// </summary>
        protected readonly IDataSourceHubAPIService _dataSourceHubService = dataSourceHubService;

        /// <summary>
        /// This will setup the agent based on its metadata.
        /// </summary>
        /// <returns></returns>
        public virtual async Task Configure(CompletionRequest completionRequest) =>
            await Task.CompletedTask;

        /// <summary>
        /// The call to execute a completion after the agent is configured.
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        public virtual async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            await Task.CompletedTask;
            return null!;
        }
    }
}
