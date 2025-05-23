using FoundationaLLM.Common.Models.Authentication;

namespace FoundationaLLM.Common.Interfaces.Plugins
{
    /// <summary>
    /// Defines the contract for a plugin service.
    /// </summary>
    public interface IPluginService
    {
        /// <summary>
        /// Retrieves the data source plugin based on the specified plugin object identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="pluginObjectId">The plugin object identifier.</param>
        /// <param name="pluginParameters">The dictionary containing the names and values of the plugin parameters.</param>
        /// <param name="dataSourceObjectId">The FoundationaLLM object identifier of the data source.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <returns>The data source plugin.</returns>
        Task<IDataSourcePlugin> GetDataSourcePlugin(
            string instanceId,
            string pluginObjectId,
            Dictionary<string, object> pluginParameters,
            string dataSourceObjectId,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves the data pipeline stage plugin based on the specified plugin object identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="pluginObjectId">The plugin object identifier.</param>
        /// <param name="pluginParameters">The dictionary containing the names and values of the plugin parameters.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <returns>The data pipeline stage plugin.</returns>
        Task<IDataPipelineStagePlugin> GetDataPipelineStagePlugin(
            string instanceId,
            string pluginObjectId,
            Dictionary<string, object> pluginParameters,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves the data source plugin based on the specified plugin object identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="pluginObjectId">The plugin object identifier.</param>
        /// <param name="userIdentity">The identity of the user running the operation.</param>
        /// <returns>The plugin package manager instance that manages the plugin.</returns>
        Task<IPluginPackageManager> GetPluginPackageManager(
            string instanceId,
            string pluginObjectId,
            UnifiedUserIdentity userIdentity);
    }
}
