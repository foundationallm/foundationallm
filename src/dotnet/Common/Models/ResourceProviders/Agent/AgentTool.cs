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
    }
}
