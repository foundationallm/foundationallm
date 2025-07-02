using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a response from the FoundationaLLM Context API.
    /// </summary>
    /// <typeparam name="T">The type of the expected result.</typeparam>
    public class ContextServiceResponse<T> : ContextServiceResponse
        where T : class
    {
        /// <summary>
        /// Gets or sets the result of the operation.
        /// </summary>
        [JsonPropertyName("result")]
        public T? Result { get; set; }
    }
}
