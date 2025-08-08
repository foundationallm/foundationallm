using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents a change log for the state of a data pipeline state artifact.
    /// </summary>
    public class DataPipelineStateArtifactChangeLog
    {
        /// <summary>
        /// Represents a change log entry for a data pipeline state artifact.
        /// </summary>
        public class DataPipelineStateArtifactChangeLogEntry
        {
            /// <summary>
            /// Gets or sets the timestamp of the change.
            /// </summary>
            [JsonPropertyName("timestamp")]
            public DateTimeOffset Timestamp { get; set; }

            /// <summary>
            /// Gets or sets the data pipeline run identifier associated with the change.
            /// </summary>
            [JsonPropertyName("data_pipeline_run_id")]
            public required string DataPipelineRunId { get; set; }
        }

        /// <summary>
        /// Gets or sets the list of changes in the change log.
        /// </summary>
        [JsonPropertyName("changes")]
        public List<DataPipelineStateArtifactChangeLogEntry> Changes { get; set; } = [];

        /// <summary>
        /// Records a change in the data pipeline state artifact change log with the specified data pipeline run identifier.
        /// </summary>
        /// <param name="dataPipelineRunId">The identifier of the data pipeline run that changed the artifact.</param>
        public void AddChange(
            string dataPipelineRunId) =>
            Changes.Add(new DataPipelineStateArtifactChangeLogEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                DataPipelineRunId = dataPipelineRunId
            });

        /// <summary>
        /// Retrieves the identifier of the last data pipeline run that changed the artifact.
        /// </summary>
        [JsonIgnore]
        public string LastChangedBy =>
            Changes.Count > 0
                ? Changes.Last().DataPipelineRunId
                : string.Empty;
    }
}
