using FoundationaLLM.Common.Constants.Quota;
using FoundationaLLM.Common.Models.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents a quota definition.
    /// </summary>
    public class QuotaDefinition : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; } = "quota-definition";

        /// <summary>
        /// Gets or sets the context of the quota.
        /// </summary>
        /// <remarks>
        /// The context defines where the quota is applied, e.g., "CoreAPI:Completions" or "CoreAPI:Completions:AgentName".
        /// </remarks>
        [JsonPropertyName("context")]
        public required string Context { get; set; }

        /// <summary>
        /// Gets or sets the type of the quota.
        /// </summary>
        [JsonPropertyName("quota_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QuotaType QuotaType { get; set; }

        /// <summary>
        /// Gets or sets the type of partitioning applied to the metric that is used to enforce the quota.
        /// </summary>
        [JsonPropertyName("metric_partition")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QuotaMetricPartitionType MetricPartition { get; set; }

        /// <summary>
        /// Gets or sets the limit of the metric that is used to enforce the quota.
        /// </summary>
        [JsonPropertyName("metric_limit")]
        public int MetricLimit { get; set; }

        /// <summary>
        /// Gets or sets the window of time in seconds over which the metric is evaluated.
        /// </summary>
        [JsonPropertyName("metric_window_seconds")]
        public int MetricWindowSeconds { get; set; }

        /// <summary>
        /// Gets or sets the duration in seconds for which the quota is locked out after it has been exceeded.
        /// </summary>
        [JsonPropertyName("lockout_duration_seconds")]
        public int LockoutDurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the quota is enforced in a distributed manner.
        /// </summary>
        [JsonPropertyName("distributed_enforcement")]
        public bool DistributedEnforcement { get; set; }
    }
}
