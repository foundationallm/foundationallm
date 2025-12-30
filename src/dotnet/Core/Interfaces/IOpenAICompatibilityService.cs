using FoundationaLLM.Common.Models.OpenAI.Requests;
using FoundationaLLM.Common.Models.OpenAI.Responses;

namespace FoundationaLLM.Core.Interfaces;

/// <summary>
/// Service interface for OpenAI compatibility operations.
/// </summary>
public interface IOpenAICompatibilityService
{
    /// <summary>
    /// Creates a chat completion from an OpenAI-style request.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
    /// <param name="request">The OpenAI chat completion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An OpenAI-style chat completion response.</returns>
    Task<OpenAIChatCompletionResponse> CreateChatCompletionAsync(
        string instanceId,
        OpenAIChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a streaming chat completion from an OpenAI-style request.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
    /// <param name="request">The OpenAI chat completion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of OpenAI-style chat completion chunks.</returns>
    IAsyncEnumerable<OpenAIChatCompletionChunk> CreateChatCompletionStreamAsync(
        string instanceId,
        OpenAIChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available models (agents) as OpenAI models.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of OpenAI-style model objects.</returns>
    Task<OpenAIModelList> ListModelsAsync(
        string instanceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific model (agent) as an OpenAI model.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
    /// <param name="modelId">The model identifier (agent name).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An OpenAI-style model object, or null if not found.</returns>
    Task<OpenAIModel?> GetModelAsync(
        string instanceId,
        string modelId,
        CancellationToken cancellationToken = default);
}
