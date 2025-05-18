using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Vectorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Represents a vectorization request.
    /// </summary>
    public class VectorizationRequest : ResourceBase
    {       
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
        [JsonPropertyOrder(50)]
        [JsonPropertyName("pipeline_object_id")]
        public string? PipelineObjectId { get; set; }

        /// <summary>
        /// If run in the context of a pipeline, the name of the pipeline resource being executed.
        /// </summary>
        [JsonPropertyOrder(51)]
        [JsonPropertyName("pipeline_name")]
        public string? PipelineName { get; set; }

        /// <summary>
        /// If run in the context of a pipeline, the unique identifier of the pipeline execution.
        /// </summary>
        [JsonPropertyOrder(52)]
        [JsonPropertyName("pipeline_execution_id")]
        public string? PipelineExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the time when the pipeline execution started.
        /// </summary>
        [JsonPropertyOrder(53)]
        [JsonPropertyName("pipeline_execution_start")]
        public DateTimeOffset? PipelineExecutionStart { get; set; }

        /// <summary>
        /// The <see cref="VectorizationProcessingState"/> indicating the current state of the vectorization request.
        /// </summary>
        [JsonPropertyOrder(100)]
        [JsonPropertyName("processing_state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VectorizationProcessingState ProcessingState { get; set; } = VectorizationProcessingState.New;

        /// <summary>
        /// The time when the vectorization request started being processed.
        /// </summary>
        [JsonPropertyOrder(101)]
        [JsonPropertyName("execution_start")]
        public DateTime? ExecutionStart { get; set; }

        /// <summary>
        /// The time when the vectorization request finished being processed.
        /// </summary>
        [JsonPropertyOrder(102)]
        [JsonPropertyName("execution_end")]
        public DateTime? ExecutionEnd { get; set; }

        /// <summary>
        /// Error messages that occurred during the processing of the vectorization request.
        /// </summary>
        [JsonPropertyOrder(103)]
        [JsonPropertyName("error_messages")]
        public List<string> ErrorMessages { get; set; } = [];

        /// <summary>
        /// The list of vectorization steps requested by the vectorization request.
        /// Vectorization steps are identified by unique names like "extract", "partition", "embed", "index", etc.
        /// </summary>
        [JsonPropertyOrder(200)]
        [JsonPropertyName("steps")]
        public required List<VectorizationStep> Steps { get; set; }

        /// <summary>
        /// The ordered list of the names of the vectorization steps that were already completed.
        /// </summary>
        [JsonPropertyOrder(201)]
        [JsonPropertyName("completed_steps")]
        public List<string> CompletedSteps { get; set; } = [];

        /// <summary>
        /// The ordered list of the names of the vectorization steps that still need to be executed.
        /// </summary>
        [JsonPropertyOrder(202)]
        [JsonPropertyName("remaining_steps")]
        public List<string> RemainingSteps { get; set; } = [];

        /// <summary>
        /// The current step of the vectorization request.
        /// </summary>
        [JsonPropertyOrder(203)]
        [JsonPropertyName("current_step")]
        public string? CurrentStep => RemainingSteps.Count == 0
            ? null
            : RemainingSteps.First();

        /// <summary>
        /// The number of times the processing of the current step resulted in an error.
        /// </summary>
        [JsonPropertyOrder(204)]
        [JsonPropertyName("error_count")]
        public int ErrorCount { get; set; }

        /// <summary>
        /// A dictionary of running operation identifiers indexed by step name.
        /// Some steps can be executed via long-running operations that required the persistence of operation identifiers.
        /// </summary>
        [JsonPropertyOrder(205)]
        [JsonPropertyName("running_operations")]
        public Dictionary<string, VectorizationLongRunningOperation> RunningOperations { get; set; } = [];

        /// <summary>
        /// The time of the last successful processing of a step.
        /// </summary>
        [JsonPropertyOrder(206)]
        [JsonPropertyName("last_successful_step_time")]
        public DateTime LastSuccessfulStepTime { get; set; } = DateTime.UtcNow;

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
                throw new VectorizationException($"Attempting to move to the next step when no steps remain for vectorization request with id {Name}.");

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

        /// <summary>
        /// Gets the date from the name of the vectorization request.
        /// </summary>
        /// <returns>The date parsed from the name.</returns>
        public DateOnly GetDateFromName() => GetDate(Name);

        /// <summary>
        /// Set default property values.
        /// </summary>
        public VectorizationRequest() =>
            Type = "vectorization-request";

        /// <summary>
        /// Gets the date from the name of the vectorization request.
        /// </summary>
        /// <param name="vectorizationRequestName">The name of the vectorization request.</param>
        /// <remarks>
        /// The required format of the vectorization request name is "yyyyMMdd-..." (e.g., "20250502-abc").
        /// </remarks>
        /// <returns>The date parsed from the name.</returns>
        public static DateOnly GetDate(string vectorizationRequestName) =>
            DateOnly.FromDateTime(DateTime.ParseExact(vectorizationRequestName.Split('-')[0], "yyyyMMdd", null));
    }
}
