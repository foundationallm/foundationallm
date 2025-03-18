using FoundationaLLM.Common.Constants.Plugins;
using FoundationaLLM.Common.Utils;
using NuGet.Versioning;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Plugin
{
    /// <summary>
    /// Provides the model for a plugin package.
    /// </summary>
    public class PluginPackageDefinition: ResourceBase
    {
        /// <summary>
        /// Gets or sets the platform of the package.
        /// </summary>
        /// <remarks>
        /// The supported values are defined in the <see cref="PluginPackagePlatform"/> enum.
        /// </remarks>
        [JsonPropertyName("package_platform")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required PluginPackagePlatform PackagePlatform { get; set; }

        /// <summary>
        /// Gets or sets the version of the package.
        /// </summary>
        [JsonPropertyName("package_version")]
        [JsonConverter(typeof(JsonStringSemanticVersionConverter))]
        public required SemanticVersion PackageVersion { get; set; }

        /// <summary>
        /// Gets or sets the path to the package file.
        /// </summary>
        [JsonPropertyName("package_file_path")]
        public required string PackageFilePath { get; set; }

        /// <summary>
        /// Gets or sets the size of the package file.
        /// </summary>
        [JsonPropertyName("package_file_size")]
        public required int PackageFileSize { get; set; }
    }
}
