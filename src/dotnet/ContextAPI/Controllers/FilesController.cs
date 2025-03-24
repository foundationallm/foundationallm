using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Context.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Context.API.Controllers
{
    /// <summary>
    /// Provdes methods for managing files.
    /// </summary>
    /// <param name="fileService">The <see cref="IFileService"/> file service.</param>
    /// <param name="callContext">>The <see cref="IOrchestrationContext"/> call context associated with the current request.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}")]
    public class FilesController(
        IFileService fileService,
        IOrchestrationContext callContext,
        ILogger<FilesController> logger): ControllerBase
    {
        private readonly IFileService _fileService = fileService;
        private readonly IOrchestrationContext _callContext = callContext;
        private readonly ILogger<FilesController> _logger = logger;

        /// <summary>
        /// Uploads a file to a conversation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <returns></returns>
        [HttpPost("conversations/{conversationId}/files")]
        public async Task<IActionResult> UploadFile(
            string instanceId,
            string conversationId)
        {
            var formFiles = HttpContext.Request.HasFormContentType ? HttpContext.Request.Form?.Files : null;
            IFormFile? formFile = (formFiles != null && formFiles.Count > 0) ? formFiles[0] : null;

            if (formFile == null || formFile.Length == 0)
                return BadRequest("File not selected.");

            var fileName = formFile.FileName;
            var contentType = formFile.ContentType;

            await using var stream = formFile.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            var fileRecord = await _fileService.CreateFile(
                instanceId,
                conversationId,
                fileName,
                contentType,
                memoryStream,
                _callContext.CurrentUserIdentity!);

            return new OkObjectResult(fileRecord);
        }
    }
}
