using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides the result of creating a code execution session.
    /// </summary>
    public class CreateCodeSessionResponse
    {
        /// <summary>
        /// The unique identifier for the code execution session.
        /// </summary>
        [JsonPropertyName("session_id")]
        public required string SessionId { get; set; }

        /// <summary>
        /// The endpoint used to execute the code.
        /// </summary>
        [JsonPropertyName("endpoint")]
        public required string Endpoint { get; set; }
    }
}
