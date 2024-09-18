using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using System.Text.Json.Serialization;

namespace FoundationaLLM.DataSource.Models
{
    /// <summary>
    /// Contains a reference to a data source..
    /// </summary>
    public class DataSourceReference : ResourceReference
    {
        /// <summary>
        /// The object type of the data source.
        /// </summary>
        [JsonIgnore]
        public Type DataSourceType =>
            Type switch
            {
                DataSourceTypes.Basic => typeof(DataSourceBase),
                DataSourceTypes.OneLake => typeof(OneLakeDataSource),
                DataSourceTypes.AzureDataLake => typeof(AzureDataLakeDataSource),
                DataSourceTypes.AzureSQLDatabase => typeof(AzureSQLDatabaseDataSource),
                DataSourceTypes.SharePointOnlineSite => typeof(SharePointOnlineSiteDataSource),
                DataSourceTypes.WebSite => typeof(WebSiteDataSource),
                _ => throw new ResourceProviderException($"The data source type {Type} is not supported.")
            };
    }
}
