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
    }
}
