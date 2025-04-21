using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provides the model for a data pipeline.
    /// </summary>
    public class DataPipelineDefinition : ResourceBase
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the pipeline is active or not.
        /// </summary>
        /// <remarks>
        /// When the pipeline is inactive, it cannot be triggered to execute.
        /// </remarks>
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataPipelineDataSource"/> model that represents the data source for the pipeline.
        /// </summary>
        [JsonPropertyName("data_source")]
        public required DataPipelineDataSource DataSource { get; set; }

        /// <summary>
        /// Gets or sets the list of starting stages in the data pipeline.
        /// </summary>
        [JsonPropertyName("starting_stages")]
        public List<DataPipelineStage> StartingStages { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of triggers that can be used to start the pipeline.
        /// </summary>
        [JsonPropertyName("triggers")]
        public List<DataPipelineTrigger> Triggers { get; set; } = [];

        /// <summary>
        /// Gets or sets the object identifier of the most recent snapshot of the data pipeline.
        /// </summary>
        [JsonPropertyName("most_recent_snapshot_object_id")]
        public string MostRecentSnapshotObjectId { get; set; } = null!;
    }
}
