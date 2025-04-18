using FoundationaLLM.Common.Interfaces.Plugins;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Text Partitioning Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    public class TextPartitioningDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, serviceProvider), IDataPipelineStagePlugin
    {
    }
}
