using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataSource
{
    /// <summary>
    /// Web Site data source.
    /// </summary>
    public class WebSiteDataSource : DataSourceBase
    {
        /// <summary>
        /// The list of site URLs covered by the data source.
        /// A URL can point to a specific path within the site.
        /// The list can contain URLs with different base paths.
        /// </summary>
        [JsonPropertyName("site_urls")]
        public List<string> SiteUrls { get; set; } = [];

        public WebSiteDataSource() =>
            Type = DataSourceTypes.WebSite;
    }
}
