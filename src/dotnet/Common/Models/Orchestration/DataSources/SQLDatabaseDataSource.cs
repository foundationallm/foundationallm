using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.DataSources
{
    /// <summary>
    /// SQL Database data source metadata model.
    /// </summary>
    public class SQLDatabaseDataSource: DataSourceBase
    {
        /// <summary>
        /// SQL Database configuration settings.
        /// </summary>
        [JsonPropertyName("configuration")]
        public SQLDatabaseConfiguration? Configuration { get; set; }
    }
}
