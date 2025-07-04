using FoundationaLLM.Common.Models.Knowledge;

namespace FoundationaLLM.Context.Models
{
    /// <summary>
    /// Represents an index for organizing and retrieving information in a knowledge graph.
    /// </summary>
    public class KnowledgeGraphIndex
    {
        /// <summary>
        /// Gets or sets the collection of nodes in the knowledge graph, indexed by their unique identifiers.
        /// </summary>
        public Dictionary<string, KnowledgeGraphIndexNode> Nodes { get; set; } = [];

        public static KnowledgeGraphIndex Create(
            IEnumerable<KnowledgeEntity> entities,
            IEnumerable<KnowledgeRelationship> relationships)
        {
            var graphIndex = new KnowledgeGraphIndex
            {
                Nodes = entities
                    .ToDictionary(
                        entity => entity.UniqueId,
                        entity => new KnowledgeGraphIndexNode
                        {
                            Entity = entity,
                            RelatedNodes = []
                        })
            };

            foreach (var relationship in relationships)
            {
                if (graphIndex.Nodes.TryGetValue(relationship.SourceUniqueId, out var sourceNode) &&
                    graphIndex.Nodes.TryGetValue(relationship.TargetUniqueId, out var targetNode))
                {
                    sourceNode.RelatedNodes.Add(new KnowledgeGraphIndexRelatedNode
                    {
                        Entity = targetNode.Entity,
                        Relationship = relationship
                    });
                }
            }

            foreach (var node in graphIndex.Nodes.Values)
            {
                node.RelatedNodes.Sort((a, b) =>
                    b.Relationship.Strengths.Max()
                    .CompareTo(a.Relationship.Strengths.Max()));
            }

            return graphIndex;
        }
    }
}
