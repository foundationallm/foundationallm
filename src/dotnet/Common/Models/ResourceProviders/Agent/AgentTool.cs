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
        /// Gets or sets a dictionary of Azure OpenAI File Search configurations.
        /// </summary>
        /// <remarks>
        /// The key is a value well-known to the tool, and the value is the configuration object identifier for the File Search.
        /// </remarks>
        [JsonPropertyName("file_search_configuration_object_ids")]
        public Dictionary<string, string> FileSearchConfigurationObjectIds { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of Assistant Code Interpreter configurations.
        /// </summary>
        /// <remarks>
        /// The key is a value well-known to the tool, and the value is the configuration object identifier for the Code Interpreter.
        /// </remarks>
        [JsonPropertyName("code_interpreter_configuration_object_ids")]
        public Dictionary<string, string> CodeInterpreterConfigurationObjectIds { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of DALL-E Image Generation configurations.
        /// </summary>
        /// <remarks>
        /// The key is a value well-known to the tool, and the value is the configuration object identifier for DALL-E Image Generation.
        /// </remarks>
        [JsonPropertyName("dalle_image_generation_configuration_object_ids")]
        public Dictionary<string, string> DalleImageGenerationConfigurationObjectIds { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of Image Analysis configurations.
        /// </summary>
        /// <remarks>
        /// The key is a value well-known to the tool, and the value is the configuration object identifier for Image Analysis.
        /// </remarks>
        [JsonPropertyName("image_analysis_configuration_object_ids")]
        public Dictionary<string, string> ImageAnalysisConfigurationObjectIds { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of properties that are specific to the tool.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = [];
    }
}
