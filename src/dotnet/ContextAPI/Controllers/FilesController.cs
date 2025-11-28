using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Context;
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
        /// <param name="agentName">The name of the agent.</param>
        /// <returns></returns>
        [HttpPost("conversations/{conversationId}/files")]
        public async Task<IActionResult> UploadFileForConversation(
            string instanceId,
            string conversationId,
            [FromQuery] string? agentName)
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

            var result = await _fileService.CreateFileForConversation(
                instanceId,
                ContextRecordOrigins.UserUpload,
                agentName,
                conversationId,
                fileName,
                contentType,
                memoryStream,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Uploads a file to an agent.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <returns></returns>
        [HttpPost("agents/{agentName}/files")]
        public async Task<IActionResult> UploadFileForAgent(
            string instanceId,
            string agentName)
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

            var result = await _fileService.CreateFileForAgent(
                instanceId,
                ContextRecordOrigins.UserUpload,
                agentName,
                fileName,
                contentType,
                memoryStream,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }


        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The identifier of the file to be downloaded.</param>
        /// <returns></returns>
        [HttpGet("files/{fileId}")]
        public async Task<IActionResult> DownloadFile(
            string instanceId,
            string fileId)
        {
            var result = await _fileService.GetFileContent(
                instanceId,
                fileId,
                _callContext.CurrentUserIdentity!);

            return result.TryGetValue(out var fileContent)
                ? File(
                    fileContent!.FileContent!,
                    fileContent!.ContentType!,
                    fileContent!.FileName!)
                : result.ToActionResult();
        }

        /// <summary>
        /// Retrieves a file record.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The identifier of the file to be retrieved.</param>
        /// <returns></returns>
        [HttpGet("fileRecords/{fileId}")]
        public async Task<IActionResult> GetFileRecord(
            string instanceId,
            string fileId)
        {
            var result = await _fileService.GetFileRecord(
                instanceId,
                fileId,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }

        /// <summary>
        /// Deletes a file record.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The identifier of the file to be deleted.</param>
        /// <returns></returns>
        [HttpDelete("fileRecords/{fileId}")]
        public async Task<IActionResult> DeleteFileRecord(
            string instanceId,
            string fileId)
        {
            var result = await _fileService.DeleteFileRecord(
                instanceId,
                fileId,
                _callContext.CurrentUserIdentity!);

            return
                result.ToActionResult();
        }
    }
}
