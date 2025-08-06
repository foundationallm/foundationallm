using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provider the model for a data pipeline run.
    /// </summary>
    public class DataPipelineRun : AzureCosmosDBResource, IRunnableResource
    {
        /// <summary>
        /// Gets or sets the unique identifier of the data pipeline run.
        /// </summary>
        [JsonPropertyName("run_id")]
        [JsonPropertyOrder(1)]
        public string RunId => Id;

        /// <summary>
        /// Gets or set the canonical run identifier.
        /// </summary>
        /// <remarks>
        /// When two separate data pipeline runs are triggered for the same data pipeline
        /// and the same parameters, they will have the same canonical run identifier.
        /// </remarks>
        [JsonPropertyName("canonical_run_id")]
        [JsonPropertyOrder(2)]
        public string? CanonicalRunId { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the data pipeline.
        /// </summary>
        [JsonPropertyName("data_pipeline_object_id")]
        [JsonPropertyOrder(3)]
        public required string DataPipelineObjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the manual trigger used to start the pipeline.
        /// </summary>
        [JsonPropertyName("trigger_name")]
        [JsonPropertyOrder(4)]
        public required string TriggerName { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that contains the parameter values required to trigger the pipeline.
        /// </summary>
        [JsonPropertyName("trigger_parameter_values")]
        [JsonPropertyOrder(5)]
        public required Dictionary<string, object> TriggerParameterValues { get; set; } = [];

        /// <summary>
        /// Gets or sets the user principal name (UPN) of the user that triggered the creation of the data pipeline run.
        /// </summary>
        [JsonPropertyName("triggering_upn")]
        [JsonPropertyOrder(6)]
        public required string TriggeringUPN { get; set; }

        /// <summary>
        /// Gets or sets the name of the processor that is used to process the data pipeline run.
        /// </summary>
        /// <remarks>
        /// Must be one of the values from <see cref="DataPipelineRunProcessors"/>.
        /// </remarks>
        [JsonPropertyName("processor")]
        [JsonPropertyOrder(7)]
        public required string Processor { get; set; }

        /// <summary>
        /// Gets or sets the list of active stages in the data pipeline run.
        /// </summary>
        [JsonPropertyName("active_stages")]
        [JsonPropertyOrder(8)]
        public List<string> ActiveStages { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of all stages in the data pipeline run.
        /// </summary>
        [JsonPropertyName("all_stages")]
        [JsonPropertyOrder(9)]
        public List<string> AllStages { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of completed stages in the data pipeline run.
        /// </summary>
        [JsonPropertyName("completed_stages")]
        [JsonPropertyOrder(10)]
        public List<string> CompletedStages { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of failed stages in the data pipeline run.
        /// </summary>
        [JsonPropertyName("failed_stages")]
        [JsonPropertyOrder(11)]
        public List<string> FailedStages { get; set; } = [];

        /// <inheritdoc/>
        [JsonPropertyName("completed")]
        [JsonPropertyOrder(12)]
        public bool Completed { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("successful")]
        [JsonPropertyOrder(13)]
        public bool Successful { get; set; }

        /// <summary>
        /// Gets or sets the metrics for each stage in the data pipeline run.
        /// </summary>
        [JsonPropertyName("stages_metrics")]
        [JsonPropertyOrder(14)]
        public Dictionary<string, DataPipelineStageMetrics> StagesMetrics { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of errors encountered during the data pipeline run.
        /// </summary>
        [JsonPropertyName("errors")]
        [JsonPropertyOrder(15)]
        public List<string>? Errors { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public DataPipelineRun() =>
            Type = DataPipelineTypes.DataPipelineRun;

        /// <summary>
        /// Creates a new <see cref="DataPipelineRun"/> instance.
        /// </summary>
        /// <param name="dataPipelineObjectId">The object identifier of the data pipeline.</param>
        /// <param name="triggerName">The name of the data pipeline trigger.</param>
        /// <param name="triggerParameterValues">The dictionary of data pipeline parameter values required by the trigger.</param>
        /// <param name="upn">The UPN that is associated with the data pipeline run.</param>
        /// <param name="processor">The name of the processor that is used to process the data pipeline run.
        /// Must be one of the values from <see cref="DataPipelineRunProcessors"/>.</param>
        /// <returns></returns>
        public static DataPipelineRun Create(
            string dataPipelineObjectId,
            string triggerName,
            Dictionary<string, object> triggerParameterValues,
            string upn,
            string processor) =>
            new()
            {
                DataPipelineObjectId = dataPipelineObjectId,
                TriggerName = triggerName,
                TriggerParameterValues = triggerParameterValues,
                UPN = upn,
                Processor = processor,

                ObjectId = ResourcePath.Join(dataPipelineObjectId, "dataPipelineRuns/new"),
                Name = string.Empty,
                Id = string.Empty,
                InstanceId = string.Empty,
                TriggeringUPN = string.Empty,
                CanonicalRunId = null!
            };

        /// <summary>
        /// Creates a new <see cref="DataPipelineRun"/> instance from a data pipeline trigger request.
        /// </summary>
        /// <param name="request">The data pipeline trigger request used to create the run.</param>
        /// <param name="ownerUserIdentity">The identity of the user that owns the newly created data pipeline run.</param>
        /// <returns>The newly created data pipeline run object.</returns>
        public static DataPipelineRun FromTriggerRequest(
            DataPipelineTriggerRequest request,
            UnifiedUserIdentity ownerUserIdentity) =>
            new()
            {
                DataPipelineObjectId = request.DataPipelineObjectId,
                TriggerName = request.TriggerName,
                TriggerParameterValues = request.TriggerParameterValues,
                UPN = ownerUserIdentity.UPN!,
                Processor = request.Processor,
                CanonicalRunId = request.DataPipelineCanonicalRunId,

                ObjectId = ResourcePath.Join(request.DataPipelineObjectId, "dataPipelineRuns/new"),
                Name = string.Empty,
                Id = string.Empty,
                InstanceId = string.Empty,
                TriggeringUPN = string.Empty
            };
    }
}
