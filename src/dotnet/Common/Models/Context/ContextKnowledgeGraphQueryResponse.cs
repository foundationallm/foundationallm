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
        /// Gets or sets the list of knowledge entities that are part of the result.
        /// </summary>
        [JsonPropertyName("entities")]
        public List<KnowledgeEntity> Entities { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of knowledge relationsips that are part of the result.
        /// </summary>
        [JsonPropertyName("relationships")]
        public List<KnowledgeRelationship> Relationships { get; set; } = [];
    }
}
