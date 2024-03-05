using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.DataSource.Constants;
using System.Text.Json.Serialization;

namespace FoundationaLLM.DataSource.Models
{
    /// <summary>
    /// Basic data source.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(AzureDataLakeDataSource), DataSourceTypes.AzureDataLake)]
    [JsonDerivedType(typeof(AzureSQLDatabaseDataSource), DataSourceTypes.AzureSQLDatabase)]
    [JsonDerivedType(typeof(SharePointOnlineSiteDataSource), DataSourceTypes.SharePointOnlineSite)]
    public class DataSourceBase : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// Configuration references associated with the data source.
        /// </summary>
        [JsonPropertyName("configuration_references")]
        public Dictionary<string, string>? ConfigurationReferences { get; set; } = [];
    }
}
