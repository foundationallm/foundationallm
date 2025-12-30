namespace FoundationaLLM.Common.Models.OpenAI.Shared;

/// <summary>
/// Represents the role of a message in an OpenAI chat completion.
/// </summary>
public static class OpenAIMessageRole
{
    /// <summary>
    /// System message role.
    /// </summary>
    public const string System = "system";

    /// <summary>
    /// User message role.
    /// </summary>
    public const string User = "user";

    /// <summary>
    /// Assistant message role.
    /// </summary>
    public const string Assistant = "assistant";

    /// <summary>
    /// Tool message role.
    /// </summary>
    public const string Tool = "tool";
}
