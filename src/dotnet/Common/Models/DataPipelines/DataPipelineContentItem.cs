using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents a content item in a data pipeline.
    /// </summary>
    public class DataPipelineContentItem
    {
        /// <summary>
        /// Gets or sets the unique identifier of the content item.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonPropertyOrder(1)]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos DB item type.
        /// </summary>
        /// <remarks>
        /// Must always be set to <see cref="DataPipelineTypes.DataPipelineContentItem"/>.
        /// </remarks>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(2)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the data pipeline run that is processing the content item.
        /// </summary>
        [JsonPropertyName("run_id")]
        [JsonPropertyOrder(3)]
        public string? RunId { get; set; }

        /// <summary>
        /// Gets or sets the FoundationaLLM object identifier of the data source that provides the content item.
        /// </summary>
        [JsonPropertyName("data_source_object_id")]
        [JsonPropertyOrder(4)]
        public required string DataSourceObjectId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the content item.
        /// </summary>
        [JsonPropertyName("content_identifier")]
        [JsonPropertyOrder(5)]
        public required ContentIdentifier ContentIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the content action to be performed on the content item.
        /// </summary>
        /// <remarks>
        /// The valid values are defined in <see cref="ContentItemActions"/>.
        /// </remarks>
        [JsonPropertyName("content_action")]
        [JsonPropertyOrder(6)]
        public required string ContentAction { get; set; }

        /// <summary>
        /// Gets or sets the additional metadata associated with the content item.
        /// </summary>
        [JsonPropertyName("metadata")]
        [JsonPropertyOrder(7)]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPipelineContentItem"/> class.
        /// </summary>
        public DataPipelineContentItem() =>
            Type = DataPipelineTypes.DataPipelineContentItem;
    }
}
