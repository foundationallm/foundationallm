using FoundationaLLM.Common.Constants.Plugins;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Plugins;

namespace FoundationaLLM.Plugins.DataPipeline
{
    /// <summary>
    /// The plugin package manager.
    /// </summary>
    public class PluginPackageManager : IPluginPackageManager
    {
        private const string PACKAGE_NAME = "Dotnet-FoundationaLLMDataPipelinePlugins";

        /// <inheritdoc/>
        public PluginPackageConfiguration GetConfiguration(string instanceId) => new()
        {
            PluginPackageName = PACKAGE_NAME,
            PluginPackageDisplayName = "FoundationaLLM Data Pipeline Plugins (.NET)",
            PluginPackageDescription = "The FoundationaLLM Data Pipeline plugins package for .NET.",
            PluginPackagePlatform = PluginPackagePlatform.Dotnet,
            PluginConfigurations = [
                new() {
                    PluginObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-AzureDataLakeDataSource",
                    PluginName = $"{PACKAGE_NAME}-AzureDataLakeDataSource",
                    PluginDisplayName = "Azure Data Lake Data Source (FoundationaLLM)",
                    PluginDescription = "The Azure Data Lake FoundationaLLM DataSource plugin.",
                    PluginCategory = PluginCategoryNames.DataSource
                }
            ]
        };
    }
}
