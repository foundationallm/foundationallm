using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Provides a representation of a node in the knowledge graph rendering.
    /// </summary>
    public class KnowledgeGraphRenderingNode
    {
        /// <summary>
        /// Gets or sets the identifier of the node.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the label of the node.
        /// </summary>
        [JsonPropertyName("label")]
        public required string Label { get; set; }

        /// <summary>
        /// Gets or sets the description of the node.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
