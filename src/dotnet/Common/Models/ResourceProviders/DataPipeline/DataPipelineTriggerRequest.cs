using FoundationaLLM.Common.Constants.DataPipelines;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Represents a request to trigger a data pipeline run.
    /// </summary>
    public class DataPipelineTriggerRequest
    {
        /// <summary>
        /// Gets or sets the object identifier of the data pipeline.
        /// </summary>
        [JsonPropertyName("data_pipeline_object_id")]
        [JsonPropertyOrder(1)]
        public required string DataPipelineObjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the manual trigger used to start the pipeline.
        /// </summary>
        [JsonPropertyName("trigger_name")]
        [JsonPropertyOrder(2)]
        public required string TriggerName { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that contains the parameter values required to trigger the pipeline.
        /// </summary>
        [JsonPropertyName("trigger_parameter_values")]
        [JsonPropertyOrder(3)]
        public required Dictionary<string, object> TriggerParameterValues { get; set; } = [];

        /// <summary>
        /// Gets or sets the name of the processor that is used to process the data pipeline run.
        /// </summary>
        /// <remarks>
        /// Must be one of the values from <see cref="DataPipelineRunProcessors"/>.
        /// </remarks>
        [JsonPropertyName("processor")]
        [JsonPropertyOrder(4)]
        public required string Processor { get; set; }

        /// <summary>
        /// Gets or sets the canonical run identifier to be used for the data pipeline run.
        /// </summary>
        /// <remarks>
        /// Providing this value allows the caller to force using a specific canonical run identifier,
        /// which allows the new run to execute using an already existing canonical data from previous pipeline runs.
        /// If not provided, the canonical run identifier will be generated based on the data pipeline name and trigger parameters.
        /// When provided, it should only be a value that matches and already existing canonical run identifier. In general,
        /// it is recommended to leave this value empty and let the system generate it automatically, unless there is a specific
        /// need to reuse an existing canonical run identifier (e.g., for testing or troubleshooting purposes).
        /// </remarks>
        [JsonPropertyName("data_pipeline_canonical_run_id")]
        [JsonPropertyOrder(5)]
        public string? DataPipelineCanonicalRunId { get; set; }
    }
}
