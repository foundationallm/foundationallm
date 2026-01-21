using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Context.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;

namespace FoundationaLLM.Context.API.Controllers
{
    /// <summary>
    /// Provdes methods for managing files.
    /// </summary>
    /// <param name="knowledgeService">The <see cref="IKnowledgeService"/> knowledge service.</param>
    /// <param name="callContext">>The <see cref="IOrchestrationContext"/> call context associated with the current request.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}")]
    public class KnowledgeController(
        IKnowledgeService knowledgeService,
        IOrchestrationContext callContext,
        ILogger<KnowledgeController> logger) : ControllerBase
    {
        private readonly IKnowledgeService _knowledgeService = knowledgeService;
        private readonly IOrchestrationContext _callContext = callContext;
        private readonly ILogger<KnowledgeController> _logger = logger;

        /// <summary>
        /// Checks whether a knowledge unit name is available within the specified instance.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">An object containing the name of the knowledge unit to check. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// indicating whether the knowledge unit name is available.</returns>
        [HttpPost("knowledgeUnits/check-name")]
        public async Task<IActionResult> CheckKnowledgeUnitName(
            string instanceId,
            [FromBody] ResourceName resourceName)
        {
            var result = await _knowledgeService.CheckKnowledgeUnitName(
                instanceId,
                resourceName,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Checks whether a vector store identifier is available within the specified vector database.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="checkVectorStoreIdRequest">An object containing the name of the vector database and
        /// the identifier of the vector store to check. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// indicating whether the vector store identifier is available.</returns>
        [HttpPost("knowledgeUnits/check-vectorstore-id")]
        public async Task<IActionResult> CheckVectorStoreId(
            string instanceId,
            [FromBody] CheckVectorStoreIdRequest checkVectorStoreIdRequest)
        {
            var result = await _knowledgeService.CheckVectorStoreId(
                instanceId,
                checkVectorStoreIdRequest,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Checks whether a knowledge source name is available within the specified instance.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">An object containing the name of the knowledge source to check. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// indicating whether the knowledge source name is available.</returns>
        [HttpPost("knowledgeSources/check-name")]
        public async Task<IActionResult> CheckKnowledgeSourceName(
            string instanceId,
            [FromBody] ResourceName resourceName)
        {
            var result = await _knowledgeService.CheckKnowledgeSourceName(
                instanceId,
                resourceName,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Retrieves a specified knowledge unit.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <param name="agentName">The agent name if the request is being made on behalf of an agent.</param>
        /// <param name="queryParams">The query parameters.</param>
        /// <returns></returns>
        [HttpGet("knowledgeUnits/{knowledgeUnitId}")]
        public async Task<IActionResult> GetKnowledgeUnit(
            string instanceId,
            string knowledgeUnitId,
            string? agentName,
            [FromQuery] Dictionary<string, string> queryParams)
        {
            var result = await _knowledgeService.GetKnowledgeUnit(
                instanceId,
                knowledgeUnitId,
                agentName,
                ResourceProviderGetOptions.FromQueryParams(queryParams, false, false),
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Retrieves a specified knowledge source.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The knowledge source identifier.</param>
        /// <param name="agentName">The agent name if the request is being made on behalf of an agent.</param>
        /// <param name="queryParams">The query parameters.</param>
        /// <returns></returns>
        [HttpGet("knowledgeSources/{knowledgeSourceId}")]
        public async Task<IActionResult> GetKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            string? agentName,
            [FromQuery] Dictionary<string, string> queryParams)
        {
            var result = await _knowledgeService.GetKnowledgeSource(
                instanceId,
                knowledgeSourceId,
                agentName,
                ResourceProviderGetOptions.FromQueryParams(queryParams, false, false),
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Retrieves the list of knowledge units.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="listRequest"> The request containing the information used to filter the knowledge resources.</param>
        /// <param name="queryParams">The query parameters.</param>
        /// <returns></returns>
        [HttpPost("knowledgeUnits/list")]
        public async Task<IActionResult> GetKnowledgeUnits(
            string instanceId,
            [FromBody] ContextKnowledgeResourceListRequest listRequest,
            [FromQuery] Dictionary<string, string> queryParams)
        {
            var result = await _knowledgeService.GetKnowledgeUnits(
                instanceId,
                listRequest,
                ResourceProviderGetOptions.FromQueryParams(queryParams, false, false),
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Retrieves the list of knowledge sources.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="listRequest"> The request containing the information used to filter the knowledge resources.</param>
        /// <param name="queryParams">The query parameters.</param>
        /// <returns></returns>
        [HttpPost("knowledgeSources/list")]
        public async Task<IActionResult> GetKnowledgeSources(
            string instanceId,
            [FromBody] ContextKnowledgeResourceListRequest listRequest,
            [FromQuery] Dictionary<string, string> queryParams)
        {
            var result = await _knowledgeService.GetKnowledgeSources(
                instanceId,
                listRequest,
                ResourceProviderGetOptions.FromQueryParams(queryParams, false, false),
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Creates or updates a knowledge unit.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnit">The knowledge unit to be created or updated.</param>
        /// <returns></returns>
        [HttpPost("knowledgeUnits")]
        public async Task<IActionResult> UpsertKnowledgeUnit(
            string instanceId,
            [FromBody] KnowledgeUnit knowledgeUnit)
        {
            var result = await _knowledgeService.UpsertKnowledgeUnit(
                instanceId,
                knowledgeUnit,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Creates or updates a knowledge source.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSource">The knowledge source to be created or updated.</param>
        /// <returns></returns>
        [HttpPost("knowledgeSources")]
        public async Task<IActionResult> UpsertKnowledgeSource(
            string instanceId,
            [FromBody] KnowledgeSource knowledgeSource)
        {
            var result = await _knowledgeService.UpsertKnowledgeSource(
                instanceId,
                knowledgeSource,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Deletes a specified knowledge unit.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <returns></returns>
        [HttpDelete("knowledgeUnits/{knowledgeUnitId}")]
        public async Task<IActionResult> DeleteKnowledgeUnit(
            string instanceId,
            string knowledgeUnitId)
        {
            var result = await _knowledgeService.DeleteKnowledgeUnit(
                instanceId,
                knowledgeUnitId,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Deletes a specified knowledge source.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The knowledge source identifier.</param>
        /// <returns></returns>
        [HttpDelete("knowledgeSources/{knowledgeSourceId}")]
        public async Task<IActionResult> DeleteKnowledgeSource(
            string instanceId,
            string knowledgeSourceId)
        {
            var result = await _knowledgeService.DeleteKnowledgeSource(
                instanceId,
                knowledgeSourceId,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Sets the knowledge graph for a knowledge unit.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <param name="setGraphRequest"> The request containing the information used to set the knowledge graph.</param>
        /// <returns></returns>
        [HttpPost("knowledgeUnits/{knowledgeUnitId}/set-graph")]
        public async Task<IActionResult> SetKnowledgeUnitGraph(
            string instanceId,
            string knowledgeUnitId,
            [FromBody] ContextKnowledgeUnitSetGraphRequest setGraphRequest)
        {
            var result = await _knowledgeService.SetKnowledgeUnitGraph(
                instanceId,
                knowledgeUnitId,
                setGraphRequest,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Retrieves the knowledge graph in a format suitable for rendering.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId"></param>
        /// <param name="queryRequest"></param>
        /// <returns></returns>
        [HttpPost("knowledgeUnits/{knowledgeUnitId}/render-graph")]
        public async Task<IActionResult> RenderKnowledgeUnitGraph(
            string instanceId,
            string knowledgeUnitId,
            [FromBody] ContextKnowledgeSourceQueryRequest? queryRequest)
        {
            var result = await _knowledgeService.RenderKnowledgeUnitGraph(
                instanceId,
                knowledgeUnitId,
                queryRequest,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Queries a knowledge graph.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId"></param>
        /// <param name="queryRequest"></param>
        /// <returns></returns>
        [HttpPost("knowledgeSources/{knowledgeSourceId}/query")]
        public async Task<IActionResult> QueryKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            [FromBody] ContextKnowledgeSourceQueryRequest queryRequest)
        {
            var result = await _knowledgeService.QueryKnowledgeSource(
                instanceId,
                knowledgeSourceId,
                queryRequest,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }
    }
}
