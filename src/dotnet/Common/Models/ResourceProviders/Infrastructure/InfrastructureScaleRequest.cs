using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Infrastructure
{
    /// <summary>
    /// Represents a request to scale an infrastructure resource.
    /// </summary>
    public class InfrastructureScaleRequest
    {
        /// <summary>
        /// The desired number of replicas.
        /// </summary>
        [JsonPropertyName("replicas")]
        public int? Replicas { get; set; }

        /// <summary>
        /// The minimum number of replicas for autoscaling.
        /// </summary>
        [JsonPropertyName("min_replicas")]
        public int? MinReplicas { get; set; }

        /// <summary>
        /// The maximum number of replicas for autoscaling.
        /// </summary>
        [JsonPropertyName("max_replicas")]
        public int? MaxReplicas { get; set; }
    }
}
