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
    public class StockConfiguration
    {
        [JsonProperty("open_ai_endpoint")]
        public string? OpenAIEndpoint { get; set; }

        [JsonProperty("open_ai_key")]
        public string? OpenAIKey { get; set; }

        [JsonProperty("search_endpoint")]
        public string? SearchEndpoint { get; set; }

        [JsonProperty("search_key")]
        public string? SearchKey { get; set; }

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

        [JsonProperty("embedding_model")]
        public string EmbeddingModel { get; set; }

        [JsonProperty("index_name")]
        public string? IndexName { get; set; }

        [JsonProperty("config_value_base_name")]
        public string ConfigValueBaseName { get; set; }
        [JsonProperty("top_n")]
        public int? TopN { get; set; } = 20;

        [JsonProperty("sources")]
        public string[]? Sources { get; set; }


        [JsonProperty("retriever_mode")]
        public string RetrieverMode { get; set; }

        [JsonProperty("load_mode")]
        public string LoadMode { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }
    }
}
