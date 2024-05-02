using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataSource
{
    /// <summary>
    /// Basic data source.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(OneLakeDataSource), DataSourceTypes.OneLake)]
    [JsonDerivedType(typeof(AzureDataLakeDataSource), DataSourceTypes.AzureDataLake)]
    [JsonDerivedType(typeof(AzureSQLDatabaseDataSource), DataSourceTypes.AzureSQLDatabase)]
    [JsonDerivedType(typeof(SharePointOnlineSiteDataSource), DataSourceTypes.SharePointOnlineSite)]
    [JsonDerivedType(typeof(WebSiteDataSource), DataSourceTypes.WebSite)]
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
