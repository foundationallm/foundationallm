using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Represents the result of executing a vectorization action.
    /// </summary>
    /// <param name="ObjectId">The object id to which the result refers to.</param>
    /// <param name="IsSuccess">Indicates whether the execution was completed successfully.</param>
    /// <param name="ErrorMessage">When IsSuccess is false, contains an error message with details.</param>
    public record VectorizationResult(
        [property: JsonPropertyName("object_id")] string ObjectId,
        [property: JsonPropertyName("is_success")] bool IsSuccess,
        [property: JsonPropertyName("error_message")] string? ErrorMessage)
    {
    }
}
