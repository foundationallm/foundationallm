using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a response from the FoundationaLLM Context API.
    /// </summary>
    public class ContextServiceResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        [JsonPropertyName("is_success")]
        public required bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the error message if the operation was not successful.
        /// </summary>
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }
    }
}
