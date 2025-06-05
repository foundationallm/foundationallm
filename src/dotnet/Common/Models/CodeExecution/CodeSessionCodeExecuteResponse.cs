using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides the result of executing code in a code session.
    /// </summary>
    public class CodeSessionCodeExecuteResponse
    {
        /// <summary>
        /// Gets or sets the status of the code execution.
        /// </summary>
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        /// <summary>
        /// Gets or sets the result produced by the code that was executed.
        /// </summary>
        [JsonPropertyName("execution_result")]
        public required string ExecutionResult { get; set; }

        /// <summary>
        /// Gets or sets the standard output from the code execution.
        /// </summary>
        [JsonPropertyName("standard_output")]
        public required string StandardOutput { get; set; }

        /// <summary>
        /// Gets or sets the standard error from the code execution.
        /// </summary>
        [JsonPropertyName("error_output")]
        public required string StandardError { get; set; }
    }
}
