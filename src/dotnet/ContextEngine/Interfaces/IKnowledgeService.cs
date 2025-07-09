using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context.Knowledge;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the service interface for the FoundationaLLM Knowledge Graph service.
    /// </summary>
    public interface IKnowledgeService
    {
        /// <summary>
        /// Updates a knowledge source.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The knowledge source+ identifier.</param>
        /// <param name="updateRequest"> The request containing the information to update the knowledge source.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task UpdateKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceUpdateRequest updateRequest,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Queries a knowledge source.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The knowledge source identifier.</param>
        /// <param name="queryRequest">The request containing the details of the query.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextKnowledgeSourceQueryResponse> QueryKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceQueryRequest queryRequest,
            UnifiedUserIdentity userIdentity);
    }
}
