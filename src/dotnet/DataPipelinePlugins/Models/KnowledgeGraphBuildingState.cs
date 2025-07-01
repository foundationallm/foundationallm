namespace FoundationaLLM.Plugins.DataPipeline.Models
{
    /// <summary>
    /// Represents the state of the knowledge graph building process.
    /// </summary>
    public class KnowledgeGraphBuildingState
    {
        /// <summary>
        /// Gets or sets the name of the last step that was successfully completed in the knowledge graph building process.
        /// </summary>
        public string? LastSuccessfullStep { get; set; }
    }
}
