using FoundationaLLM.Common.Models.Plugins.Metadata;

namespace FoundationaLLM.Common.Interfaces.Plugins
{
    /// <summary>
    /// Provides methods to manage plugins in FoundationaLLM plugin packages.
    /// </summary>
    public interface IPluginPackageManager
    {
        /// <summary>
        /// Gets the plugin package configuration with the plugin configurations of to the plugins in the package.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns>An object of type <see cref="PluginPackageMetadata"/>.</returns>
        PluginPackageMetadata GetMetadata(string instanceId);

        /// <summary>
        /// Gets a data source plugin by its name.
        /// </summary>
        /// <param name="pluginName">The name of the data source plugin.</param>
        /// <param name="dataSourceObjectId">The FoundationaLLM object identifier of the data source.</param>
        /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
        /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
        /// <returns>The <see cref="IDataSourcePlugin"/> interface implemented by the data source plugin.</returns>
        /// <exception cref="NotImplementedException"></exception>
        IDataSourcePlugin GetDataSourcePlugin(
            string pluginName,
            string dataSourceObjectId,
            Dictionary<string, object> pluginParameters,
            IServiceProvider serviceProvider);

        /// <summary>
        /// Gets a data pipeline stage plugin by its name.
        /// </summary>
        /// <param name="pluginName">The name of the data pipeline stage plugin.</param>
        /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
        /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
        /// <returns>The <see cref="IDataPipelineStagePlugin"/> interface implemented by the data pipeline stage plugin.</returns>
        /// <exception cref="NotImplementedException"></exception>
        IDataPipelineStagePlugin GetDataPipelineStagePlugin(
            string pluginName,
            Dictionary<string, object> pluginParameters,
            IServiceProvider serviceProvider);

        /// <summary>
        /// Gets a content text extraction plugin by its name.
        /// </summary>
        /// <param name="pluginName">The name of the content text extraction plugin.</param>
        /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
        /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
        /// <returns>The <see cref="IContentTextExtractionPlugin"/> interface implemented by the content text extraction plugin.</returns>
        /// <exception cref="NotImplementedException"></exception>
        IContentTextExtractionPlugin GetContentTextExtractionPlugin(
            string pluginName,
            Dictionary<string, object> pluginParameters,
            IServiceProvider serviceProvider);

        /// <summary>
        /// Gets a content text partitioning plugin by its name.
        /// </summary>
        /// <param name="pluginName">The name of the content text partitioning plugin.</param>
        /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
        /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
        /// <returns>The <see cref="IContentTextExtractionPlugin"/> interface implemented by the content text partitioning plugin.</returns>
        /// <exception cref="NotImplementedException"></exception>
        IContentTextPartitioningPlugin GetContentTextPartitioningPlugin(
            string pluginName,
            Dictionary<string, object> pluginParameters,
            IServiceProvider serviceProvider);
    }
}
