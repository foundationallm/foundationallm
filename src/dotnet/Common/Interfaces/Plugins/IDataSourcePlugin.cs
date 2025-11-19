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
        /// <param name="contentItemIdentifier">The identifier of the content item.</param>
        /// <returns>A <see cref="PluginResult{T}"/> object with the conten item's raw content.</returns>
        Task<PluginResult<ContentItemRawContent>> GetContentItemRawContent(
            ContentIdentifier contentItemIdentifier);

        /// <summary>
        /// Handles an unsafe content item identified by its canonical identifier.
        /// </summary>
        /// <param name="canonicalContentItemIdentifier">The canonical identifier of the content item to be handled. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task HandleUnsafeContentItem(
            string canonicalContentItemIdentifier);
    }
}
