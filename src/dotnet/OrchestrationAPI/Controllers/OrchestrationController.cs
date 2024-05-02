using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Orchestration.API.Controllers
{
    /// <summary>
    /// OrchestrationController class
    /// </summary>
    /// <remarks>
    /// Constructor for the Orchestration orchestration controller
    /// </remarks>
    /// <param name="orchestrationService"></param>
    /// <param name="logger"></param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class OrchestrationController(
        IOrchestrationService orchestrationService,
        ILogger<OrchestrationController> logger) : ControllerBase
    {
        private readonly IOrchestrationService _orchestrationService = orchestrationService;
        private readonly ILogger<OrchestrationController> _logger = logger;

        /// <summary>
        /// Retrieves a completion from an orchestration service
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        [HttpPost("completion")]
        public async Task<CompletionResponse> GetCompletion([FromBody] CompletionRequest completionRequest) =>
            await _orchestrationService.GetCompletion(completionRequest);
    }
}
