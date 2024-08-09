using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration;

/// <summary>
/// Response from a language model.
/// </summary>
public class CompletionResponse : CompletionResponseBase
{

    /// <summary>
    /// User prompt embedding.
    /// </summary>
    [JsonPropertyName("user_prompt_embedding")]
    public float[]? UserPromptEmbedding { get; set; }

    /// <summary>
    /// Initialize a completion response
    /// </summary>
    /// <param name="operationId">The operation id of the completion request.</param>
    /// <param name="completion">The completion response from the language model.</param>
    /// <param name="userPrompt">The user prompt the language model responded to.</param>
    /// <param name="userPromptTokens">The number of tokens in the prompt.</param>
    /// <param name="responseTokens">The number of tokens in the completion.</param>
    /// <param name="userPromptEmbedding">User prompt embedding.</param>
    /// <param name="fileObjectIdentifiers">List of file object identifiers.</param>
    /// <param name="coreAPIDeepLink">Deep link of the object.</param>
    public CompletionResponse(string operationId, string completion, string userPrompt, int userPromptTokens, int responseTokens,
        float[]? userPromptEmbedding)
    {
        OperationId = operationId;
        Completion = completion;
        UserPrompt = userPrompt;
        PromptTokens = userPromptTokens;
        CompletionTokens = responseTokens;
        UserPromptEmbedding = userPromptEmbedding;
    }

    /// <summary>
    /// Initialize a completion response.
    /// </summary>
    public CompletionResponse()
    {
        Completion = string.Empty;
        UserPrompt = string.Empty;
    }

    /// <summary>
    /// Returns a CompletionResponse object from a JSON document.
    /// </summary>
    /// <param name="json">The JSON document to deserialize.</param>
    /// <returns></returns>
    public static CompletionResponse? FromJSONDocument(JsonDocument json) =>
        JsonSerializer.Deserialize<CompletionResponse>(json.RootElement.GetRawText());
}
