using FoundationaLLM.Common.Constants.Orchestration;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// The point-in-time state of a long-running operation.
    /// </summary>
    public class LongRunningOperationLogEntry
    {
        /// <summary>
        /// The identifier of the long-running operation log entry.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The document type.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type => LongRunningOperationTypes.LongRunningOperationLogEntry;

        /// <summary>
        /// The identifier of the long-running operation.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public string OperationId { get; set; }

        /// <summary>
        /// The status of the long-running operation.
        /// </summary>
        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OperationStatus Status { get; set; }

        /// <summary>
        /// The time stamp of the log entry.
        /// </summary>
        [JsonPropertyName("time_stamp")]
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The message describing the current state of the operation.
        /// </summary>
        [JsonPropertyName("status_message")]
        public string? StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the User Principal Name (UPN) of the user who triggered the long-running operation.
        /// </summary>
        [JsonPropertyName("upn")]
        public string? UPN { get; set; }

        /// <summary>
        /// The time-to-live (TTL) for the log entry.
        /// </summary>
        [JsonPropertyName("ttl")]
        public int TTL { get; set; } = 604800;

        /// <summary>
        /// Initializes a new instance of the <see cref="LongRunningOperationLogEntry"/> class.
        /// </summary>
        public LongRunningOperationLogEntry()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LongRunningOperationLogEntry"/> class with the specified values.
        /// </summary>
        /// <param name="operationId">The identifier of the long-running operation.</param>
        /// <param name="status">The status of the long-running operation.</param>
        /// <param name="statusMessage">The message describing the current state of the operation.</param>
        /// <param name="upn">The user principal name (UPN) of the user who initiated the operation.</param>
        public LongRunningOperationLogEntry(string operationId, OperationStatus status, string? statusMessage, string? upn)
        {
            OperationId = operationId;
            Status = status;
            StatusMessage = statusMessage;
            UPN = upn;
        }
    }
}
