using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents the state of a quota metric partition.
    /// </summary>
    public class QuotaMetricPartitionState
    {
        /// <summary>
        /// Gets or sets the name of the quota that contains the partition.
        /// </summary>
        [JsonPropertyName("quota_name")]
        public required string QuotaName { get; set; }

        /// <summary>
        /// Gets or sets the quota context that contains the partition.
        /// </summary>
        [JsonPropertyName("quota_context")]
        public required string QuotaContext { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the quota metric partition used for evaluation.
        /// </summary>
        [JsonIgnore]
        public string QuotaMetricPartitionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the quota has been exceeded.
        /// </summary>
        [JsonPropertyName("quota_exceeded")]
        public bool QuotaExceeded { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds until the request can be retried.
        /// </summary>
        [JsonPropertyName("time_until_retry_seconds")]
        public int TimeUntilRetrySeconds { get; set; }

        /// <summary>
        /// Gets or sets the number of units of the quota metric that have a local origin
        /// (the service instance hosting the quota metric sequence).
        /// </summary>
        [JsonIgnore]
        public int LocalMetricCount { get; set; }

        /// <summary>
        /// Gets or sets the number of units of the quota metric that have a remote origin
        /// (other service instances than the one hosting the quota metric sequence).
        /// </summary>
        [JsonIgnore]
        public int RemoteMetricCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of units of the quota metric, both local and remote.
        /// </summary>
        [JsonIgnore]
        public int TotalMetricCount { get; set; }
    }
}
