using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Provides details about an LLM operation that is in progress.
    /// </summary>
    public class RunningOperation
    {
        /// <summary>
        /// The unique identifier for the operation.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public required string OperationId { get; set; } = string.Empty;
    }
}
