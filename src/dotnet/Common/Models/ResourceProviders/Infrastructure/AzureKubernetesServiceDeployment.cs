using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Infrastructure
{
    /// <summary>
    /// Represents a Kubernetes deployment in an Azure Kubernetes Service cluster.
    /// </summary>
    public class AzureKubernetesServiceDeployment : ResourceBase
    {
        /// <summary>
        /// The object identifier of the parent AKS cluster.
        /// </summary>
        [JsonPropertyName("cluster_object_id")]
        public string? ClusterObjectId { get; set; }

        /// <summary>
        /// The Kubernetes namespace of the deployment.
        /// </summary>
        [JsonPropertyName("namespace")]
        public string? Namespace { get; set; }

        /// <summary>
        /// The container image used by the deployment.
        /// </summary>
        [JsonPropertyName("container_image")]
        public string? ContainerImage { get; set; }

        /// <summary>
        /// The desired number of replicas for the deployment.
        /// </summary>
        [JsonPropertyName("replicas")]
        public int? Replicas { get; set; }

        /// <summary>
        /// The current number of ready replicas for the deployment.
        /// </summary>
        [JsonPropertyName("ready_replicas")]
        public int? ReadyReplicas { get; set; }

        /// <summary>
        /// The current number of available replicas for the deployment.
        /// </summary>
        [JsonPropertyName("available_replicas")]
        public int? AvailableReplicas { get; set; }

        /// <summary>
        /// The CPU resource request.
        /// </summary>
        [JsonPropertyName("cpu_request")]
        public string? CpuRequest { get; set; }

        /// <summary>
        /// The CPU resource limit.
        /// </summary>
        [JsonPropertyName("cpu_limit")]
        public string? CpuLimit { get; set; }

        /// <summary>
        /// The memory resource request.
        /// </summary>
        [JsonPropertyName("memory_request")]
        public string? MemoryRequest { get; set; }

        /// <summary>
        /// The memory resource limit.
        /// </summary>
        [JsonPropertyName("memory_limit")]
        public string? MemoryLimit { get; set; }

        /// <summary>
        /// The environment variables configured for the deployment.
        /// </summary>
        [JsonPropertyName("environment_variables")]
        public Dictionary<string, string>? EnvironmentVariables { get; set; }

        /// <summary>
        /// The labels applied to the deployment.
        /// </summary>
        [JsonPropertyName("labels")]
        public Dictionary<string, string>? Labels { get; set; }

        /// <summary>
        /// The deployment status conditions.
        /// </summary>
        [JsonPropertyName("conditions")]
        public List<KubernetesDeploymentCondition>? Conditions { get; set; }

        /// <summary>
        /// The deployment strategy type.
        /// </summary>
        [JsonPropertyName("strategy")]
        public string? Strategy { get; set; }
    }

    /// <summary>
    /// Represents a condition of a Kubernetes deployment.
    /// </summary>
    public class KubernetesDeploymentCondition
    {
        /// <summary>
        /// The type of the condition.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// The status of the condition (True, False, Unknown).
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// The reason for the condition's last transition.
        /// </summary>
        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// A message describing the condition.
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// The last time the condition transitioned.
        /// </summary>
        [JsonPropertyName("last_transition_time")]
        public DateTimeOffset? LastTransitionTime { get; set; }
    }
}
