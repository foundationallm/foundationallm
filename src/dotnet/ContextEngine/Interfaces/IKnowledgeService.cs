using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders.Context;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the service interface for the FoundationaLLM Knowledge Graph service.
    /// </summary>
    public interface IKnowledgeService
    {
        /// <summary>
        /// Retrieves the list of knowledge sources.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="listRequest">The request containing the information used to filter the knowledge sources.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<IEnumerable<KnowledgeSource>> GetKnowledgeSources(
            string instanceId,
            ContextKnowledgeSourceListRequest listRequest,
            UnifiedUserIdentity userIdentity);

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

        /// <summary>
        /// Retrieves the knowledge source's knowledge graph in a format suitable for visualization or further processing.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The knowledge source identifier.</param>
        /// <param name="queryRequest">The request containing the details of the query.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextKnowledgeSourceRenderGraphResponse> RenderKnowledgeSourceGraph(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceQueryRequest? queryRequest,
            UnifiedUserIdentity userIdentity);
    }
}
