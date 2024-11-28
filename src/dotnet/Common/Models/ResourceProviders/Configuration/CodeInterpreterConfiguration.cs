using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Configuration
{
    /// <summary>
    /// Provides the configuration for the Assistants Code Interpreter.
    /// </summary>
    public class CodeInterpreterConfiguration : ResourceBase
    {
        /// <summary>
        /// The package name of the tool.
        /// </summary>
        [JsonPropertyName("package_name")]
        public required string PackageName { get; set; }
    }
}
