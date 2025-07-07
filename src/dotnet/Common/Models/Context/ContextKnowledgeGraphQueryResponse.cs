using FoundationaLLM.Common.Models.Knowledge;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents the result of a knowledge graph query.
    /// </summary>
    public class ContextKnowledgeGraphQueryResponse : ContextTextChunkQueryResponse
    {
        /// <summary>
        /// Gets or sets the list of knowledge entities that most similar to the user prompt in the query.
        /// </summary>
        [JsonPropertyName("entities")]
        public List<KnowledgeEntity> Entities { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of knowledge entities that are related to the ones in <see cref="Entities"/>
        /// and are most similar to the user prompt in the query.
        /// </summary>
        [JsonPropertyName("related_entities")]
        public List<KnowledgeEntity> RelatedEntities { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of knowledge relationsips that exist between entities in <see cref="Entities"/>
        /// and entities in <see cref="RelatedEntities"/>.
        /// </summary>
        [JsonPropertyName("relationships")]
        public List<KnowledgeRelationship> Relationships { get; set; } = [];
    }
}
