using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Hubs
{
    /// <summary>
    /// The format of a Data Source Hub Request.
    /// </summary>
    public record DataSourceHubRequest
    {
        /// <summary>
        /// The session ID.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }

        /// <summary>
        /// List of data sources to be returned from the Data Source Hub.
        /// </summary>
        [JsonPropertyName("data_sources")]
        public List<string>? DataSources { get; set; }

    }
}
