using FoundationaLLM.Common.Models.Plugins;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provides the definition of a data pipeline stage.
    /// </summary>
    public class DataPipelineStage : PluginComponent
    {
        /// <summary>
        /// Gets or sets the list of the data pipeline stages following this stage.
        /// </summary>
        [JsonPropertyName("next_stages")]
        public List<DataPipelineStage> NextStages { get; set; } = [];

        /// <summary>
        /// Gets the name of the data pipeline stage and the names of all its next stages.
        /// </summary>
        [JsonIgnore]
        public List<string> AllStageNames =>
            [.. NextStages.SelectMany(stage => stage.AllStageNames).Prepend(Name)];


        /// <summary>
        /// Gets the data pipeline stage and all its next stages.
        /// </summary>
        [JsonIgnore]
        public List<DataPipelineStage> AllStages =>
            [.. NextStages.SelectMany(stage => stage.AllStages).Prepend(this)];
    }
}
