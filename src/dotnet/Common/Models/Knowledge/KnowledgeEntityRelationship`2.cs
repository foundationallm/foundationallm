using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a subset of entities and relationship in a knowledge graph.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRelationship"></typeparam>
    public class KnowledgeEntityRelationship<TEntity, TRelationship>
        where TEntity: class
        where TRelationship : class
    {
        /// <summary>
        /// Gets or sets the list of entities.
        /// </summary>
        [JsonPropertyName("entities")]
        public required List<TEntity> Entities { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of relationships.
        /// </summary>
        [JsonPropertyName("relationships")]
        public required List<TRelationship> Relationships { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeEntityRelationship{TEntity, TRelationship}"/> class.
        /// </summary>
        public KnowledgeEntityRelationship()
        {
            Entities = [];
            Relationships = [];
        }
    }
}
