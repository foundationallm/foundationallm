using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents a work item in a data pipeline run.
    /// </summary>
    public class DataPipelineRunWorkItem
    {
        /// <summary>
        /// Gets or sets the unique identifier of the work item.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonPropertyOrder(1)]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos DB item type.
        /// </summary>
        /// <remarks>
        /// Must always be set to <see cref="DataPipelineTypes.DataPipelineRunWorkItem"/>.
        /// </remarks>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(2)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the data pipeline run that is processing the content item.
        /// </summary>
        [JsonPropertyName("run_id")]
        [JsonPropertyOrder(3)]
        public required string RunId { get; set; }

        /// <summary>
        /// Gets or sets the name of the stage in the data pipeline run that is processing the work item.
        /// </summary>
        [JsonPropertyName("stage")]
        [JsonPropertyOrder(4)]
        public required string Stage { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the data pipeline run input artifact that is referenced by the work item.
        /// </summary>
        [JsonPropertyName("input_artifact_id")]
        [JsonPropertyOrder(5)]
        public required string InputArtifactId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the data pipeline run output artifact that is referenced by the work item.
        /// </summary>
        [JsonPropertyName("output_artifact_id")]
        [JsonPropertyOrder(6)]
        public string? OutputArtifactId { get; set; }

        /// <summary>
        /// Gets or sets the completion status of the work item.
        /// </summary>
        [JsonPropertyName("completed")]
        [JsonPropertyOrder(7)]
        public bool Completed { get; set; }

        /// <summary>
        /// Gets or sets the success status of the work item.
        /// </summary>
        [JsonPropertyName("successful")]
        [JsonPropertyOrder(8)]
        public bool Successful { get; set; }

        /// <summary>
        /// Gets or sets the error message if the work item failed.
        /// </summary>
        [JsonPropertyName("error")]
        [JsonPropertyOrder(9)]
        public string? Error { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPipelineRunWorkItem"/> class.
        /// </summary>
        public DataPipelineRunWorkItem() =>
            Type = DataPipelineTypes.DataPipelineRunWorkItem;
    }
}
