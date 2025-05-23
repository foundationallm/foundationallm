namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Represents a snapshot of a data pipeline definition.
    /// </summary>
    public class DataPipelineDefinitionSnapshot : ResourceBase
    {
        /// <summary>
        /// Gets or sets the data pipeline definition.
        /// </summary>
        public required DataPipelineDefinition DataPipelineDefinition { get; set; }
    }
}
