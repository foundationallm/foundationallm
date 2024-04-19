using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataSource
{
    /// <summary>
    /// SharePoint Online Site data source.
    /// </summary>
    public class SharePointOnlineSiteDataSource : DataSourceBase
    {
        /// <summary>
        /// The URL of the SharePoint Online site.
        /// </summary>
        [JsonPropertyName("site_url")]
        public string? SiteUrl { get; set; }

        /// <summary>
        /// The list of paths of document libraries from the SharePoint online site. The paths must be relative to the site URL.
        /// </summary>
        [JsonPropertyName("document_libraries")]
        public List<string> DocumentLibraries { get; set; } = [];

        /// <summary>
        /// Creates a new instance of the <see cref="SharePointOnlineSiteDataSource"/> data source.
        /// </summary>
        public SharePointOnlineSiteDataSource() =>
            Type = DataSourceTypes.SharePointOnlineSite;
    }
}
