using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Infrastructure
{
    /// <summary>
    /// Represents an Azure Container App.
    /// </summary>
    public class AzureContainerApp : ResourceBase
    {
        /// <summary>
        /// The Azure resource identifier of the Container App.
        /// </summary>
        [JsonPropertyName("azure_resource_id")]
        public string? AzureResourceId { get; set; }

        /// <summary>
        /// The object identifier of the parent Container Apps Environment.
        /// </summary>
        [JsonPropertyName("environment_object_id")]
        public string? EnvironmentObjectId { get; set; }

        /// <summary>
        /// The Azure location/region of the Container App.
        /// </summary>
        [JsonPropertyName("location")]
        public string? Location { get; set; }

        /// <summary>
        /// The provisioning state of the Container App.
        /// </summary>
        [JsonPropertyName("provisioning_state")]
        public string? ProvisioningState { get; set; }

        /// <summary>
        /// The running status of the Container App.
        /// </summary>
        [JsonPropertyName("running_status")]
        public string? RunningStatus { get; set; }

        /// <summary>
        /// The container image used by the Container App.
        /// </summary>
        [JsonPropertyName("container_image")]
        public string? ContainerImage { get; set; }

        /// <summary>
        /// The minimum number of replicas for the Container App.
        /// </summary>
        [JsonPropertyName("min_replicas")]
        public int? MinReplicas { get; set; }

        /// <summary>
        /// The maximum number of replicas for the Container App.
        /// </summary>
        [JsonPropertyName("max_replicas")]
        public int? MaxReplicas { get; set; }

        /// <summary>
        /// The current number of running replicas for the Container App.
        /// </summary>
        [JsonPropertyName("current_replicas")]
        public int? CurrentReplicas { get; set; }

        /// <summary>
        /// The CPU resource limit in cores.
        /// </summary>
        [JsonPropertyName("cpu_cores")]
        public double? CpuCores { get; set; }

        /// <summary>
        /// The memory resource limit in gigabytes.
        /// </summary>
        [JsonPropertyName("memory_gb")]
        public double? MemoryGb { get; set; }

        /// <summary>
        /// The workload profile name used by the Container App.
        /// </summary>
        [JsonPropertyName("workload_profile_name")]
        public string? WorkloadProfileName { get; set; }

        /// <summary>
        /// The ingress configuration for the Container App.
        /// </summary>
        [JsonPropertyName("ingress")]
        public AzureContainerAppIngress? Ingress { get; set; }

        /// <summary>
        /// The environment variables configured for the Container App.
        /// </summary>
        [JsonPropertyName("environment_variables")]
        public Dictionary<string, string>? EnvironmentVariables { get; set; }

        /// <summary>
        /// The secrets configured for the Container App.
        /// </summary>
        [JsonPropertyName("secrets")]
        public List<string>? Secrets { get; set; }

        /// <summary>
        /// The latest revision name of the Container App.
        /// </summary>
        [JsonPropertyName("latest_revision_name")]
        public string? LatestRevisionName { get; set; }

        /// <summary>
        /// The FQDN of the Container App.
        /// </summary>
        [JsonPropertyName("fqdn")]
        public string? Fqdn { get; set; }
    }

    /// <summary>
    /// Represents the ingress configuration for an Azure Container App.
    /// </summary>
    public class AzureContainerAppIngress
    {
        /// <summary>
        /// Indicates whether ingress is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// The target port for ingress traffic.
        /// </summary>
        [JsonPropertyName("target_port")]
        public int? TargetPort { get; set; }

        /// <summary>
        /// Indicates whether external ingress is enabled.
        /// </summary>
        [JsonPropertyName("external")]
        public bool External { get; set; }

        /// <summary>
        /// The transport protocol for ingress.
        /// </summary>
        [JsonPropertyName("transport")]
        public string? Transport { get; set; }
    }
}
