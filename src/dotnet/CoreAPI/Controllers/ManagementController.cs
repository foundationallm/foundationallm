using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods to manage resources.
    /// </summary>
    /// <param name="callContext">The call context containing user identity details.</param>
    /// <param name="resourceProviderServices">The list of <see cref="IResourceProviderService"/> resource providers.</param>
    /// <param name="managementCapabilitiesService">The management capabilities service used to restrict access
    /// to resource providers and resource types.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [ApiController]
    [Consumes("application/json", "multipart/form-data")]
    [Produces("application/json")]
    [Route($"management/instances/{{instanceId}}/providers/{{resourceProvider}}")]
    public class ResourceController(
        IOrchestrationContext callContext,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IManagementCapabilitiesService managementCapabilitiesService,
        ILogger<ResourceController> logger) : Controller
    {
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
        private readonly IManagementCapabilitiesService _managementCapabilitiesService = managementCapabilitiesService;
        private readonly ILogger<ResourceController> _logger = logger;
        private readonly IOrchestrationContext _callContext = callContext;

        /// <summary>
        /// Gets one or more resources.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceProvider">The name of the resource provider that should handle the request.</param>
        /// <param name="resourcePath">The logical path of the resource type.</param>
        /// <param name="queryParams">The query parameters.</param>
        /// <returns></returns>
        [HttpGet("{*resourcePath}", Name = "GetResources")]
        public async Task<IActionResult> GetResources(
            string instanceId,
            string resourceProvider,
            string resourcePath,
            [FromQuery] Dictionary<string, string> queryParams) =>
            await HandleRequest(
                resourceProvider,
                resourcePath,
                async (resourceProviderService) =>
                {
                    var result = await resourceProviderService.HandleGetAsync(
                        $"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}",
                        _callContext.CurrentUserIdentity!,
                        ResourceProviderGetOptions.FromQueryParams(
                                queryParams,
                                includeRolesDefault: true,
                                includeActionsDefault: false),
                        resourcePathAvailabilityChecker: _managementCapabilitiesService.IsResourcePathAvailable);
                    return new OkObjectResult(result);
                });

        /// <summary>
        /// Creates or updates resources.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceProvider">The name of the resource provider that should handle the request.</param>
        /// <param name="resourcePath">The logical path of the resource type.</param>
        /// <param name="queryParams">The query parameters.</param>
        /// <returns>The ObjectId of the created or updated resource.</returns>
        [HttpPost("{*resourcePath}", Name = "UpsertResource")]
        public async Task<IActionResult> UpsertResource(
            string instanceId,
            string resourceProvider,
            string resourcePath,
            [FromQuery] Dictionary<string, string> queryParams) =>
            await HandleRequest(
                resourceProvider,
                resourcePath,
                async (resourceProviderService) =>
                {
                    var formFiles = HttpContext.Request.HasFormContentType ? HttpContext.Request.Form?.Files : null;
                    IFormFile? formFile = (formFiles != null && formFiles.Count > 0) ? formFiles[0] : null;

                    Dictionary<string, string>? formPayload = null;
                    if (HttpContext.Request.HasFormContentType)
                        formPayload = HttpContext.Request.Form?.Keys.ToDictionary(k => k, v => HttpContext.Request.Form[v].ToString());

                    // First, attempt to retrieve the serialized resource from the request body.
                    // If it is not found, attempt to retrieve it from the form payload using the well-known 'resource' key.
                    var bodyContent = await (new StreamReader(HttpContext.Request.Body)).ReadToEndAsync();
                    if (string.IsNullOrWhiteSpace(bodyContent)
                        && formPayload != null)
                        formPayload.TryGetValue(HttpFormDataKeys.Resource, out bodyContent);
                    string? serializedResource = !string.IsNullOrWhiteSpace(bodyContent) ? bodyContent : null;

                    if ((formFile == null || formFile.Length == 0) && serializedResource == null)
                        throw new ResourceProviderException("The serialized resource and the attached file cannot be null at the same time.", StatusCodes.Status400BadRequest);

                    ResourceProviderFormFile? resourceProviderFormFile = default;
                    if (formFile != null && formFile.Length > 0)
                    {
                        await using var stream = formFile.OpenReadStream();
                        resourceProviderFormFile = new()
                        {
                            FileName = formFile.FileName,
                            ContentType = formFile.ContentType,
                            BinaryContent = BinaryData.FromStream(stream),
                            Payload = formPayload
                        };
                    }

                    var result = await resourceProviderService.HandlePostAsync(
                        $"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}",
                        serializedResource?.ToString(),
                        resourceProviderFormFile,
                        _callContext.CurrentUserIdentity!,
                        resourcePathAvailabilityChecker: _managementCapabilitiesService.IsResourcePathAvailable,
                        requestPayloadValidator: _managementCapabilitiesService.IsValidRequestPayload,
                        urlEncodedParentResourcePath: queryParams.TryGetValue("parentResource", out var parentResource)
                            ? parentResource
                            : null);
                    return new OkObjectResult(result);
                });

        /// <summary>
        /// Deletes a resource.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceProvider">The name of the resource provider that should handle the request.</param>
        /// <param name="resourcePath">The logical path of the resource type.</param>
        /// <returns></returns>
        [HttpDelete("{*resourcePath}", Name = "DeleteResource")]
        public async Task<IActionResult> DeleteResource(string instanceId, string resourceProvider, string resourcePath) =>
            await HandleRequest(
                resourceProvider,
                resourcePath,
                async (resourceProviderService) =>
                {
                    var objectId = $"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}";
                    await resourceProviderService.HandleDeleteAsync(
                        objectId,
                        _callContext.CurrentUserIdentity!,
                        resourcePathAvailabilityChecker: _managementCapabilitiesService.IsResourcePathAvailable);
                    return new OkObjectResult(new ResourceProviderActionResult(objectId, true));
                });

        private async Task<IActionResult> HandleRequest(string resourceProvider, string resourcePath, Func<IResourceProviderService, Task<IActionResult>> handler)
        {
            if (!_resourceProviderServices.TryGetValue(resourceProvider, out var resourceProviderService))
                return new NotFoundResult();

            try
            {
                return await handler(resourceProviderService);
            }
            catch (ResourceProviderException ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The {ResourceProviderName} encountered an error while handling the request for {ResourcePath}.", resourceProvider, resourcePath);
                return StatusCode(StatusCodes.Status500InternalServerError, $"The {resourceProvider} encountered an error while handling the request for {resourcePath}.");
            }
        }
    }
}
