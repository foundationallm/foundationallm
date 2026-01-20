using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.Services;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the service interface for the FoundationaLLM Knowledge Graph service.
    /// </summary>
    public interface IKnowledgeService
    {
        /// <summary>
        /// Checks whether the specified knowledge unit name is available within the given instance.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">The knowledge unit name to validate. Cannot be null.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="ResourceNameCheckResult"/> indicating whether the knowledge unit name is available and, if not, the
        /// reason for unavailability.</returns>
        Task<Result<ResourceNameCheckResult>> CheckKnowledgeUnitName(
            string instanceId,
            ResourceName resourceName,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Checks whether the specified vector store identifier is available within the given instance.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="checkVectorStoreIdRequest">The vector store identifier validation request. Cannot be null.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="ResourceNameCheckResult"/> indicating whether the vector store identifier is available and, if not, the
        /// reason for unavailability.</returns>
        Task<Result<ResourceNameCheckResult>> CheckVectorStoreId(
            string instanceId,
            CheckVectorStoreIdRequest checkVectorStoreIdRequest,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Checks whether the specified knowledge source name is available within the given instance.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">The knowledge source name to validate. Cannot be null.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="ResourceNameCheckResult"/> indicating whether the knowledge source name is available and, if not, the
        /// reason for unavailability.</returns>
        Task<Result<ResourceNameCheckResult>> CheckKnowledgeSourceName(
            string instanceId,
            ResourceName resourceName,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves a specified knowledge unit.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <param name="agentName">The agent name if the request is being made on behalf of an agent.</param>
        /// <param name="options">The loading options for the knowledge unit.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ResourceProviderGetResult{T}"/> containing the <see cref="KnowledgeUnit"/> and its assigned roles/actions when successful; otherwise a failed result with a domain error.
        /// </returns>
        Task<Result<ResourceProviderGetResult<KnowledgeUnit>>> GetKnowledgeUnit(
            string instanceId,
            string knowledgeUnitId,
            string? agentName,
            ResourceProviderGetOptions? options,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves a specified knowledge source.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The knowledge source identifier.</param>
        /// <param name="agentName">The agent name if the request is being made on behalf of an agent.</param>
        /// <param name="options">The loading options for the knowledge source.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ResourceProviderGetResult{T}"/> containing the <see cref="KnowledgeSource"/> and its assigned roles/actions when successful; otherwise a failed result with a domain error.
        /// </returns>
        Task<Result<ResourceProviderGetResult<KnowledgeSource>>> GetKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            string? agentName,
            ResourceProviderGetOptions? options,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves the list of knowledge units.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="listRequest">The request containing the information used to filter the knowledge resources.</param>
        /// <param name="options">The loading options for the knowledge units.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is an enumerable of <see cref="ResourceProviderGetResult{T}"/> items for the matching <see cref="KnowledgeUnit"/> resources; otherwise a failed result with a domain error.
        /// </returns>
        Task<Result<IEnumerable<ResourceProviderGetResult<KnowledgeUnit>>>> GetKnowledgeUnits(
            string instanceId,
            ContextKnowledgeResourceListRequest listRequest,
            ResourceProviderGetOptions? options,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves the list of knowledge sources.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="listRequest">The request containing the information used to filter the knowledge resources.</param>
        /// <param name="options">The loading options for the knowledge sources.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is an enumerable of <see cref="ResourceProviderGetResult{T}"/> items for the matching <see cref="KnowledgeSource"/> resources; otherwise a failed result with a domain error.
        /// </returns>
        Task<Result<IEnumerable<ResourceProviderGetResult<KnowledgeSource>>>> GetKnowledgeSources(
            string instanceId,
            ContextKnowledgeResourceListRequest listRequest,
            ResourceProviderGetOptions? options,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates or updates a knowledge unit in the context service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnit">The knowledge unit to be created or updated.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ResourceProviderUpsertResult{T}"/> describing the upsert outcome for the <see cref="KnowledgeUnit"/>; otherwise a failed result with a domain error.
        /// </returns>
        Task<Result<ResourceProviderUpsertResult<KnowledgeUnit>>> UpsertKnowledgeUnit(
            string instanceId,
            KnowledgeUnit knowledgeUnit,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates or updates a knowledge source in the context service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSource">The knowledge source to be created or updated.</param>
        /// <param name="userIdentity">The identity of the security principal submitting the request.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ResourceProviderUpsertResult{T}"/> describing the upsert outcome for the <see cref="KnowledgeSource"/>; otherwise a failed result with a domain error.
        /// </returns>
        Task<Result<ResourceProviderUpsertResult<KnowledgeSource>>> UpsertKnowledgeSource(
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
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ResourceProviderActionResult"/> indicating the graph set operation outcome; otherwise a failed result with a domain error.
        /// </returns>
        Task<Result<ResourceProviderActionResult>> SetKnowledgeUnitGraph(
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
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ContextKnowledgeUnitRenderGraphResponse"/> containing nodes and edges when successful; otherwise a failed result with a domain error.
        /// </returns>
        Task<Result<ContextKnowledgeUnitRenderGraphResponse>> RenderKnowledgeUnitGraph(
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
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ContextKnowledgeSourceQueryResponse"/> containing vector store and/or knowledge graph results when successful; otherwise a failed result with a domain error.
        /// </returns>
        Task<Result<ContextKnowledgeSourceQueryResponse>> QueryKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceQueryRequest queryRequest,
            UnifiedUserIdentity userIdentity);
    }
}
