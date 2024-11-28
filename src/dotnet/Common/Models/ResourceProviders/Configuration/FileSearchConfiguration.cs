using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Configuration
{
    /// <summary>
    /// Provides the configuration for Azure OpenAI Assistants File Search.
    /// </summary>
    public class FileSearchConfiguration : ResourceBase
    {
        /// <summary>
        /// The package name of the tool.
        /// </summary>
        [JsonPropertyName("package_name")]
        public required string PackageName { get; set; }
    }
}
