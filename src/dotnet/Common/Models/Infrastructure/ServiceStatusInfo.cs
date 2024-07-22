using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Infrastructure
{
    /// <summary>
    /// Represents the status of a service.
    /// </summary>
    public class ServiceStatusInfo
    {
        /// <summary>
        /// The name of the service.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// The instance ID of the service.
        /// </summary>
        [JsonPropertyName("instance_id")]
        public string? InstanceId { get; set; }

        /// <summary>
        /// The instance of the service.
        /// </summary>
        [JsonPropertyName("instance_name")]
        public string? InstanceName { get; set; }

        /// <summary>
        /// The deployed FoundationaLLM version of the service.
        /// </summary>
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        /// <summary>
        /// The status of the service.
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// The message associated with the status.
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// The status of all the subordinate services.
        /// </summary>
        [JsonPropertyName("subordinate_services")]
        public List<ServiceStatusInfo>? SubordinateServices { get; set; }
    }
}
