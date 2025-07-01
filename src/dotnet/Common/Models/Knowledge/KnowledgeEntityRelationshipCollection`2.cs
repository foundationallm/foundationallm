using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a subset of entities and relationship in a knowledge graph.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRelationship"></typeparam>
    public class KnowledgeEntityRelationshipCollection<TEntity, TRelationship>
        where TEntity: class, new()
        where TRelationship : class, new()
    {
        /// <summary>
        /// Gets or sets the list of entities.
        /// </summary>
        [JsonPropertyName("entities")]
        public List<TEntity> Entities { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of relationships.
        /// </summary>
        [JsonPropertyName("relationships")]
        public List<TRelationship> Relationships { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeEntityRelationshipCollection{TEntity, TRelationship}"/> class.
        /// </summary>
        public KnowledgeEntityRelationshipCollection()
        {
            Entities = [];
            Relationships = [];
        }
    }
}
