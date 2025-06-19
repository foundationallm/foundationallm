using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;

namespace FoundationaLLM.Common.Interfaces.Plugins
{
    /// <summary>
    /// Defines the interface for a data source plugin.
    /// </summary>
    public interface IDataSourcePlugin
    {
        /// <summary>
        /// Gets the list of content items from the data source.
        /// </summary>
        /// <returns></returns>
        Task<List<DataPipelineContentItem>> GetContentItems();

        /// <summary>
        /// Gets the raw content of a content item.
        /// </summary>
        /// <param name="contentItemCanonicalId">The canonical identifier of the content item.</param>
        /// <returns>A <see cref="PluginResult{T}"/> object with the conten item's raw content.</returns>
        Task<PluginResult<ContentItemRawContent>> GetContentItemRawContent(
            string contentItemCanonicalId);
    }
}
