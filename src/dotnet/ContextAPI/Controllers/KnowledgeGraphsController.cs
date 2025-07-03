using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Context.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Context.API.Controllers
{
    /// <summary>
    /// Provdes methods for managing files.
    /// </summary>
    /// <param name="knowledgeGraphService">The <see cref="IKnowledgeGraphService"/> knowledge graph service.</param>
    /// <param name="callContext">>The <see cref="IOrchestrationContext"/> call context associated with the current request.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}")]
    public class KnowledgeGraphsController(
        IKnowledgeGraphService knowledgeGraphService,
        IOrchestrationContext callContext,
        ILogger<KnowledgeGraphsController> logger) : ControllerBase
    {
        private readonly IKnowledgeGraphService _knowledgeGraphService = knowledgeGraphService;
        private readonly IOrchestrationContext _callContext = callContext;
        private readonly ILogger<KnowledgeGraphsController> _logger = logger;

        /// <summary>
        /// Updates a knowledge graph.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeGraphId">The knowledge graph identifier.</param>
        /// <param name="updateRequest"> The request containing the paths to the source files for the knowledge graph.</param>
        /// <returns></returns>
        [HttpPost("knowledgegraphs/{knowledgeGraphId}")]
        public async Task<IActionResult> UpdateKnowledgeGraph(
            string instanceId,
            string knowledgeGraphId,
            [FromBody] ContextKnowledgeGraphUpdateRequest updateRequest)
        {
            await _knowledgeGraphService.UpdateKnowledgeGraph(
                instanceId,
                knowledgeGraphId,
                updateRequest,
                _callContext.CurrentUserIdentity!);

            return Ok();
        }

        /// <summary>
        /// Queries a knowledge graph.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeGraphId"></param>
        /// <param name="queryRequest"></param>
        /// <returns></returns>
        [HttpPost("knowledgegraphs/{knowledgeGraphId}/query")]
        public async Task<IActionResult> QueryKnowledgeGraph(
            string instanceId,
            string knowledgeGraphId,
            [FromBody] ContextKnowledgeGraphQueryRequest queryRequest)
        {
            var response = await _knowledgeGraphService.QueryKnowledgeGraph(
                instanceId,
                knowledgeGraphId,
                queryRequest,
                _callContext.CurrentUserIdentity!);
            return Ok(response);
        }
    }
}
