using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Configuration
{
    /// <summary>
    /// Provides the configuration for Image Analysis.
    /// </summary>
    public class ImageAnalysisConfiguration : ResourceBase
    {
        /// <summary>
        /// The package name of the tool.
        /// </summary>
        [JsonPropertyName("package_name")]
        public required string PackageName { get; set; }

        /// <summary>
        /// The AI model object IDs, with a required key "main_model" specifying the vision-enabled AI model.
        /// </summary>
        [JsonPropertyName("ai_model_object_ids")]
        public required Dictionary<string, string> AiModelObjectIds { get; set; }
    }
}
