using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Telemetry;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Telemetry;
using FoundationaLLM.Common.Utils;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for retrieving and managing files.
    /// </summary>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [Authorize(
        AuthenticationSchemes = AgentAccessTokenDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.FoundationaLLMAgentAccessToken)]
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IOrchestrationContext _callContext;
        private readonly InstanceSettings _instanceSettings;
        private readonly ICoreService _coreService;
        private readonly ILogger<FilesController> _logger;

        /// <summary>
        /// The controller for managing files.
        /// </summary>
        /// <param name="callContext">The <see cref="IOrchestrationContext"/> call context of the request being handled.</param>
        /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
        /// <param name="coreService">The <see cref="ICoreService"/> core service.</param>
        /// <param name="logger"></param>
        /// <exception cref="ResourceProviderException"></exception>
        public FilesController(
            IOrchestrationContext callContext,
            IOptions<InstanceSettings> instanceOptions,
            ICoreService coreService,
            ILogger<FilesController> logger)
        {
            _callContext = callContext;
            _instanceSettings = instanceOptions.Value;
            _coreService = coreService;
            _logger = logger;
        }

        /// <summary>
        /// Uploads an attachment.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="sessionId">The session ID from which the file is uploaded.</param>
        /// <param name="agentName">The agent name.</param>
        /// <param name="file">The file sent with the HTTP request.</param>
        /// <returns></returns>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(string instanceId, string sessionId, string agentName, IFormFile file)
        {
            using var telemetryActivity = TelemetryActivitySources.CoreAPIActivitySource.StartActivity(
                TelemetryActivityNames.CoreAPI_Files_Upload,
                ActivityKind.Server,
                parentContext: default,
                tags: new Dictionary<string, object?>
                    {
                    { TelemetryActivityTagNames.InstanceId, instanceId },
                        { TelemetryActivityTagNames.AgentName, agentName ?? "N/A" },
                        { TelemetryActivityTagNames.ConversationId, sessionId ?? "N/A" },
                        { TelemetryActivityTagNames.UPN, _callContext.CurrentUserIdentity?.UPN ?? "N/A" },
                        { TelemetryActivityTagNames.UserId, _callContext.CurrentUserIdentity?.UserId ?? "N/A" }
                    });

            if (file == null || file.Length == 0)
                return BadRequest("File not selected.");

            var fileName = file.FileName;
            var name = $"a-{Guid.NewGuid()}-{DateTime.UtcNow.Ticks}";
            var contentType = file.ContentType;

            await using var stream = file.OpenReadStream();
            var content = await BinaryData.FromStreamAsync(stream);
            var contentTypeResult = FileUtils.GetFileContentType(fileName, content);

            if (!contentTypeResult.IsSupported
                || !contentTypeResult.MatchesExtension)
            {
                _logger.LogError(
                    "File upload failed due to unsupported file type or file type mismatch. FileName: {FileName}, DetectedContentType: {DetectedContentType}, IsSupported: {IsSupported}, MatchesExtension: {MatchesExtension}",
                    fileName,
                    contentTypeResult.ContentType,
                    contentTypeResult.IsSupported,
                    contentTypeResult.MatchesExtension);
                return UnprocessableEntity(
                    "The file type is not supported or does not match file extension.");
            }
            if (contentType != contentTypeResult.ContentType)
            {
                _logger.LogWarning(
                    "File content type mismatch detected (the detected content type will be used). FileName: {FileName}, OriginalContentType: {OriginalContentType}, DetectedContentType: {DetectedContentType}",
                    fileName,
                    contentType,
                    contentTypeResult.ContentType);
                contentType = contentTypeResult.ContentType;
            }

            var uploadResult = await _coreService.UploadAttachment(
                instanceId,
                sessionId,
                new AttachmentFile
                {
                    Name = name,
                    Content = content,
                    DisplayName = fileName,
                    ContentType = contentType,
                    OriginalFileName = fileName
                },
                agentName);

            if (uploadResult.Resource != null)
            {
                uploadResult.Resource.Content = null;
            }

            return new OkObjectResult(uploadResult);
        }

        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="fileProvider">The name of the file provider.</param>
        /// <param name="fileId">The identifier of the file.</param>
        /// <returns>The file content.</returns>
        /// <remarks>
        /// The following file providers are supported:
        /// <list type="bullet">
        /// <item>FoundationaLLM.Attachments</item>
        /// <item>FoundationaLLM.AzureOpenAI</item>
        /// </list>
        /// </remarks>
        [HttpGet("{fileProvider}/{fileId}")]
        public async Task<IActionResult> Download(string instanceId, string fileProvider, string fileId)
        {
            using var telemetryActivity = TelemetryActivitySources.CoreAPIActivitySource.StartActivity(
               TelemetryActivityNames.CoreAPI_Files_Download,
               ActivityKind.Server,
               parentContext: default,
                tags: new Dictionary<string, object?>
                   {
                    { TelemetryActivityTagNames.InstanceId, instanceId },
                        { TelemetryActivityTagNames.FileProvider, fileProvider ?? "N/A" },
                        { TelemetryActivityTagNames.FileId, fileId ?? "N/A" },
                        { TelemetryActivityTagNames.UPN, _callContext.CurrentUserIdentity?.UPN ?? "N/A" },
                        { TelemetryActivityTagNames.UserId, _callContext.CurrentUserIdentity?.UserId ?? "N/A" }
                   });

            var attachment = await _coreService.DownloadAttachment(instanceId, fileProvider, fileId);

            return attachment == null
                ? NotFound()
                : File(
                    attachment.Content!.ToStream(),
                    attachment.ContentType!,
                    attachment.OriginalFileName);
        }

        /// <summary>
        /// Deletes the specified file(s).
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="resourcePaths">The list of object identifiers to be deleted.</param>
        /// <returns></returns>
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(string instanceId, [FromBody] List<string> resourcePaths) =>
            new OkObjectResult(await _coreService.DeleteAttachments(instanceId, resourcePaths));
    }
}
