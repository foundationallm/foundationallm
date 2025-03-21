using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a response from the FoundationaLLM Context API.
    /// </summary>
    /// <typeparam name="T">The type of the expected result.</typeparam>
    public class ContextServiceResponse<T> where T : class
    {
        /// <summary>
        /// Gets or sets the result of the operation.
        /// </summary>
        [JsonPropertyName("result")]
        public T? Result { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        [JsonPropertyName("success")]
        public required bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if the operation was not successful.
        /// </summary>
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

    }
}
