using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ContentSafety;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Context.API.Controllers
{
    /// <summary>
    /// Provides methods for content safety operations including prompt injection detection.
    /// </summary>
    /// <param name="contentSafetyService">The <see cref="IContentSafetyService"/> content safety service.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}")]
    public class ContentSafetyController(
        IContentSafetyService contentSafetyService,
        ILogger<ContentSafetyController> logger) : ControllerBase
    {
        private readonly IContentSafetyService _contentSafetyService = contentSafetyService;
        private readonly ILogger<ContentSafetyController> _logger = logger;

        /// <summary>
        /// Scans content for prompt injection attacks using Azure AI Content Safety.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="request">The content shield request containing the content to analyze.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ContentShieldResponse"/> containing the analysis results.</returns>
        /// <remarks>
        /// This endpoint supports two modes of operation:
        /// <list type="bullet">
        /// <item><description>Single content scanning: Provide the <c>content</c> field for analyzing a single text.</description></item>
        /// <item><description>Batch document scanning: Provide the <c>documents</c> field for analyzing multiple documents.</description></item>
        /// </list>
        /// </remarks>
        [HttpPost("contentSafety/shield")]
        public async Task<IActionResult> ShieldContent(
            string instanceId,
            [FromBody] ContentShieldRequest request,
            CancellationToken cancellationToken)
        {
            // Validate request - must have either content or documents
            if (string.IsNullOrWhiteSpace(request.Content) && (request.Documents == null || request.Documents.Count == 0))
            {
                return BadRequest(new ContentShieldResponse
                {
                    Success = false,
                    SafeContent = false,
                    PromptInjectionDetected = false,
                    Details = "Request must contain either 'content' for single text scanning or 'documents' for batch document scanning."
                });
            }

            try
            {
                // Batch document scanning mode
                if (request.Documents != null && request.Documents.Count > 0)
                {
                    _logger.LogInformation(
                        "Processing batch content shield request for instance {InstanceId} with {DocumentCount} documents.",
                        instanceId,
                        request.Documents.Count);

                    var context = request.Context ?? "Content shield analysis";
                    var documentResult = await _contentSafetyService.DetectPromptInjection(
                        context,
                        request.Documents,
                        cancellationToken);

                    var unsafeDocumentIds = documentResult.DocumentResults
                        .Where(kvp => !kvp.Value.SafeContent)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    var response = new ContentShieldResponse
                    {
                        Success = documentResult.Success,
                        SafeContent = unsafeDocumentIds.Count == 0,
                        PromptInjectionDetected = unsafeDocumentIds.Count > 0,
                        Details = documentResult.Details,
                        UnsafeDocumentIds = unsafeDocumentIds.Count > 0 ? unsafeDocumentIds : null,
                        DocumentResults = documentResult.DocumentResults
                    };

                    return Ok(response);
                }

                // Single content scanning mode
                _logger.LogInformation(
                    "Processing single content shield request for instance {InstanceId}.",
                    instanceId);

                var result = await _contentSafetyService.DetectPromptInjection(request.Content!);

                return Ok(new ContentShieldResponse
                {
                    Success = result.Success,
                    SafeContent = result.SafeContent,
                    PromptInjectionDetected = !result.SafeContent,
                    Details = result.Details
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing content shield request for instance {InstanceId}.", instanceId);

                return StatusCode(500, new ContentShieldResponse
                {
                    Success = false,
                    SafeContent = false,
                    PromptInjectionDetected = false,
                    Details = "An error occurred while processing the content shield request."
                });
            }
        }
    }
}
