using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataSource
{
    /// <summary>
    /// Azure Data Lake data source.
    /// </summary>
    public class AzureDataLakeDataSource : DataSourceBase
    {
        /// <summary>
        /// The list of folders from the data lake. The format is [container_name]/[folder_path].
        /// </summary>
        [JsonPropertyName("folders")]
        public List<string> Folders { get; set; } = [];

        /// <summary>
        /// Creates a new instance of the <see cref="AzureDataLakeDataSource"/> data source.
        /// </summary>
        public AzureDataLakeDataSource() =>
            Type = DataSourceTypes.AzureDataLake;
    }
}
