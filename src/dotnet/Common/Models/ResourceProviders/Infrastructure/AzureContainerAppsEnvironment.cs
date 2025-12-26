using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Infrastructure
{
    /// <summary>
    /// Represents an Azure Container Apps Environment.
    /// </summary>
    public class AzureContainerAppsEnvironment : ResourceBase
    {
        /// <summary>
        /// The Azure resource identifier of the Container Apps Environment.
        /// </summary>
        [JsonPropertyName("azure_resource_id")]
        public string? AzureResourceId { get; set; }

        /// <summary>
        /// The Azure location/region of the Container Apps Environment.
        /// </summary>
        [JsonPropertyName("location")]
        public string? Location { get; set; }

        /// <summary>
        /// The provisioning state of the Container Apps Environment.
        /// </summary>
        [JsonPropertyName("provisioning_state")]
        public string? ProvisioningState { get; set; }

        /// <summary>
        /// The default domain of the Container Apps Environment.
        /// </summary>
        [JsonPropertyName("default_domain")]
        public string? DefaultDomain { get; set; }

        /// <summary>
        /// The static IP address of the Container Apps Environment.
        /// </summary>
        [JsonPropertyName("static_ip")]
        public string? StaticIp { get; set; }

        /// <summary>
        /// The workload profiles configured for the Container Apps Environment.
        /// </summary>
        [JsonPropertyName("workload_profiles")]
        public List<AzureContainerAppsWorkloadProfile>? WorkloadProfiles { get; set; }
    }

    /// <summary>
    /// Represents a workload profile in an Azure Container Apps Environment.
    /// </summary>
    public class AzureContainerAppsWorkloadProfile
    {
        /// <summary>
        /// The name of the workload profile.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The type of the workload profile.
        /// </summary>
        [JsonPropertyName("workload_profile_type")]
        public string? WorkloadProfileType { get; set; }

        /// <summary>
        /// The minimum number of instances for the workload profile.
        /// </summary>
        [JsonPropertyName("minimum_count")]
        public int? MinimumCount { get; set; }

        /// <summary>
        /// The maximum number of instances for the workload profile.
        /// </summary>
        [JsonPropertyName("maximum_count")]
        public int? MaximumCount { get; set; }
    }
}
