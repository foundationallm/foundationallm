using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Context.Knowledge;
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
        /// Retrieves a specified knowledge unit.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeUnitId">The knowledge unit identifier.</param>
        /// <param name="agentName">The agent name if the request is being made on behalf of an agent.</param>
        /// <returns></returns>
        [HttpGet("knowledgeUnits/{knowledgeUnitId}")]
        public async Task<IActionResult> GetKnowledgeUnit(
            string instanceId,
            string knowledgeUnitId,
            string? agentName)
        {
            var result = await _knowledgeService.GetKnowledgeUnit(
                instanceId,
                knowledgeUnitId,
                agentName,
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
        /// <returns></returns>
        [HttpGet("knowledgeSources/{knowledgeSourceId}")]
        public async Task<IActionResult> GetKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            string? agentName)
        {
            var result = await _knowledgeService.GetKnowledgeSource(
                instanceId,
                knowledgeSourceId,
                agentName,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Retrieves the list of knowledge units.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="listRequest"> The request containing the information used to filter the knowledge resources.</param>
        /// <returns></returns>
        [HttpPost("knowledgeUnits/list")]
        public async Task<IActionResult> GetKnowledgeUnits(
            string instanceId,
            [FromBody] ContextKnowledgeResourceListRequest listRequest)
        {
            var result = await _knowledgeService.GetKnowledgeUnits(
                instanceId,
                listRequest,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Retrieves the list of knowledge sources.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="listRequest"> The request containing the information used to filter the knowledge resources.</param>
        /// <returns></returns>
        [HttpPost("knowledgeSources/list")]
        public async Task<IActionResult> GetKnowledgeSources(
            string instanceId,
            [FromBody] ContextKnowledgeResourceListRequest listRequest)
        {
            var result = await _knowledgeService.GetKnowledgeSources(
                instanceId,
                listRequest,
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
