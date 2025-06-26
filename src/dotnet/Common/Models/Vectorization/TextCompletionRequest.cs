using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// Models a request to run a list of completions.
    /// </summary>
    public class TextCompletionRequest
    {
        /// <summary>
        /// The list of <see cref="TextChunk"/> objects containing the completion inputs.
        /// </summary>
        [JsonPropertyName("text_chunks")]
        public IList<TextChunk> TextChunks { get; set; } = [];

        /// <summary>
        /// The name of the completion model to use.
        /// If not specified, a default embedding model should be used.
        /// </summary>
        [JsonPropertyName("completion_model_name")]
        public string CompletionModelName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parameters for the completion model.
        /// </summary>
        [JsonPropertyName("completion_model_parameters")]
        public Dictionary<string, object> CompletionModelParameters { get; set; } = [];
    }
}
