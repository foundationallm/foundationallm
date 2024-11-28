using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Configuration
{
    /// <summary>
    /// Provides the configuration for DALL-E Image Generation.
    /// </summary>
    public class DalleImageGenerationConfiguration : ResourceBase
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

        /// <summary>
        /// The optional number of images to generate.
        /// </summary>
        [JsonPropertyName("n")]
        public int? N { get; set; }

        /// <summary>
        /// The optional quality setting for the images. Possible values are "standard" or "hd".
        /// </summary>
        [JsonPropertyName("quality")]
        public ImageQualityTypes? Quality { get; set; }

        /// <summary>
        /// The optional style setting for the images. Possible values are "natural" or "vivid".
        /// </summary>
        [JsonPropertyName("style")]
        public ImageStyleTypes? Style { get; set; }

        /// <summary>
        /// The optional size setting for the images. Possible values are "1024x1024", "1792x1024", or "1024x1792".
        /// </summary>
        [JsonPropertyName("size")]
        public ImageSizeTypes? Size { get; set; }
    }
}
