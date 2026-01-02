using FoundationaLLM.Common.Models.OpenAI.Responses;
using FoundationaLLM.Common.Models.Orchestration.Response;

namespace FoundationaLLM.Core.API.Services;

/// <summary>
/// Interface for translating FoundationaLLM responses to OpenAI responses.
/// </summary>
public interface IOpenAIResponseTranslator
{
    /// <summary>
    /// Translates a FoundationaLLM completion response to an OpenAI chat completion response.
    /// </summary>
    /// <param name="completionResponse">The FoundationaLLM response.</param>
    /// <param name="model">The model name to use in the response.</param>
    /// <returns>An OpenAI chat completion response.</returns>
    OpenAIChatCompletionResponse TranslateCompletionResponse(CompletionResponse completionResponse, string model);
}
