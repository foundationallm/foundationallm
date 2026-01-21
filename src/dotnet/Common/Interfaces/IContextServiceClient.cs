using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.Services;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides methods to call the FoundationaLLM Context API service.
    /// </summary>
    public interface IContextServiceClient
    {
        /// <summary>
        /// Calls the Context API service to create a file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="fileName">The name of the file to be created.</param>
        /// <param name="fileContentType">The content type of the file to be created.</param>
        /// <param name="fileContent">The binary content of the file.</param>
        /// <returns>A <see cref="Result{T}"/> instance where <c>T</c> is of type <see cref="ContextFileRecord"/>.</returns>
        Task<Result<ContextFileRecord>> CreateFileForConversation(
            string instanceId,
            string agentName,
            string conversationId,
            string fileName,
            string fileContentType,
            BinaryData fileContent);

        /// <summary>
        /// Calls the Context API service to create a file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="fileName">The name of the file to be created.</param>
        /// <param name="fileContentType">The content type of the file to be created.</param>
        /// <param name="fileContent">The binary content of the file.</param>
        /// <returns>A <see cref="Result{T}"/> instance where <c>T</c> is of type <see cref="ContextFileRecord"/>.</returns>
        Task<Result<ContextFileRecord>> CreateFileForAgent(
            string instanceId,
            string agentName,
            string fileName,
            string fileContentType,
            BinaryData fileContent);

        /// <summary>
        /// Calls the Context API service to get the content of a file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The unique identifier of the file.</param>
        /// <returns>A <see cref="Result{T}"/> instance where <c>T</c> is of type <see cref="ContextFileContent"/>.</returns>
        Task<Result<ContextFileContent>> GetFileContent(
            string instanceId,
            string fileId);

        /// <summary>
        /// Calls the Context API service to get a file record.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The unique identifier of the file.</param>
        /// <returns>A <see cref="Result{T}"/> instance where <c>T</c> is of type <see cref="ContextFileRecord"/>.</returns>
        Task<Result<ContextFileRecord>> GetFileRecord(
            string instanceId,
            string fileId);

        /// <summary>
        /// Calls the Context API service to delete a file record.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The unique identifier of the file.</param>
        /// <returns>A <see cref="Result"/> instance .</returns>
        Task<Result> DeleteFileRecord(
            string instanceId,
            string fileId);

        /// <summary>
        /// Calls the Context API service to create a code session.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="context">The context within the conversation in which the code session must be created (e.g., an agent tool name).</param>
        /// <param name="endpointProvider">The name of the code session endpoint provider.</param>
        /// <param name="endpointProviderOverride">Optional user level override for the code session endpoint provider.</param>
        /// <param name="language">The programming language of the code session.</param>
        /// <returns>A <see cref="Result{T}"/> instance where <c>T</c> is of type <see cref="CreateCodeSessionResponse"/>.</returns>
        Task<Result<CreateCodeSessionResponse>> CreateCodeSession(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string endpointProvider,
            string language,
            CodeSessionEndpointProviderOverride? endpointProviderOverride = null);

        /// <summary>
        /// Checks whether the specified knowledge unit name is available within the given instance.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">The knowledge unit name to validate. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="ResourceNameCheckResult"/> indicating whether the knowledge unit name is available and, if not, the
        /// reason for unavailability.</returns>
        Task<Result<ResourceNameCheckResult>> CheckKnowledgeUnitName(
            string instanceId,
            ResourceName resourceName);

        /// <summary>
        /// Checks whether the specified vector store identifier is available within the given instance.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="checkVectorStoreIdRequest">The vector store identifier validation request. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="ResourceNameCheckResult"/> indicating whether the vector store identifier is available and, if not, the
        /// reason for unavailability.</returns>
        Task<Result<ResourceNameCheckResult>> CheckVectorStoreId(
            string instanceId,
            CheckVectorStoreIdRequest checkVectorStoreIdRequest);

        /// <summary>
        /// Checks whether the specified knowledge source name is available within the given instance.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">The knowledge source name to validate. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="ResourceNameCheckResult"/> indicating whether the knowledge source name is available and, if not, the
        /// reason for unavailability.</returns>
        Task<Result<ResourceNameCheckResult>> CheckKnowledgeSourceName(
            string instanceId,
            ResourceName resourceName);

        /// <summary>
        /// Call the Context API to retrieve the a knowledge unit.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <param name="agentName">The agent name if the request is being made on behalf of an agent.</param>
        /// <param name="options">The loading options for the knowledge unit.</param>
        /// <returns>The requested knowledge unit.</returns>
        Task<Result<ResourceProviderGetResult<KnowledgeUnit>>> GetKnowledgeUnit(
            string instanceId,
            string knowledgeUnitId,
            string? agentName = null,
            ResourceProviderGetOptions? options = null);

        /// <summary>
        /// Call the Context API to retrieve a knowledge source.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The knowledge source identifier.</param>
        /// <param name="agentName">The agent name if the request is being made on behalf of an agent.</param>
        /// <param name="options">The loading options for the knowledge source.</param>
        /// <returns>The requested knowledge source.</returns>
        Task<Result<ResourceProviderGetResult<KnowledgeSource>>> GetKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            string? agentName = null,
            ResourceProviderGetOptions? options = null);

        /// <summary>
        /// Call the Context API to retrieve the list of knowledge units.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitNames">An optional list of specific knowledge units to retrieve.</param>
        /// <param name="options">The loading options for the knowledge units.</param>
        /// <returns>The list of knowledge units.</returns>
        Task<Result<IEnumerable<ResourceProviderGetResult<KnowledgeUnit>>>> GetKnowledgeUnits(
            string instanceId,
            IEnumerable<string>? knowledgeUnitNames = null,
            ResourceProviderGetOptions? options = null);

        /// <summary>
        /// Call the Context API to retrieve the list of knowledge sources.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceNames">An optional list of specific knowledge sources to retrieve.</param>
        /// <param name="options">The loading options for the knowledge sources.</param>
        /// <returns>The list of knowledge sources.</returns>
        Task<Result<IEnumerable<ResourceProviderGetResult<KnowledgeSource>>>> GetKnowledgeSources(
            string instanceId,
            IEnumerable<string>? knowledgeSourceNames = null,
            ResourceProviderGetOptions? options = null);

        /// <summary>
        /// Creates or updates a knowledge unit in the context service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnit">The knowledge unit resource to be created or updated.</param>
        /// <returns></returns>
        Task<Result<ResourceProviderUpsertResult<KnowledgeUnit>>> UpsertKnowledgeUnit(
            string instanceId,
            KnowledgeUnit knowledgeUnit);

        /// <summary>
        /// Creates or updates a knowledge source in the context service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSource">The knowledge source resource to be created or updated.</param>
        /// <returns></returns>
        Task<Result<ResourceProviderUpsertResult<KnowledgeSource>>> UpsertKnowledgeSource(
            string instanceId,
            KnowledgeSource knowledgeSource);

        /// <summary>
        /// Deletes the specified knowledge unit from the given instance asynchronously.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The unique identifier of the knowledge unit to delete. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous delete operation. The task result contains a <see cref="Result"/>
        /// indicating whether the deletion was successful.</returns>
        Task<Result> DeleteKnowledgeUnit(
            string instanceId,
            string knowledgeUnitId);

        /// <summary>
        /// Deletes the specified knowledge source from the given instance asynchronously.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The unique identifier of the knowledge source to delete. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous delete operation. The task result contains a <see cref="Result"/>
        /// indicating whether the deletion was successful.</returns>
        Task<Result> DeleteKnowledgeSource(
            string instanceId,
            string knowledgeSourceId);

        /// <summary>
        /// Sets the knowledge graph for a knowledge unit.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <param name="setGraphRequest">The request containing the knowledge graph details.</param>
        /// <returns>A response indicating the success of the operation and an optional error message.</returns>
        Task<Result<ResourceProviderActionResult>> SetKnowledgeUnitGraph(
            string instanceId,
            string knowledgeUnitId,
            ContextKnowledgeUnitSetGraphRequest setGraphRequest);

        /// <summary>
        /// Calls the Context API to query a knowledge source.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The knowledge source identifier.</param>
        /// <param name="queryRequest">The request object containing query parameters and options.</param>
        /// <returns>A response containing the result of the query execution.</returns>
        Task<Result<ContextKnowledgeSourceQueryResponse>> QueryKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceQueryRequest queryRequest);

        /// <summary>
        /// Retrieves the knowledge unit's knowledge graph in a format suitable for visualization or further processing.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <param name="queryRequest">The request containing the details of the query.</param>
        /// <returns></returns>
        Task<Result<ContextKnowledgeUnitRenderGraphResponse>> RenderKnowledgeUnitGraph(
            string instanceId,
            string knowledgeUnitId,
            ContextKnowledgeSourceQueryRequest? queryRequest);
    }
}
