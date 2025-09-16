using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Context.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Context.API.Controllers
{
    /// <summary>
    /// Provdes methods for managing files.
    /// </summary>
    /// <param name="mcpClientService">The <see cref="IMCPClientService"/> MCP client service.</param>
    /// <param name="callContext">>The <see cref="IOrchestrationContext"/> call context associated with the current request.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}/mcp")]
    public class MCPController(
        IMCPClientService mcpClientService,
        IOrchestrationContext callContext,
        ILogger<KnowledgeController> logger) : ControllerBase
    {
        private readonly IMCPClientService _mcpClientService = mcpClientService;
        private readonly IOrchestrationContext _callContext = callContext;
        private readonly ILogger<KnowledgeController> _logger = logger;

        [HttpGet("test")]
        public async Task<IActionResult> Test(
           string instanceId)
        {
            var result = await _mcpClientService.Test();
            return Ok(result);
        }
    }
}
