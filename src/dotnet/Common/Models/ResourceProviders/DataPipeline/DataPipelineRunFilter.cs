using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Represents a filter for querying <see cref="DataPipelineRun"/> resources in the FoundationaLLM.DataPipeline resource provider.
    /// </summary>
    public class DataPipelineRunFilter
    {
        /// <summary>
        /// Gets or sets the identifier of the FoundationaLLM instance to filter runs by.
        /// </summary>
        [JsonPropertyName("instance_id")]
        public string? InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the data pipeline to filter runs by.
        /// </summary>
        [JsonPropertyName("data_pipeline_name")]
        public string? DataPipelineName { get; set; }

        /// <summary>
        /// Gets or sets the data pipeline name filter to filter runs by.
        /// </summary>
        [JsonPropertyName("data_pipeline_name_filter")]
        public string? DataPipelineNameFilter { get; set; }

        /// <summary>
        /// Gets or sets the flag that indicates whether only completed runs should be returned.
        /// </summary>
        [JsonPropertyName("completed")]
        public bool? Completed { get; set; }

        /// <summary>
        /// Gets or sets the flag that indicates whether only successful runs should be returned.
        /// </summary>
        [JsonPropertyName("successful")]
        public bool? Successful { get; set; }

        /// <summary>
        /// Gets or sets the start time of the data pipeline run.
        /// </summary>
        [JsonPropertyName("start_time")]
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the data pipeline run.
        /// </summary>
        [JsonPropertyName("end_time")]
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the continuation token for pagination.
        /// </summary>
        [JsonPropertyName("continuation_token")]
        public string? ContinuationToken { get; set; }

        /// <summary>
        /// Gets or sets the page size for pagination.
        /// </summary>
        [JsonPropertyName("page_size")]
        public int? PageSize { get; set; }
    }
}
