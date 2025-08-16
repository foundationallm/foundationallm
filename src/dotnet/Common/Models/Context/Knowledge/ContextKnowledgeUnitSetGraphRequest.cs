using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents a request to update the knowledge graph associated with a knowledge unit
    /// in the FoundationaLLM Context API.
    /// </summary>
    public class ContextKnowledgeUnitSetGraphRequest
    {
        /// <summary>
        /// Gets or sets the path of the source file containing the entities for the knowledge graph.
        /// </summary>
        [JsonPropertyName("entities_source_file_path")]
        public required string EntitiesSourceFilePath { get; set; }

        /// <summary>
        /// Gets or sets the path of the source file containing the relationships for the knowledge graph.
        /// </summary>
        [JsonPropertyName("relationships_source_file_path")]
        public required string RelationshipsSourceFilePath { get; set; }
    }
}
