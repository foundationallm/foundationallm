﻿using FoundationaLLM.AgentFactory.Core.Agents;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Core.Services;

/// <summary>
/// AgentFactoryService class.
/// </summary>
public class AgentFactoryService : IAgentFactoryService
{
    private readonly IEnumerable<ILLMOrchestrationService> _orchestrationServices;
    private readonly IAgentHubAPIService _agentHubAPIService;
    private readonly IPromptHubAPIService _promptHubAPIService;
    private readonly IDataSourceHubAPIService _dataSourceHubAPIService;

    private readonly ILogger<AgentFactoryService> _logger;

    /// <summary>
    /// Constructor for the Agent Factory Service.
    /// </summary>
    /// <param name="orchestrationServices"></param>
    /// <param name="agentHubService"></param>    
    /// <param name="promptHubService"></param>    
    /// <param name="dataSourceHubService"></param>    
    /// <param name="logger"></param>
    public AgentFactoryService(
        IEnumerable<ILLMOrchestrationService> orchestrationServices,
        IAgentHubAPIService agentHubService,
        IPromptHubAPIService promptHubService,
        IDataSourceHubAPIService dataSourceHubService,
        ILogger<AgentFactoryService> logger)
    {
        _orchestrationServices = orchestrationServices;
        _agentHubAPIService = agentHubService;
        _promptHubAPIService = promptHubService;
        _dataSourceHubAPIService = dataSourceHubService;

        _logger = logger;
    }

    /// <summary>
    /// Returns the status of the Semantic Kernel.
    /// </summary>
    public string Status
    {
        get
        {
            if (_orchestrationServices.All(os => os.IsInitialized))
                return "ready";

            return string.Join(",", _orchestrationServices
                .Where(os => !os.IsInitialized)
                .Select(os => $"{os.GetType().Name}: initializing"));
        }
    }


    /// <summary>
    /// Retrieve a completion from the configured orchestration service.
    /// </summary>
    public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
    {
        try
        {
            var agent = await AgentBuilder.Build(
                completionRequest.UserPrompt ?? string.Empty,
                completionRequest.SessionId ?? string.Empty,
                _agentHubAPIService,
                _orchestrationServices,
                _promptHubAPIService,
                _dataSourceHubAPIService);

            return await agent.GetCompletion(completionRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving completion from the orchestration service for {completionRequest.UserPrompt}.");
            return new CompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = completionRequest.UserPrompt ?? string.Empty,
                PromptTokens = 0,
                CompletionTokens = 0,
                UserPromptEmbedding = new float[] { 0 }
            };
        }
    }

    /// <summary>
    /// Retrieve a summarization for the passed in prompt from the orchestration service.
    /// </summary>
    public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
    {
        try
        {
            var agent = await AgentBuilder.Build(
                summaryRequest.UserPrompt ?? string.Empty,
                summaryRequest.SessionId ?? string.Empty,
                _agentHubAPIService,
                _orchestrationServices,
                _promptHubAPIService,
                _dataSourceHubAPIService);

            return await agent.GetSummary(summaryRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving summarization for {summaryRequest.UserPrompt}.");
            return new SummaryResponse
            {
                Summary = "[No Summary]"
            };
        }
    }
}
