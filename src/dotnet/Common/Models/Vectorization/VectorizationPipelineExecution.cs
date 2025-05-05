using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// Provides the details of a vectorization pipeline execution.
    /// </summary>
    public class VectorizationPipelineExecution : ResourceBase
    {
        /// <summary>
        /// Gets or sets the object identifier of the pipeline resource being executed.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("pipeline_object_id")]
        public required string PipelineObjectId { get; set; }

        /// <summary>
        /// Gets or sets the UTC time when the pipeline execution was started.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName("execution_start")]
        public DateTimeOffset? ExecutionStart { get; set; }

        /// <summary>
        /// Gets or sets the UTC time when the pipeline execution was completed.
        /// </summary>
        [JsonPropertyOrder(3)]
        [JsonPropertyName("execution_end")]
        public DateTimeOffset? ExecutionEnd { get; set; }

        /// <summary>
        /// Gets or sets the number of vectorization requests that were submitted to the pipeline.
        /// </summary>
        [JsonPropertyOrder(4)]
        [JsonPropertyName("vectorization_request_count")]
        public int VectorizationRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the number of vectorization requests that failed during processing.
        /// </summary>
        [JsonPropertyOrder(5)]
        [JsonPropertyName("vectorization_request_failures_count")]
        public int VectorizationRequestFailuresCount { get; set; }

        /// <summary>
        /// Gets or sets the number of vectorization requests that failed during processing.
        /// </summary>
        [JsonPropertyOrder(6)]
        [JsonPropertyName("vectorization_request_successes_count")]
        public int VectorizationRequestSuccessesCount { get; set; }

        /// <summary>
        /// Gets or sets the processing state of the pipeline execution.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>New -> empty vectorization requests collection</item>
        /// <item>InProgress -> at least one vectorization request in progress</item>
        /// <item>Failed -> at least one vectorization request failed</item>
        /// <item>Completed -> all vectorization requests completed successfully</item>
        /// </list>
        /// </remarks>
        [JsonPropertyOrder(7)]
        [JsonPropertyName("processing_state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VectorizationProcessingState ProcessingState =>
            VectorizationRequestSuccessesCount + VectorizationRequestFailuresCount < VectorizationRequestCount
                ? VectorizationProcessingState.InProgress
                : VectorizationRequestFailuresCount > 0
                    ? VectorizationProcessingState.Failed
                    : VectorizationProcessingState.Completed;


        /// <summary>
        /// Gets or sets a list of error messages that includes content that was rejected at creation time along with the error.
        /// </summary>
        [JsonPropertyOrder(8)]
        [JsonPropertyName("error_messages")]
        public List<string> ErrorMessages { get; set; } = [];

    }
}
