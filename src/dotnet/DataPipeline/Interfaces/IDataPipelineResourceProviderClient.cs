using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.DataPipeline.Interfaces
{
    /// <summary>
    /// Defines the interface for a data pipeline resource provider client.
    /// </summary>
    public interface IDataPipelineResourceProviderClient
    {
        /// <summary>
        /// Sets the data pipeline resource provider.
        /// </summary>
        public IEnumerable<IResourceProviderService> ResourceProviders { set; }
    }
}
