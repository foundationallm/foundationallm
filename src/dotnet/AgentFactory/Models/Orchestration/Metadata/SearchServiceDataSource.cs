using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata
{
    /// <summary>
    /// Search service data source metadata model.
    /// </summary>
    public class SearchServiceDataSource : DataSourceBase
    {
        /// <summary>
        /// Search Service configuration settings.
        /// </summary>
        [JsonProperty("configuration")]
        public SearchServiceConfiguration? Configuration { get; set; }

    }
}
