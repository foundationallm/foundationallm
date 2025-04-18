using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces.Plugins;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataSource
{
    /// <summary>
    /// Implements the FoundationaLLM Context File data source plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class ContextFileDataSourcePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, serviceProvider), IDataSourcePlugin
    {
        protected override string Name => PluginNames.CONTEXTFILE_DATASOURCE;

        public List<string> GetContentItems()
        {
            var contextFilePath = _pluginParameters[PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID]?.ToString()
                ?? throw new PluginException($"The {PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID} parameter is required by the {Name} plugin.");

            return [contextFilePath];
        }
    }
}
