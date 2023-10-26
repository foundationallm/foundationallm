using Asp.Versioning;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Configuration.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Methods for orchestration services exposed by the Gatekeeper API service.
    /// </summary>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IGatekeeperAPIService _gatekeeperAPIService;
        private readonly ILogger<OrchestrationController> _logger;

        /// <summary>
        /// Constructor for the Orchestration Controller.
        /// </summary>
        /// <param name="gatekeeperAPIService">Provides methods for calling the Gatekeeper API.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="OrchestrationController"/> type name.</param>
        public OrchestrationController(IGatekeeperAPIService gatekeeperAPIService,
            ILogger<OrchestrationController> logger)
        {
            _gatekeeperAPIService = gatekeeperAPIService;
            _logger = logger;
        }

        /// <summary>
        /// Requests a completion from the downstream APIs via the Gatekeeper API.
        /// </summary>
        /// <param name="completionRequest">The completion request containing the user
        /// prompt and message history.</param>
        [AllowAnonymous]
        [HttpPost("completion", Name = "GetCompletion")]
        public async Task<IActionResult> GetCompletion(CompletionRequest completionRequest)
        {
            var completionResponse = await _gatekeeperAPIService.GetCompletion(completionRequest);

            return Ok(completionResponse);
        }

        /// <summary>
        /// Requests a summary from the downstream APIs via the Gatekeeper API.
        /// </summary>
        /// <param name="summaryRequest">The summary request containing the user
        /// prompt.</param>
        [AllowAnonymous]
        [HttpPost("summary", Name = "GetSummary")]
        public async Task<IActionResult> GetSummary(SummaryRequest summaryRequest)
        {
            var summaryResponse = await _gatekeeperAPIService.GetSummary(summaryRequest.UserPrompt);

            return Ok(summaryResponse);
        }
    }
}
