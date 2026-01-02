using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Infrastructure
{
    /// <summary>
    /// Represents a request to scale an infrastructure resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For Azure Container Apps: Use <see cref="MinReplicas"/> and <see cref="MaxReplicas"/> to configure
    /// autoscaling boundaries. The actual replica count is managed by the autoscaler within these bounds.
    /// </para>
    /// <para>
    /// For Kubernetes Deployments: Use <see cref="Replicas"/> to set a fixed number of replicas.
    /// If autoscaling (HPA) is desired, <see cref="MinReplicas"/> and <see cref="MaxReplicas"/> can be used.
    /// </para>
    /// </remarks>
    public class InfrastructureScaleRequest
    {
        /// <summary>
        /// The desired number of replicas for fixed scaling.
        /// </summary>
        /// <remarks>
        /// Use this property for Kubernetes deployments when you want a specific number of replicas.
        /// For autoscaling scenarios, use <see cref="MinReplicas"/> and <see cref="MaxReplicas"/> instead.
        /// </remarks>
        [JsonPropertyName("replicas")]
        public int? Replicas { get; set; }

        /// <summary>
        /// The minimum number of replicas for autoscaling.
        /// </summary>
        /// <remarks>
        /// Used with <see cref="MaxReplicas"/> to define autoscaling boundaries.
        /// For Azure Container Apps, this defines the minimum instances during scale-down.
        /// </remarks>
        [JsonPropertyName("min_replicas")]
        public int? MinReplicas { get; set; }

        /// <summary>
        /// The maximum number of replicas for autoscaling.
        /// </summary>
        /// <remarks>
        /// Used with <see cref="MinReplicas"/> to define autoscaling boundaries.
        /// For Azure Container Apps, this defines the maximum instances during scale-up.
        /// </remarks>
        [JsonPropertyName("max_replicas")]
        public int? MaxReplicas { get; set; }
    }
}
