using AngleSharp.Text;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Models;
using FoundationaLLM.Orchestration.Core.Orchestration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Orchestration.Core.Services;

/// <summary>
/// OrchestrationService class.
/// </summary>
public class OrchestrationService : IOrchestrationService
{
    private readonly IEnumerable<ILLMOrchestrationService> _orchestrationServices;
    private readonly ICallContext _callContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrchestrationService> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices;

    /// <summary>
    /// Constructor for the Orchestration Service.
    /// </summary>
    /// <param name="resourceProviderServices">A list of <see cref="IResourceProviderService"/> resource providers.</param>
    /// <param name="orchestrationServices"></param>
    /// <param name="callContext">The call context of the request being handled.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public OrchestrationService(
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IEnumerable<ILLMOrchestrationService> orchestrationServices,
        ICallContext callContext,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _resourceProviderServices = resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);

        _orchestrationServices = orchestrationServices;
        _callContext = callContext;
        _configuration = configuration;

        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<OrchestrationService>();
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
            var conversationSteps = await GetAgentConversationSteps(completionRequest.AgentName!, completionRequest.UserPrompt);
            return await GetCompletionForAgentConversation(completionRequest, conversationSteps);
            
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

    private async Task<CompletionResponse> GetCompletionForAgentConversation(
        CompletionRequest completionRequest,
        List<AgentConversationStep> agentConversationSteps)
    {
        var currentCompletionResponse = default(CompletionResponse);

        foreach (var conversationStep in agentConversationSteps)
        {
            var orchestration = await OrchestrationBuilder.Build(
                conversationStep.AgentName,
                _callContext,
                _configuration,
                _resourceProviderServices,
                _orchestrationServices,
                _loggerFactory);

            var stepCompletionRequest = new CompletionRequest
            {
                AgentName = conversationStep.AgentName,
                SessionId = completionRequest.SessionId,
                Settings = completionRequest.Settings,
                MessageHistory = completionRequest.MessageHistory,
                UserPrompt = currentCompletionResponse == null
                    ? conversationStep.UserPrompt
                    : $"{currentCompletionResponse.Completion}{Environment.NewLine}{conversationStep.UserPrompt}"
            };

            currentCompletionResponse = orchestration == null
                ? throw new OrchestrationException($"The orchestration builder was not able to create an orchestration for agent [{completionRequest.AgentName ?? string.Empty}].")
                : await orchestration.GetCompletion(stepCompletionRequest);

            var newConversationSteps = await GetAgentConversationSteps(
                currentCompletionResponse.AgentName!,
                currentCompletionResponse.Completion);
            if (newConversationSteps.Count > 0
                && newConversationSteps.First().AgentName != currentCompletionResponse.AgentName)
                currentCompletionResponse =
                    await GetCompletionForAgentConversation(completionRequest, newConversationSteps);
        }

        return currentCompletionResponse!;
    }

    private async Task<List<AgentConversationStep>> GetAgentConversationSteps(string agentName, string userPrompt)
    {
        var currentPrompt = new StringBuilder();
        var result = new List<AgentConversationStep>();
        var currentAgentName = agentName;
        
        using (StringReader sr = new StringReader(userPrompt))
        {
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith('@'))
                {
                    var tokens = line.Split([' ', ',']);
                    var candidateAgentName = tokens.First().Replace("@", string.Empty);
                    var isValid = await ValidAgentName(candidateAgentName);

                    if (isValid)
                    {
                        var newUserPrompt = currentPrompt.ToString().Trim();
                        if (!string.IsNullOrEmpty(newUserPrompt))
                            result.Add(new AgentConversationStep
                            {
                                AgentName = currentAgentName,
                                UserPrompt = newUserPrompt
                            });
                        currentAgentName = candidateAgentName;

                        currentPrompt = new StringBuilder();
                        var remainingLine = line.Substring(candidateAgentName.Length + 2);
                        if (!string.IsNullOrWhiteSpace(remainingLine))
                            currentPrompt.AppendLine(remainingLine);
                    }
                }
                else
                    currentPrompt.AppendLine(line);
            }

            var lastUserPrompt = currentPrompt.ToString().Trim();
            if (!string.IsNullOrEmpty(lastUserPrompt))
                result.Add(new AgentConversationStep
                {
                    AgentName = currentAgentName,
                    UserPrompt = lastUserPrompt
                });
        }

        return result;
    }

    private async Task<bool> ValidAgentName(string agentName)
    {
        var agentResourceProvider = _resourceProviderServices[ResourceProviderNames.FoundationaLLM_Agent];

        var result = await agentResourceProvider.HandlePostAsync(
            $"/{AgentResourceTypeNames.Agents}/{AgentResourceProviderActions.CheckName}",
            JsonSerializer.Serialize(new ResourceName { Name = agentName }),
            _callContext.CurrentUserIdentity!);

        return true;
    }
}
