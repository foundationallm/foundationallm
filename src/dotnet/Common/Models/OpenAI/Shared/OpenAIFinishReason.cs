namespace FoundationaLLM.Common.Models.OpenAI.Shared;

/// <summary>
/// Represents the finish reason for an OpenAI chat completion.
/// </summary>
public static class OpenAIFinishReason
{
    /// <summary>
    /// The model stopped generating text naturally.
    /// </summary>
    public const string Stop = "stop";

    /// <summary>
    /// The model hit the maximum token limit.
    /// </summary>
    public const string Length = "length";

    /// <summary>
    /// The model called one or more tools.
    /// </summary>
    public const string ToolCalls = "tool_calls";

    /// <summary>
    /// Content was filtered by content safety filters.
    /// </summary>
    public const string ContentFilter = "content_filter";
}
