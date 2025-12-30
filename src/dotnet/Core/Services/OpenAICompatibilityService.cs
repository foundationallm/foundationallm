using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.OpenAI.Requests;
using FoundationaLLM.Common.Models.OpenAI.Responses;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Core.API.Services;
using FoundationaLLM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Core.Services;

/// <summary>
/// Service for OpenAI compatibility operations.
/// </summary>
public class OpenAICompatibilityService : IOpenAICompatibilityService
{
    private readonly ICoreService _coreService;
    private readonly IOpenAIRequestTranslator _requestTranslator;
    private readonly IOpenAIResponseTranslator _responseTranslator;
    private readonly IResourceProviderService _agentResourceProvider;
    private readonly ILogger<OpenAICompatibilityService> _logger;

    /// <summary>
    /// Initializes a new instance of the OpenAICompatibilityService.
    /// </summary>
    public OpenAICompatibilityService(
        ICoreService coreService,
        IOpenAIRequestTranslator requestTranslator,
        IOpenAIResponseTranslator responseTranslator,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILogger<OpenAICompatibilityService> logger)
    {
        _coreService = coreService;
        _requestTranslator = requestTranslator;
        _responseTranslator = responseTranslator;
        _logger = logger;

        var resourceProviderServicesDictionary = resourceProviderServices.ToDictionary<IResourceProviderService, string>(
            rps => rps.Name);
        if (!resourceProviderServicesDictionary.TryGetValue(
            Common.Constants.ResourceProviders.ResourceProviderNames.FoundationaLLM_Agent,
            out var agentResourceProvider))
        {
            throw new Common.Exceptions.ResourceProviderException(
                $"The resource provider {Common.Constants.ResourceProviders.ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");
        }
        _agentResourceProvider = agentResourceProvider;
    }

    /// <summary>
    /// Creates a chat completion from an OpenAI-style request.
    /// </summary>
    public async Task<OpenAIChatCompletionResponse> CreateChatCompletionAsync(
        string instanceId,
        OpenAIChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Translate OpenAI request to FoundationaLLM request
            var completionRequest = _requestTranslator.TranslateChatCompletionRequest(request, instanceId);

            // Call the core service
            var message = await _coreService.GetCompletionAsync(instanceId, completionRequest);

            // Translate FoundationaLLM response to OpenAI response
            // We need to get the completion response from the message
            // For now, we'll create a basic response - this may need enhancement based on actual Message structure
            var completionResponse = new CompletionResponse
            {
                OperationId = completionRequest.OperationId ?? Guid.NewGuid().ToString().ToLower(),
                Completion = message.Text ?? string.Empty,
                UserPrompt = completionRequest.UserPrompt,
                PromptTokens = 0, // These would need to be extracted from the message if available
                CompletionTokens = 0,
                AgentName = completionRequest.AgentName
            };

            return _responseTranslator.TranslateCompletionResponse(completionResponse, request.Model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat completion for instance {InstanceId}", instanceId);
            throw;
        }
    }

    /// <summary>
    /// Creates a streaming chat completion from an OpenAI-style request.
    /// </summary>
    public async IAsyncEnumerable<OpenAIChatCompletionChunk> CreateChatCompletionStreamAsync(
        string instanceId,
        OpenAIChatCompletionRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // For MVP, we'll use async completion operations and poll for status
        // This is a simplified implementation - a full implementation would use proper streaming
        var completionRequest = _requestTranslator.TranslateChatCompletionRequest(request, instanceId);
        var operation = await _coreService.StartCompletionOperation(instanceId, completionRequest);

        var operationId = operation.OperationId;
        var chunkId = $"chatcmpl-{operationId}";
        var created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Poll for completion status
        while (!cancellationToken.IsCancellationRequested)
        {
            var status = await _coreService.GetCompletionOperationStatus(instanceId, operationId);
            
            if (status.Status == Common.Models.Orchestration.LongRunningOperationStatus.Completed)
            {
                // Final chunk with completion
                if (status.Result is CompletionResponse completionResponse)
                {
                    var finalResponse = _responseTranslator.TranslateCompletionResponse(completionResponse, request.Model);
                    yield return new OpenAIChatCompletionChunk
                    {
                        Id = chunkId,
                        Created = created,
                        Model = request.Model,
                        Choices = finalResponse.Choices.Select(c => new OpenAIChatCompletionChoice
                        {
                            Index = c.Index,
                            Delta = new Requests.OpenAIMessage
                            {
                                Role = Common.Models.OpenAI.Shared.OpenAIMessageRole.Assistant,
                                Content = string.Empty
                            },
                            FinishReason = c.FinishReason
                        }).ToList()
                    };
                }
                break;
            }

            // For MVP, we'll yield a simple chunk
            // A full implementation would parse partial results from the status
            await Task.Delay(100, cancellationToken); // Poll interval
        }
    }

    /// <summary>
    /// Lists available models (agents) as OpenAI models.
    /// </summary>
    public async Task<OpenAIModelList> ListModelsAsync(
        string instanceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var agents = await _coreService.GetAgentsAsync(instanceId);
            var models = agents.Select(agent => new OpenAIModel
            {
                Id = agent.Resource?.Name ?? string.Empty,
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                OwnedBy = "foundationallm"
            }).ToList();

            return new OpenAIModelList
            {
                Data = models
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing models for instance {InstanceId}", instanceId);
            throw;
        }
    }

    /// <summary>
    /// Gets a specific model (agent) as an OpenAI model.
    /// </summary>
    public async Task<OpenAIModel?> GetModelAsync(
        string instanceId,
        string modelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var agents = await _coreService.GetAgentsAsync(instanceId);
            var agent = agents.FirstOrDefault(a => 
                a.Resource?.Name?.Equals(modelId, StringComparison.OrdinalIgnoreCase) == true);

            if (agent?.Resource == null)
            {
                return null;
            }

            return new OpenAIModel
            {
                Id = agent.Resource.Name,
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                OwnedBy = "foundationallm"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model {ModelId} for instance {InstanceId}", modelId, instanceId);
            throw;
        }
    }
}
