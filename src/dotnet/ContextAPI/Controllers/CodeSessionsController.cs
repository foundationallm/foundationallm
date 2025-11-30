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
        ILogger<CodeSessionsController> logger) : ControllerBase
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
            var result = await _codeSessionService.CreateCodeSession(
                instanceId,
                request,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
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
            var result = await _codeSessionService.UploadFilesToCodeSession(
                instanceId,
                sessionId,
                request,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Downloads files from a code session.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The identifier of the code session from where the files must be downloaded.</param>
        /// <param name="request">The <see cref="CodeSessionFileDownloadRequest"/> providing the details of the request.</param>
        /// <returns></returns>
        [HttpPost("{sessionId}/downloadFiles")]
        public async Task<IActionResult> DownloadFilesFromCodeSession(
            string instanceId,
            string sessionId,
            [FromBody] CodeSessionFileDownloadRequest request)
        {
            var result = await _codeSessionService.DownloadFilesFromCodeSession(
                instanceId,
                sessionId,
                request.OperationId,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Executes code in a code session and returns the result of the execution.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The identifier of the code session where the code must be executed.</param>
        /// <param name="codeExecutionRequest">The code execution request.</param>
        /// <returns>The result of the code execution including standard and error outputs.</returns>
        [HttpPost("{sessionId}/executeCode")]
        public async Task<IActionResult> ExecuteCodeInCodeSession(
            string instanceId,
            string sessionId,
            [FromBody] CodeSessionCodeExecuteRequest codeExecutionRequest)
        {
            var result = await _codeSessionService.ExecuteCodeInCodeSession(
                instanceId,
                sessionId,
                codeExecutionRequest.CodeToExecute,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }
    }
}
