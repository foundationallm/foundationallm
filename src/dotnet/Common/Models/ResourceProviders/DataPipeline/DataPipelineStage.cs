using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provides the definition of a data pipeline stage.
    /// </summary>
    public class DataPipelineStage
    {
        /// <summary>
        /// Gets or sets the name of the stage.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of the data pipeline stages following this stage.
        /// </summary>
        [JsonPropertyName("next_stages")]
        public List<DataPipelineStage> NextStages { get; set; } = [];
    }
}
