using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Context.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
            var knowledgeSources = await _knowledgeService.GetKnowledgeUnits(
                instanceId,
                listRequest,
                _callContext.CurrentUserIdentity!);
            return Ok(knowledgeSources);
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
            var knowledgeSources = await _knowledgeService.GetKnowledgeSources(
                instanceId,
                listRequest,
                _callContext.CurrentUserIdentity!);
            return Ok(knowledgeSources);
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
            var response = await _knowledgeService.UpsertKnowledgeUnit(
                instanceId,
                knowledgeUnit,
                _callContext.CurrentUserIdentity!);
            return Ok(response);
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
            var response = await _knowledgeService.UpsertKnowledgeSource(
                instanceId,
                knowledgeSource,
                _callContext.CurrentUserIdentity!);
            return Ok(response);
        }

        /// <summary>
        /// Updates a knowledge source.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId">The knowledge source identifier.</param>
        /// <param name="updateRequest"> The request containing the information used to update the knowledge source.</param>
        /// <returns></returns>
        [HttpPost("knowledgeSources/{knowledgeSourceId}")]
        public async Task<IActionResult> UpdateKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            [FromBody] ContextKnowledgeSourceUpdateRequest updateRequest)
        {
            await _knowledgeService.UpdateKnowledgeSource(
                instanceId,
                knowledgeSourceId,
                updateRequest,
                _callContext.CurrentUserIdentity!);

            return Ok();
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
            var response = await _knowledgeService.QueryKnowledgeSource(
                instanceId,
                knowledgeSourceId,
                queryRequest,
                _callContext.CurrentUserIdentity!);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves the knowledge graph in a format suitable for rendering.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeSourceId"></param>
        /// <param name="queryRequest"></param>
        /// <returns></returns>
        [HttpPost("knowledgeSources/{knowledgeSourceId}/render-graph")]
        public async Task<IActionResult> RenderKnowledgeSourceGraph(
            string instanceId,
            string knowledgeSourceId,
            [FromBody] ContextKnowledgeSourceQueryRequest? queryRequest)
        {
            var response = await _knowledgeService.RenderKnowledgeSourceGraph(
                instanceId,
                knowledgeSourceId,
                queryRequest,
                _callContext.CurrentUserIdentity!);
            return Ok(response);
        }
    }
}
