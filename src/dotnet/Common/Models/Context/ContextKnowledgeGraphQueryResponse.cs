using FoundationaLLM.Common.Models.Knowledge;

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
        public List<KnowledgeEntity> Entities { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of knowledge relationsips that are part of the result.
        /// </summary>
        public List<KnowledgeRelationship> Relationships { get; set; } = [];
    }
}
