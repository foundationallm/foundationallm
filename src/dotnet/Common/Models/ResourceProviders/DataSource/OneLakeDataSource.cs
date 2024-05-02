using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataSource
{
    /// <summary>
    /// Azure Data Lake data source.
    /// </summary>
    public class OneLakeDataSource : DataSourceBase
    {
        /// <summary>
        /// The list of workspaces from the data lake. The format is [workspace_name].
        /// </summary>
        [JsonPropertyName("workspaces")]
        public List<string> Workspaces { get; set; } = [];

        /// <summary>
        /// Creates a new instance of the <see cref="OneLakeDataSource"/> data source.
        /// </summary>
        public OneLakeDataSource() =>
            Type = DataSourceTypes.OneLake;
    }
}
