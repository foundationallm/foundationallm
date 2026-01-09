using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Telemetry;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.OpenAI.Requests;
using FoundationaLLM.Common.Models.OpenAI.Responses;
using FoundationaLLM.Common.Telemetry;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace FoundationaLLM.Core.API.Controllers;

/// <summary>
/// Provides OpenAI-compatible API endpoints.
/// </summary>
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
[Authorize(
    AuthenticationSchemes = AgentAccessTokenDefaults.AuthenticationScheme,
    Policy = AuthorizationPolicyNames.FoundationaLLMAgentAccessToken)]
[ApiController]
[Route("openai")]
public class OpenAIController : ControllerBase
{
    private readonly IOpenAICompatibilityService _openAIService;
    private readonly IOrchestrationContext _callContext;
    private readonly ILogger<OpenAIController> _logger;

    /// <summary>
    /// Initializes a new instance of the OpenAIController.
    /// </summary>
    public OpenAIController(
        IOpenAICompatibilityService openAIService,
        IOrchestrationContext callContext,
        ILogger<OpenAIController> logger)
    {
        _openAIService = openAIService;
        _callContext = callContext;
        _logger = logger;
    }

    /// <summary>
    /// Creates a chat completion - OpenAI compatible endpoint.
    /// </summary>
    /// <param name="request">The OpenAI chat completion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("v1/chat/completions")]
    [Produces("application/json", "text/event-stream")]
    public async Task<IActionResult> CreateChatCompletion(
        [FromBody] OpenAIChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var instanceId = ExtractInstanceId();

        using var telemetryActivity = TelemetryActivitySources.CoreAPIActivitySource.StartActivity(
            "CoreAPI.OpenAI.ChatCompletions",
            ActivityKind.Server,
            parentContext: default,
            tags: new Dictionary<string, object?>
            {
                { TelemetryActivityTagNames.InstanceId, instanceId },
                { TelemetryActivityTagNames.UPN, _callContext.CurrentUserIdentity?.UPN ?? "N/A" },
                { TelemetryActivityTagNames.UserId, _callContext.CurrentUserIdentity?.UserId ?? "N/A" }
            });

        if (request.Stream)
        {
            return await HandleStreamingCompletion(instanceId, request, cancellationToken);
        }

        try
        {
            var response = await _openAIService.CreateChatCompletionAsync(instanceId, request, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat completion for instance {InstanceId}", instanceId);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Lists available models - OpenAI compatible endpoint.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("v1/models")]
    [Produces("application/json")]
    public async Task<IActionResult> ListModels(CancellationToken cancellationToken)
    {
        var instanceId = ExtractInstanceId();

        try
        {
            var models = await _openAIService.ListModelsAsync(instanceId, cancellationToken);
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing models for instance {InstanceId}", instanceId);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Retrieves a specific model - OpenAI compatible endpoint.
    /// </summary>
    /// <param name="id">The model identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("v1/models/{id}")]
    [Produces("application/json")]
    public async Task<IActionResult> GetModel(string id, CancellationToken cancellationToken)
    {
        var instanceId = ExtractInstanceId();

        try
        {
            var model = await _openAIService.GetModelAsync(instanceId, id, cancellationToken);
            if (model == null)
            {
                return NotFound(new { error = new OpenAIError
                {
                    Message = $"Model {id} not found",
                    Type = "not_found",
                    Code = "model_not_found"
                }});
            }

            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model {ModelId} for instance {InstanceId}", id, instanceId);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Handles streaming chat completion requests.
    /// </summary>
    private async Task<IActionResult> HandleStreamingCompletion(
        string instanceId,
        OpenAIChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        try
        {
            await foreach (var chunk in _openAIService.CreateChatCompletionStreamAsync(instanceId, request, cancellationToken))
            {
                var json = JsonSerializer.Serialize(chunk);
                await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            return new EmptyResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming completion for instance {InstanceId}", instanceId);
            // Send error as SSE
            var error = new { error = new OpenAIError
            {
                Message = ex.Message,
                Type = "internal_error",
                Code = "stream_error"
            }};
            var errorJson = JsonSerializer.Serialize(error);
            await Response.WriteAsync($"data: {errorJson}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            return new EmptyResult();
        }
    }

    /// <summary>
    /// Extracts the instance ID from the request context.
    /// For agent access tokens, this can be extracted from the token.
    /// For now, we'll use a default or extract from configuration.
    /// </summary>
    private string ExtractInstanceId()
    {
        // TODO: Extract instanceId from agent access token or use configuration
        // For MVP, we'll use a default value
        // In production, this should be extracted from the authenticated token
        return "default-instance";
    }

    /// <summary>
    /// Handles errors and returns OpenAI-compatible error responses.
    /// </summary>
    private IActionResult HandleError(Exception ex)
    {
        var error = ex switch
        {
            Common.Exceptions.ResourceProviderException rpe when rpe.StatusCode == 404 => new OpenAIError
            {
                Message = ex.Message,
                Type = "not_found",
                Code = "resource_not_found"
            },
            Common.Exceptions.ResourceProviderException rpe when rpe.StatusCode == 403 => new OpenAIError
            {
                Message = ex.Message,
                Type = "permission_denied",
                Code = "insufficient_permissions"
            },
            ArgumentException => new OpenAIError
            {
                Message = ex.Message,
                Type = "invalid_request_error",
                Code = "invalid_argument"
            },
            _ => new OpenAIError
            {
                Message = "An internal error occurred",
                Type = "internal_error",
                Code = "internal_error"
            }
        };

        var statusCode = ex switch
        {
            Common.Exceptions.ResourceProviderException rpe => rpe.StatusCode,
            ArgumentException => 400,
            _ => 500
        };

        return StatusCode(statusCode, new { error });
    }
}
