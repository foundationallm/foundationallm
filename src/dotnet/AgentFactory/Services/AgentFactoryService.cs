using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.AgentFactory.Core.Services;

/// <summary>
/// AgentFactoryService class.
/// </summary>
public class AgentFactoryService : IAgentFactoryService
{
    private readonly IEnumerable<ILLMOrchestrationService> _orchestrationServices;
    private readonly ICacheService _cacheService;
    private readonly ICallContext _callContext;
    private readonly IConfiguration _configuration;
    private readonly IAgentHubAPIService _agentHubAPIService;
    private readonly IPromptHubAPIService _promptHubAPIService;
    private readonly IDataSourceHubAPIService _dataSourceHubAPIService;

    private readonly ILogger<AgentFactoryService> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices;

    /// <summary>
    /// Constructor for the Agent Factory Service.
    /// </summary>
    /// <param name="resourceProviderServices">A list of <see cref="IResourceProviderService"/> resource providers.</param>
    /// <param name="orchestrationServices"></param>
    /// <param name="callContext">The call context of the request being handled.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
    /// <param name="agentHubService"></param>    
    /// <param name="promptHubService"></param>    
    /// <param name="dataSourceHubService"></param>    
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public AgentFactoryService(
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IEnumerable<ILLMOrchestrationService> orchestrationServices,
        ICallContext callContext,
        IConfiguration configuration,
        IAgentHubAPIService agentHubService,
        IPromptHubAPIService promptHubService,
        IDataSourceHubAPIService dataSourceHubService,
        ILoggerFactory loggerFactory)
    {
        _resourceProviderServices = resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);

        _orchestrationServices = orchestrationServices;
        _callContext = callContext;
        _configuration = configuration;
        _agentHubAPIService = agentHubService;
        _promptHubAPIService = promptHubService;
        _dataSourceHubAPIService = dataSourceHubService;

        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<AgentFactoryService>();
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
            var orchestration = await OrchestrationBuilder.Build(
                completionRequest,
                _callContext,
                _configuration,
                _resourceProviderServices,
                _agentHubAPIService,
                _orchestrationServices,
                _promptHubAPIService,
                _dataSourceHubAPIService,
                _loggerFactory);

            return orchestration == null
                ? throw new OrchestrationException($"The orchestration builder was not able to create an orchestration for agent [{completionRequest.AgentName ?? string.Empty }].")
                : await orchestration.GetCompletion(completionRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving completion from the orchestration service for {UserPrompt}.",
                completionRequest.UserPrompt);
            return new CompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = completionRequest.UserPrompt ?? string.Empty,
                PromptTokens = 0,
                CompletionTokens = 0,
                UserPromptEmbedding = [0f]
            };
        }
    }
}
