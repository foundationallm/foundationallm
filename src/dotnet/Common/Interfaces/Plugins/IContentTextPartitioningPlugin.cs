using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;

namespace FoundationaLLM.Common.Interfaces.Plugins
{
    /// <summary>
    /// Defines the interface for a content text partitioning plugin.
    /// </summary>
    public interface IContentTextPartitioningPlugin
    {
        /// <summary>
        /// Partitions the provided text into chunks.
        /// </summary>
        /// <param name="text">The text to be partitioned.</param>
        /// <param name="contentItemCanonicalId"> The canonical identifier of the content item.</param>
        /// <returns>A list of text chunks.</returns>
        Task<PluginResult<List<DataPipelineContentItemPart>>> PartitionText(
            string contentItemCanonicalId,
            string text);
    }
}
