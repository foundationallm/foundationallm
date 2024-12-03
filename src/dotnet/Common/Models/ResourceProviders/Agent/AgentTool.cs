using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Prvides the settings for a tool that is registered with the agent.
    /// </summary>
    public class AgentTool
    {
        /// <summary>
        /// Gets or sets the name of the tool.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the tool.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the package name of the tool.
        /// For internal tools, this value will be FoundationaLLM
        /// For external tools, this value will be the name of the package.
        /// </summary>
        [JsonPropertyName("package_name")]
        public required string PackageName { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of AI model object identifiers.
        /// </summary>
        /// <remarks>
        /// The key is a value that is well-known to the tool, and the value is the AI model object identifier.
        /// </remarks>
        [JsonPropertyName("ai_model_object_ids")]
        public Dictionary<string, string> AIModelObjectIds { get; set; } = [];

        /// <summary>
        /// Gets of sets a dictionary of API endpoint configuration object identifiers.
        /// </summary>
        /// <remarks>
        /// The key is a value that is well-known to the tool, and the value is the API endpoint configuration object identifier.
        /// </remarks>
        [JsonPropertyName("api_endpoint_configuration_object_ids")]
        public Dictionary<string, string> APIEndpointConfigurationObjectIds { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of indexing profile object identifiers.
        /// </summary>
        [JsonPropertyName("indexing_profile_object_ids")]
        public Dictionary<string, string> IndexingProfileObjectIds { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of text embedding model names.
        /// </summary>
        [JsonPropertyName("text_embedding_model_names")]
        public Dictionary<string, string> TextEmbeddingModelNames { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of properties that are specific to the tool.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = [];
    }
}
