using System.Text.Json.Serialization;

namespace FoundationaLLM.CSharpCodeSession.API.Models
{
    /// <summary>
    /// Represents a request to execute code.
    /// </summary>
    public class CodeExecutionRequest
    {
        /// <summary>
        /// Gets or sets the code to be executed.
        /// </summary>
        [JsonPropertyName("code")]
        public required string Code { get; set; }
    }
}
