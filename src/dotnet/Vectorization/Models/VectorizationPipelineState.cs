using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents the state of a run of a vectorization pipeline.
    /// </summary>
    public class VectorizationPipelineState
    {
        /// <summary>
        /// The unique identifier of the pipeline execution.
        /// </summary>
        [JsonPropertyOrder(-2)]
        [JsonPropertyName("id")]
        public required string ExecutionId { get; set; }

        /// <summary>
        /// The object id of the pipeline resource being executed.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("pipeline_object_id")]
        public required string PipelineObjectId { get; set; }

        /// <summary>
        /// Time in UTC the pipeline run was started.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName("execution_start")]
        public DateTime? ExecutionStart { get; set; }

        /// <summary>
        /// Time in UTC the pipeline run was completed.
        /// </summary>
        [JsonPropertyOrder(3)]
        [JsonPropertyName("execution_end")]
        public DateTime? ExecutionEnd { get; set; }

        /// <summary>
        /// The vectorization requests associated with the pipeline execution and their status.
        /// Key: vectorization request resource object id
        /// Value: the processing state of the request
        /// </summary>
        [JsonPropertyOrder(4)]
        [JsonPropertyName("vectorization_request_object_ids")]
        public List<string> VectorizationRequestObjectIds { get; set; }
            = [];

        /// <summary>
        /// The processing state of the pipeline execution.
        /// New -> empty vectorization requests collection
        /// InProgress -> at least one vectorization request in progress
        /// Failed -> at least one vectorization request failed
        /// Completed -> all vectorization requests completed successfully
        /// </summary>
        [JsonPropertyOrder(5)]
        [JsonPropertyName("processing_state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VectorizationProcessingState ProcessingState { get; set; } = VectorizationProcessingState.New;
        

        /// <summary>
        /// A list of error messages that includes content that was rejected at creation time along with the error.
        /// </summary>
        [JsonPropertyOrder(6)]
        [JsonPropertyName("error_messages")]
        public List<string> ErrorMessages { get; set; } = [];

        /// <summary>
        /// The cost center associated with the pipeline execution.
        /// </summary>
        [JsonPropertyOrder(7)]
        [JsonPropertyName("cost_center")]
        public string? CostCenter { get; set; }

    }
}
