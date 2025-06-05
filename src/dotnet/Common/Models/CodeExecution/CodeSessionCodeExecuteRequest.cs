using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides the request to execute code in a code session.
    /// </summary>
    public class CodeSessionCodeExecuteRequest
    {
        /// <summary>
        /// Gets or sets the code to execute.
        /// </summary>
        [JsonPropertyName("code_to_execute")]
        public required string CodeToExecute { get; set; }
    }
}
