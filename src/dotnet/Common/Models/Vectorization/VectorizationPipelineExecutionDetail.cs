using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// Represents the state of a vectorization pipeline execution.
    /// </summary>
    public class VectorizationPipelineExecutionDetail
    {
        /// <summary>
        /// Gets or sets the object identifier of the pipeline resource being executed.
        /// </summary>
        [JsonPropertyOrder(0)]
        [JsonPropertyName("pipeline_object_id")]
        public required string PipelineObjectId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the pipeline execution.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("execution_id")]
        public required string ExecutionId { get; set; }

        /// <summary>
        /// The vectorization requests associated with the pipeline execution and their status.
        /// Key: vectorization request resource object id
        /// Value: the processing state of the request
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName("vectorization_request_object_ids")]
        public List<string> VectorizationRequestObjectIds { get; set; }
            = [];
    }
}
