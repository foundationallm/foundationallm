using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Holds the state associated with each step of the vectorization pipeline
    /// for a specified content item (i.e., document).
    /// </summary>
    public class VectorizationState
    {
        /// <summary>
        /// The unique identifier of the content (i.e., document) being vectorized.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("content_id")]
        public required string ContentId { get; set; }

        /// <summary>
        /// The unique identifier of the current vectorization request. Subsequent vectorization requests
        /// referring to the same content will have different unique identifiers.
        /// </summary>
        [JsonPropertyOrder(0)]
        [JsonPropertyName("request_id")]
        public required string CurrentRequestId { get; set; }

        /// <summary>
        /// The list of log entries associated with actions executed by the vectorization pipeline.
        /// </summary>
        [JsonPropertyOrder(18)]
        [JsonPropertyName("log")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<VectorizationLogEntry> LogEntries { get; set; } = new List<VectorizationLogEntry>();

        /// <summary>
        /// Adds a new generic log entry.
        /// </summary>
        /// <param name="handler">The vectorization step handler executing the action.</param>
        /// <param name="requestId">The identifier of the vectorization request.</param>
        /// <param name="text">The string content of the log entry.</param>
        public void Log(IVectorizationStepHandler handler, string requestId, string text) =>
            LogEntries.Add(new VectorizationLogEntry(
                requestId, handler.StepId, text));

        /// <summary>
        /// Adds a log entry marking the start of handling.
        /// </summary>
        /// <param name="handler">The vectorization step handler executing the action.</param>
        /// <param name="requestId">The identifier of the vectorization request.</param>
        public void LogHandlerStart(IVectorizationStepHandler handler, string requestId) =>
            LogEntries.Add(new VectorizationLogEntry(
                requestId, handler.StepId, "Started handling step."));

        /// <summary>
        /// Adds a log entry marking the completion of handling.
        /// </summary>
        /// <param name="handler">The vectorization step handler executing the action.</param>
        /// <param name="requestId">The identifier of the vectorization request.</param>
        public void LogHandlerEnd(IVectorizationStepHandler handler, string requestId) =>
            LogEntries.Add(new VectorizationLogEntry(
                requestId, handler.StepId, "Finished handling step."));

        /// <summary>
        /// Adds a log entry for a handling exception.
        /// </summary>
        /// <param name="handler">The vectorization step handler executing the action.</param>
        /// <param name="requestId">The identifier of the vectorization request.</param>
        /// <param name="ex">The exception being logged.</param>
        public void LogHandlerError(IVectorizationStepHandler handler, string requestId, Exception ex) =>
            LogEntries.Add(new VectorizationLogEntry(
                requestId, handler.StepId, $"ERROR: {ex.Message}"));
    }
}
