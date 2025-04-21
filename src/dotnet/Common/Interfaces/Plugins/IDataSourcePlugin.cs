using FoundationaLLM.Common.Models.DataPipelines;

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
        List<DataPipelineContentItem> GetContentItems();
    }
}
