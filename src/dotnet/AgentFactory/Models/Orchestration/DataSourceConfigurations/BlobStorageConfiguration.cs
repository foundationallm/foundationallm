using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations
{
    /// <summary>
    /// Blob storage configuration settings.
    /// </summary>
    public class BlobStorageConfiguration
    {
        /// <summary>
        /// The type of configuration. This value should not be changed.
        /// </summary>
        [JsonProperty("configuration_type")]
        public string ConfigurationType = "blob_storage";

        /// <summary>
        /// The connection string key vault secret name that is retrieved from key vault.
        /// </summary>
        [JsonProperty("connection_string_secret")]
        public string? ConnectionStringSecretName { get; set; }

        /// <summary>
        /// The name of the container
        /// </summary>
        [JsonProperty("container")]
        public string? ContainerName { get; set; }

        /// <summary>
        /// The list of files to get
        /// </summary>
        [JsonProperty("files")]
        public List<string>? Files { get; set; }
    }
}
