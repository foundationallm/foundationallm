using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Context.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Context.API.Controllers
{
    /// <summary>
    /// Provdes methods for managing code sessions.
    /// </summary>
    /// <param name="codeSessionService">The <see cref="ICodeSessionService"/> providing code sessions services.</param>
    /// <param name="callContext">>The <see cref="IOrchestrationContext"/> call context associated with the current request.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}/[controller]")]
    public class CodeSessionsController(
        ICodeSessionService codeSessionService,
        IOrchestrationContext callContext,
        ILogger<CodeSessionsController> logger): ControllerBase
    {
        private readonly ICodeSessionService _codeSessionService = codeSessionService;
        private readonly IOrchestrationContext _callContext = callContext;
        private readonly ILogger<CodeSessionsController> _logger = logger;

        /// <summary>
        /// Creates a new code session.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="request">The <see cref="CreateCodeSessionRequest"/> providing the details of the request.</param>
        /// <returns>An <see cref="OkObjectResult"/> containing the <see cref="CreateCodeSessionResponse"/> response.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCodeSessions(
            string instanceId,
            [FromBody] CreateCodeSessionRequest request)
        {
            var response = await _codeSessionService.CreateCodeSession(
                instanceId,
                request,
                _callContext.CurrentUserIdentity!);

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Uploads files to a code session.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The identifier of the code session where the files must be uploaded.</param>
        /// <param name="request">The <see cref="CodeSessionFileUploadRequest"/> providing the details of the request.</param>
        /// <returns></returns>
        [HttpPost("{sessionId}/uploadFiles")]
        public async Task<IActionResult> UploadFilesToCodeSession(
            string instanceId,
            string sessionId,
            [FromBody] CodeSessionFileUploadRequest request)
        {
            var response = await _codeSessionService.UploadFilesToCodeSession(
                instanceId,
                sessionId,
                request,
                _callContext.CurrentUserIdentity!);

            return new OkObjectResult(response);
        }
    }
}
