using FoundationaLLM.Common.Models.Chat;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration;

/// <summary>
/// The completion request object.
/// </summary>
public class CompletionRequest : OrchestrationRequest
{
    /// <summary>
    /// The message history associated with the completion request.
    /// </summary>
    [JsonPropertyName("message_history")]
    public List<MessageHistoryItem>? MessageHistory { get; init; } = [];
}
