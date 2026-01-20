using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipelineEngine.Models
{
    /// <summary>
    /// Represents information about a scheduled pipeline, including the next scheduled run time.
    /// </summary>
    public class ScheduledPipelineInfo
    {
        /// <summary>
        /// Gets or sets the data pipeline definition.
        /// </summary>
        public required DataPipelineDefinition Pipeline { get; set; }

        /// <summary>
        /// Gets or sets the trigger definition.
        /// </summary>
        public required DataPipelineTrigger Trigger { get; set; }

        /// <summary>
        /// Gets or sets the next scheduled run time in UTC.
        /// </summary>
        public DateTimeOffset? NextRunTime { get; set; }

        /// <summary>
        /// Gets or sets the last execution time in UTC.
        /// </summary>
        /// <remarks>
        /// Used to prevent duplicate executions within the same minute.
        /// </remarks>
        public DateTimeOffset? LastExecutionTime { get; set; }

        /// <summary>
        /// Gets the cache key for this scheduled pipeline.
        /// </summary>
        public string CacheKey => $"{Pipeline.Name}|{Trigger.Name}";
    }
}
