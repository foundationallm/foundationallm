using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provider the model for a data pipeline run.
    /// </summary>
    public class DataPipelineRun : AzureCosmosDBResource, IRunnableResource
    {
        /// <summary>
        /// The unique identifier of the data pipeline run.
        /// </summary>
        [JsonPropertyName("run_id")]
        [JsonPropertyOrder(1)]
        public string RunId => Id;

        /// <summary>
        /// Gets or sets the object identifier of the data pipeline.
        /// </summary>
        [JsonPropertyName("data_pipeline_object_id")]
        [JsonPropertyOrder(2)]
        public required string DataPipelineObjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the manual trigger used to start the pipeline.
        /// </summary>
        [JsonPropertyName("trigger_name")]
        [JsonPropertyOrder(3)]
        public required string TriggerName { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that contains the parameter values required to trigger the pipeline.
        /// </summary>
        [JsonPropertyName("trigger_parameter_values")]
        [JsonPropertyOrder(4)]
        public required Dictionary<string, object> TriggerParameterValues { get; set; } = [];

        /// <summary>
        /// Gets or sets the user principal name (UPN) of the user that triggered the creation of the data pipeline run.
        /// </summary>
        [JsonPropertyName("triggering_upn")]
        [JsonPropertyOrder(5)]
        public required string TriggeringUPN { get; set; }

        /// <summary>
        /// Gets or sets the name of the processor that is used to process the data pipeline run.
        /// </summary>
        [JsonPropertyName("processor")]
        [JsonPropertyOrder(6)]
        public required string Processor { get; set; }

        /// <summary>
        /// Gets or sets the list of active stages in the data pipeline run.
        /// </summary>
        [JsonPropertyName("active_stages")]
        [JsonPropertyOrder(7)]
        public List<string> ActiveStages { get; set; } = [];

        /// <inheritdoc/>
        [JsonPropertyName("completed")]
        [JsonPropertyOrder(8)]
        public bool Completed { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("successful")]
        [JsonPropertyOrder(9)]
        public bool Successful { get; set; }

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
                TriggeringUPN = string.Empty
            };
    }
}
