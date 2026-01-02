using FoundationaLLM.Common.Models.OpenAI.Requests;
using FoundationaLLM.Common.Models.Orchestration.Request;

namespace FoundationaLLM.Core.API.Services;

/// <summary>
/// Interface for translating OpenAI requests to FoundationaLLM requests.
/// </summary>
public interface IOpenAIRequestTranslator
{
    /// <summary>
    /// Translates an OpenAI chat completion request to a FoundationaLLM completion request.
    /// </summary>
    /// <param name="openAIRequest">The OpenAI request.</param>
    /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
    /// <returns>A FoundationaLLM completion request.</returns>
    CompletionRequest TranslateChatCompletionRequest(OpenAIChatCompletionRequest openAIRequest, string instanceId);
}
