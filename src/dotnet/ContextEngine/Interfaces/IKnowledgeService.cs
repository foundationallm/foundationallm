using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the service interface for the FoundationaLLM Knowledge Graph service.
    /// </summary>
    public interface IKnowledgeService
    {
        /// <summary>
        /// Retrieves a specified knowledge unit.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <param name="agentName">The agent name if the request is being made on behalf of an agent.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextServiceResponse<ResourceProviderGetResult<KnowledgeUnit>>> GetKnowledgeUnit(
            string instanceId,
            string knowledgeUnitId,
            string? agentName,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves a specified knowledge source.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The knowledge source identifier.</param>
        /// <param name="agentName">The agent name if the request is being made on behalf of an agent.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextServiceResponse<ResourceProviderGetResult<KnowledgeSource>>> GetKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            string? agentName,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves the list of knowledge units.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="listRequest">The request containing the information used to filter the knowledge resources.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextServiceResponse<IEnumerable<ResourceProviderGetResult<KnowledgeUnit>>>> GetKnowledgeUnits(
            string instanceId,
            ContextKnowledgeResourceListRequest listRequest,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves the list of knowledge sources.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="listRequest">The request containing the information used to filter the knowledge resources.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextServiceResponse<IEnumerable<ResourceProviderGetResult<KnowledgeSource>>>> GetKnowledgeSources(
            string instanceId,
            ContextKnowledgeResourceListRequest listRequest,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates or updates a knowledge unit in the context service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnit">The knowledge unit to be created or updated.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextServiceResponse<ResourceProviderUpsertResult<KnowledgeUnit>>> UpsertKnowledgeUnit(
            string instanceId,
            KnowledgeUnit knowledgeUnit,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates or updates a knowledge source in the context service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSource">The knowledge source to be created or updated.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextServiceResponse<ResourceProviderUpsertResult<KnowledgeSource>>> UpsertKnowledgeSource(
            string instanceId,
            KnowledgeSource knowledgeSource,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Sets the knowledge graph associated with a knowledge unit.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <param name="setGraphRequest">The request containing the knowledge graph details.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextServiceResponse<ResourceProviderActionResult>> SetKnowledgeUnitGraph(
            string instanceId,
            string knowledgeUnitId,
            ContextKnowledgeUnitSetGraphRequest setGraphRequest,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves the knowledge unit's knowledge graph in a format suitable for visualization or further processing.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <param name="queryRequest">The request containing the details of the query.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns></returns>
        Task<ContextKnowledgeUnitRenderGraphResponse> RenderKnowledgeUnitGraph(
            string instanceId,
            string knowledgeUnitId,
            ContextKnowledgeSourceQueryRequest? queryRequest,
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
