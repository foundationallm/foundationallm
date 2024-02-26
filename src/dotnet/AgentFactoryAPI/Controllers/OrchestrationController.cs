using Asp.Versioning;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.AgentFactory.API.Controllers
{
    /// <summary>
    /// OrchestrationController class
    /// </summary>
    /// <remarks>
    /// Constructor for the Agent Factory orchestration controller
    /// </remarks>
    /// <param name="agentFactoryService"></param>
    /// <param name="logger"></param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class OrchestrationController(
        IAgentFactoryService agentFactoryService,
        ILogger<OrchestrationController> logger) : ControllerBase
    {
        private readonly IAgentFactoryService _agentFactoryService = agentFactoryService;
        private readonly ILogger<OrchestrationController> _logger = logger;

        /// <summary>
        /// Retrieves a completion from an orchestration service
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        [HttpPost("completion")]
        public async Task<CompletionResponse> GetCompletion([FromBody] CompletionRequest completionRequest) =>
            await _agentFactoryService.GetCompletion(completionRequest);
    }
}
