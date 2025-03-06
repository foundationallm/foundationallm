using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provides the model for a data pipeline data source.
    /// </summary>
    public class DataPipelineDataSource
    {


        /// <summary>
        /// Gets or sets the object identifier of the FoundationaLLM DataSource resource
        /// used to connect to the data source.
        /// </summary>
        [JsonPropertyName("data_source_object_id")]
        public required string DataSourceObjectId { get; set; }
    }
}
