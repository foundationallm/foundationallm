using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata
{
    /// <summary>
    /// 
    /// </summary>
    public class EmbeddingModel
    {
        /// <summary>
        /// The type of the embedding model.
        /// </summary>
        [JsonProperty("type")]
        public string? Type { get; set; }

        /// <summary>
        /// The provider of the emberdding model.
        /// </summary>
        [JsonProperty("provider")]
        public string? Provider { get; set; }

        /// <summary>
        /// Name of the embedding model deployment.
        /// </summary>
        [JsonProperty("deployment")]
        public string? Deployment { get; set; }

        /// <summary>
        /// Name of the embedding model.
        /// </summary>
        [JsonProperty("model")]
        public string? Model { get; set; }

        /// <summary>
        /// The chunk size to use when creating vectors from the embedding model.
        /// </summary>
        [JsonProperty("chunk_size")]
        public int ChunkSize { get; set; } = 1000;
    }
}
