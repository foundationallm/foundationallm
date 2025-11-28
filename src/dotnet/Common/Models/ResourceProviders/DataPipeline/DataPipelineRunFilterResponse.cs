using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provider the model for a data pipeline run filter response.
    /// </summary>
    public class DataPipelineRunFilterResponse : ResourceBase
    {
        /// <summary>
        /// Gets or sets the list of data pipeline runs.
        /// </summary>
        [JsonPropertyName("items")]
        public List<DataPipelineRun> Items { get; set; } = [];

        /// <summary>
        /// Gets or sets the continuation token for pagination.
        /// </summary>
        [JsonPropertyName("continuation_token")]
        public string? ContinuationToken { get; set; }
    }
}
