using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    /// <summary>
    /// The response returned from the Agent Hub.
    /// </summary>
    public record AgentHubResponse
    {
        /// <summary>
        /// Information about a requested agent from the Agent Hub.
        /// </summary>
        //[JsonObject]
        [JsonProperty("agent")]
        public AgentMetadata? Agent { get; set; }

    }

    /// <summary>
    /// The information about an agent returned from the Agent Hub.
    /// </summary>
    public class AgentMetadata : MetadataBase
    {
        /// <summary>
        /// The orchestration engine to use.
        /// </summary>
        [JsonProperty("orchestrator")]
        public string? Orchestrator { get; set; }

        /// <summary>
        /// Datasources that are used or available to the agent.
        /// </summary>
        [JsonProperty("allowed_data_source_names")]
        public List<string>? AllowedDataSourceNames { get; set; }

        /// <summary>
        /// The lanauge model used by the agent.
        /// </summary>
        [JsonProperty("language_model")]
        public LanguageModel? LanguageModel { get; set; }

        /// <summary>
        /// The embedding model used by the agent.
        /// </summary>
        [JsonProperty("embedding_model")]
        public EmbeddingModel? EmbeddingModel { get; set; }
    }
}