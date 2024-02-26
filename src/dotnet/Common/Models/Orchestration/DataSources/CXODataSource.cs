using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.DataSources
{
    /// <summary>
    /// CXO data source metadata model.
    /// </summary>
    public class CXODataSource : DataSourceBase
    {
        /// <summary>
        /// Search Service configuration settings.
        /// </summary>
        [JsonPropertyName("configuration")]
        public CXOConfiguration? Configuration { get; set; }
    }
}
