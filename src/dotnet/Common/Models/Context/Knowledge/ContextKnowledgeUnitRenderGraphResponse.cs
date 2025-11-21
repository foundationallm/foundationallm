using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents the response to the request to render a knowledge unit's knowledge graph.
    /// </summary>
    public class ContextKnowledgeUnitRenderGraphResponse
    {
        /// <summary>
        /// Gets or sets the list of nodes in the knowledge graph.
        /// </summary>
        [JsonPropertyName("nodes")]
        public List<KnowledgeGraphRenderingNode> Nodes { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of edges in the knowledge graph, where each edge is represented as a list of two node identifiers.
        /// </summary>
        [JsonPropertyName("edges")]
        public List<List<string>> Edges { get; set; } = [];
    }
}
