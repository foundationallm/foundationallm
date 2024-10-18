using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// The response from the Azure OpenAI orchestration service.
    /// </summary>
    public class AzureOpenAICompletionResponse
    {
        /// <summary>
        /// The completion response ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The completion response object.
        /// </summary>
        [JsonPropertyName("object")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Object { get; set; }

        /// <summary>
        /// The completion response created timestamp.
        /// </summary>
        [JsonPropertyName("created")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Created { get; set; }

        /// <summary>
        /// The completion model used for the response.
        /// </summary>
        [JsonPropertyName("model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Model { get; set; }

        /// <summary>
        /// The completion response choices.
        /// </summary>
        [JsonPropertyName("choices")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AzureOpenAICompletionResponseChoice[]? Choices { get; set; }

        /// <summary>
        /// Completion usage statistics.
        /// </summary>
        [JsonPropertyName("usage")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CompletionUsage? Usage { get; set; }
    }

}
