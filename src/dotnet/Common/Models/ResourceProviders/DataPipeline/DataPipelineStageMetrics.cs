using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Represents the metrics for a data pipeline stage.
    /// </summary>
    public class DataPipelineStageMetrics
    {
        /// <summary>
        /// Gets or sets the number of work items that must be processed in the stage.
        /// </summary>
        [JsonPropertyName("work_items_count")]
        public int WorkItemsCount { get; set; }

        /// <summary>
        /// Gets or sets the number of work items that have been processed in the stage.
        /// </summary>
        [JsonPropertyName("completed_work_items_count")]
        public int CompletedWorkItemsCount { get; set; }

        /// <summary>
        /// Gets or sets the number of work items that have been successfully processed in the stage.
        /// </summary>
        [JsonPropertyName("successful_work_items_count")]
        public int SuccessfulWorkItemsCount { get; set; }
    }
}
