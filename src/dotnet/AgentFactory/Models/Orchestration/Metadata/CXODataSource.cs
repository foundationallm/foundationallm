using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata
{
    /// <summary>
    /// CXO data source metadata model.
    /// </summary>
    public class CXODataSource : DataSourceBase
    {
        /// <summary>
        /// Search Service configuration settings.
        /// </summary>
        [JsonProperty("configuration")]
        public CXOConfiguration? Configuration { get; set; }
    }
}
