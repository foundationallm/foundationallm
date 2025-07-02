using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the service interface for the FoundationaLLM Knowledge Graph service.
    /// </summary>
    public interface IKnowledgeGraphService
    {
        /// <summary>
        /// Updates a knowledge graph with the specified entities and relationships source files.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeGraphId">The knowledge graph identifier.</param>
        /// <param name="updateRequest"> The request containing the information to update the knowledge graph.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task UpdateKnowledgeGraph(
            string instanceId,
            string knowledgeGraphId,
            ContextKnowledgeGraphUpdateRequest updateRequest,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Queries a knowledge graph.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeGraphId">The knowledge graph identifier.</param>
        /// <param name="queryRequest">The request containing the details of the query.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextKnowledgeGraphQueryResponse> QueryKnowledgeGraph(
            string instanceId,
            string knowledgeGraphId,
            ContextKnowledgeGraphQueryRequest queryRequest,
            UnifiedUserIdentity userIdentity);
    }
}
