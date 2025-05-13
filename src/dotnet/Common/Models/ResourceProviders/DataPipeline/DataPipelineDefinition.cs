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

        /// <summary>
        /// Gets the names of all the data pipeline stages.
        /// </summary>
        [JsonIgnore]
        public List<string> AllStageNames =>
            [.. StartingStages.SelectMany(stage => stage.AllStageNames)];

        /// <summary>
        /// Gets all the data pipeline stages.
        /// </summary>
        [JsonIgnore]
        public List<DataPipelineStage> AllStages =>
            [.. StartingStages.SelectMany(stage => stage.AllStages)];

        /// <summary>
        /// Gets the list of data pipeline stages that follow the specified stage.
        /// </summary>
        /// <param name="stageName">The name of the stage to search for.</param>
        /// <returns>The list of data pipeline stages that follow the specified stage.</returns>
        /// <exception cref="ArgumentException"></exception>
        public List<DataPipelineStage> GetNextStages(string stageName) =>
            AllStages.FirstOrDefault(stage => stage.Name == stageName)?.NextStages
            ?? throw new ArgumentException($"The stage {stageName} was not found in the pipeline definition.");
    }
}
