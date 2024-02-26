using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations
{
    /// <summary>
    /// Blob storage configuration settings.
    /// </summary>
    public class BlobStorageConfiguration
    {
        /// <summary>
        /// The type of configuration. This value should not be changed.
        /// </summary>
        [JsonPropertyName("configuration_type")]
        public string ConfigurationType => "blob_storage";

        /// <summary>
        /// The connection string key vault secret name that is retrieved from key vault.
        /// </summary>
        [JsonPropertyName("connection_string_secret")]
        public string? ConnectionStringSecretName { get; set; }

        /// <summary>
        /// The name of the container
        /// </summary>
        [JsonPropertyName("container")]
        public string? ContainerName { get; set; }

        /// <summary>
        /// The list of files to get
        /// </summary>
        [JsonPropertyName("files")]
        public List<string>? Files { get; set; }
    }
}
