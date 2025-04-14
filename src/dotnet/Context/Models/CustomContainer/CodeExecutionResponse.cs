using System.Text.Json.Serialization;

namespace FoundationaLLM.Context.Models.CustomContainer
{
    /// <summary>
    /// Represents the response from executing code.
    /// </summary>
    public class CodeExecutionResponse
    {
        /// <summary>
        /// Gets or sets the output of the code execution.
        /// </summary>
        [JsonPropertyName("output")]
        public required string Output { get; set; }

        /// <summary>
        /// Gets or sets the results of the code execution.
        /// </summary>
        [JsonPropertyName("results")]
        public required object Results { get; set; }
    }
}
