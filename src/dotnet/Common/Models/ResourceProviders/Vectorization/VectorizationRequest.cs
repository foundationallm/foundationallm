using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Vectorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Represents a vectorization request.
    /// </summary>
    public class VectorizationRequest
    {
        /// <summary>
        /// The unique identifier of the vectorization request.
        /// The responsibility to create this identifier belongs to the initiator of the vectorization request.
        /// </summary>
        [JsonPropertyOrder(-2)]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// The unique identifier of the vectorization request.
        /// The responsibility to create this identifier belongs to the FoundationaLLM.Vectorization resource provider.
        /// Subsequent vectorization requests referring to the same content will have different unique identifiers.
        /// </summary>
        [JsonPropertyOrder(0)]
        [JsonPropertyName("object_id")]
        public string? ObjectId { get; set; }

        /// <summary>
        /// Path to the vectorization request resource file.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("resource_filepath")]
        public string? ResourceFilePath { get; set; }

        /// <summary>
        /// The <see cref="ContentIdentifier"/> object identifying the content being vectorized.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName("content_identifier")]
        public required ContentIdentifier ContentIdentifier { get; set; }

        /// <summary>
        /// The <see cref="VectorizationProcessingType"/> indicating how should the request be processed.
        /// </summary>
        [JsonPropertyOrder(3)]
        [JsonPropertyName("processing_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required VectorizationProcessingType ProcessingType { get; set; }

        /// <summary>
        /// If run in the context of a pipeline, the object id of the pipeline resource being executed.
        /// </summary>
        [JsonPropertyOrder(4)]
        [JsonPropertyName("pipeline_object_id")]
        public string? PipelineObjectId { get; set; }

        /// <summary>
        /// If run in the context of a pipeline, the unique identifier of the pipeline execution.
        /// </summary>
        [JsonPropertyOrder(5)]
        [JsonPropertyName("pipeline_execution_id")]
        public string? PipelineExecutionId { get; set; }

        /// <summary>
        /// The <see cref="VectorizationProcessingState"/> indicating the current state of the vectorization request.
        /// </summary>
        [JsonPropertyOrder(6)]
        [JsonPropertyName("processing_state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VectorizationProcessingState ProcessingState { get; set; }

        /// <summary>
        /// The time when the vectorization request started being processed.
        /// </summary>
        [JsonPropertyOrder(7)]
        [JsonPropertyName("execution_start")]
        public DateTime? ExecutionStart { get; set; }

        /// <summary>
        /// The time when the vectorization request finished being processed.
        /// </summary>
        [JsonPropertyOrder(8)]
        [JsonPropertyName("execution_end")]
        public DateTime? ExecutionEnd { get; set; }

        /// <summary>
        /// Error messages that occurred during the processing of the vectorization request.
        /// </summary>
        [JsonPropertyOrder(9)]
        [JsonPropertyName("error_messages")]
        public List<string> ErrorMessages { get; set; } = [];

        /// <summary>
        /// The list of vectorization steps requested by the vectorization request.
        /// Vectorization steps are identified by unique names like "extract", "partition", "embed", "index", etc.
        /// </summary>
        [JsonPropertyOrder(10)]
        [JsonPropertyName("steps")]
        public required List<VectorizationStep> Steps { get; set; }

        /// <summary>
        /// The ordered list of the names of the vectorization steps that were already completed.
        /// </summary>
        [JsonPropertyOrder(11)]
        [JsonPropertyName("completed_steps")]
        public List<string> CompletedSteps { get; set; } = [];

        /// <summary>
        /// The ordered list of the names of the vectorization steps that still need to be executed.
        /// </summary>
        [JsonPropertyOrder(12)]
        [JsonPropertyName("remaining_steps")]
        public List<string> RemainingSteps { get; set; } = [];

        /// <summary>
        /// The current step of the vectorization request.
        /// </summary>
        [JsonPropertyOrder(13)]
        [JsonPropertyName("current_step")]
        public string? CurrentStep => RemainingSteps.Count == 0
            ? null
            : RemainingSteps.First();

        /// <summary>
        /// The number of times the processing of the current step resulted in an error.
        /// </summary>
        [JsonPropertyOrder(14)]
        [JsonPropertyName("error_count")]
        public int ErrorCount { get; set; }

        /// <summary>
        /// A dictionary of running operation identifiers indexed by step name.
        /// Some steps can be executed via long-running operations that required the persistence of operation identifiers.
        /// </summary>
        [JsonPropertyOrder(15)]
        [JsonPropertyName("running_operations")]
        public Dictionary<string, VectorizationLongRunningOperation> RunningOperations { get; set; } = [];

        /// <summary>
        /// The time of the last successful processing of a step.
        /// </summary>
        [JsonPropertyOrder(16)]
        [JsonPropertyName("last_successful_step_time")]
        public DateTime LastSuccessfulStepTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The cost center associated with the vectorization request.
        /// </summary>
        [JsonPropertyOrder(17)]
        [JsonPropertyName("cost_center")]
        public string? CostCenter { get; set; }

        /// <summary>
        /// Indicates whether the vectorization process is complete or not.
        /// </summary>
        [JsonIgnore]
        public bool Complete => RemainingSteps.Count == 0;

        /// <summary>
        /// Advances the vectorization pipeline to the next step.
        /// The newly set current step is used to choose the next request source to which the vectorization request will be added.
        /// </summary>
        /// <returns>A tuple containing the name of the previous step and the name of the next step to execute if there are steps left to execute or null if the processing
        /// of the vectorization request is complete.</returns>
        public (string PreviousStep, string? CurrentStep) MoveToNextStep()
        {
            if (RemainingSteps.Count == 0)
                throw new VectorizationException($"Attempting to move to the next step when no steps remain for vectorization request with id {Id}.");

            var previousStepName = RemainingSteps.First();
            RemainingSteps.RemoveAt(0);
            CompletedSteps.Add(previousStepName);

            var nextStepName = RemainingSteps.Count == 0
                ? null
                : RemainingSteps[0];

            LastSuccessfulStepTime = DateTime.UtcNow;

            return (previousStepName, nextStepName);
        }

        /// <summary>
        /// Gets the vectorization pipeline step that has a specific identifier.
        /// </summary>
        /// <param name="step">The identifier of the vectorization pipeline step.</param>
        /// <returns>An instances of the <see cref="VectorizationStep"/> class with the details required by the step handler.</returns>
        public VectorizationStep? this[string step] =>
            Steps.SingleOrDefault(s => s.Id == step);

        /// <summary>
        /// Identifies whether the request is expired or not.
        /// Vectorization requests that had no successful step executions in the last 10 days are considered expired.
        /// </summary>
        public bool Expired =>
            (DateTime.UtcNow - LastSuccessfulStepTime).TotalHours > 240;
    }
}
