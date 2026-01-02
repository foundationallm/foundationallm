using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Infrastructure
{
    /// <summary>
    /// Represents an Azure Kubernetes Service cluster.
    /// </summary>
    public class AzureKubernetesService : ResourceBase
    {
        /// <summary>
        /// The Azure resource identifier of the AKS cluster.
        /// </summary>
        [JsonPropertyName("azure_resource_id")]
        public string? AzureResourceId { get; set; }

        /// <summary>
        /// The Azure location/region of the AKS cluster.
        /// </summary>
        [JsonPropertyName("location")]
        public string? Location { get; set; }

        /// <summary>
        /// The provisioning state of the AKS cluster.
        /// </summary>
        [JsonPropertyName("provisioning_state")]
        public string? ProvisioningState { get; set; }

        /// <summary>
        /// The Kubernetes version of the AKS cluster.
        /// </summary>
        [JsonPropertyName("kubernetes_version")]
        public string? KubernetesVersion { get; set; }

        /// <summary>
        /// The DNS prefix of the AKS cluster.
        /// </summary>
        [JsonPropertyName("dns_prefix")]
        public string? DnsPrefix { get; set; }

        /// <summary>
        /// The FQDN of the AKS cluster.
        /// </summary>
        [JsonPropertyName("fqdn")]
        public string? Fqdn { get; set; }

        /// <summary>
        /// The node pools configured for the AKS cluster.
        /// </summary>
        [JsonPropertyName("node_pools")]
        public List<AzureKubernetesServiceNodePool>? NodePools { get; set; }

        /// <summary>
        /// The power state of the AKS cluster.
        /// </summary>
        [JsonPropertyName("power_state")]
        public string? PowerState { get; set; }
    }

    /// <summary>
    /// Represents a node pool in an Azure Kubernetes Service cluster.
    /// </summary>
    public class AzureKubernetesServiceNodePool
    {
        /// <summary>
        /// The name of the node pool.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The VM size of the nodes in the pool.
        /// </summary>
        [JsonPropertyName("vm_size")]
        public string? VmSize { get; set; }

        /// <summary>
        /// The number of nodes in the pool.
        /// </summary>
        [JsonPropertyName("node_count")]
        public int? NodeCount { get; set; }

        /// <summary>
        /// The minimum number of nodes for autoscaling.
        /// </summary>
        [JsonPropertyName("min_count")]
        public int? MinCount { get; set; }

        /// <summary>
        /// The maximum number of nodes for autoscaling.
        /// </summary>
        [JsonPropertyName("max_count")]
        public int? MaxCount { get; set; }

        /// <summary>
        /// Indicates whether autoscaling is enabled.
        /// </summary>
        [JsonPropertyName("enable_auto_scaling")]
        public bool EnableAutoScaling { get; set; }

        /// <summary>
        /// The mode of the node pool (System or User).
        /// </summary>
        [JsonPropertyName("mode")]
        public string? Mode { get; set; }
    }
}
