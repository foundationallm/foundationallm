using Asp.Versioning;
using FoundationaLLM.Attachment.Models;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides branding information for the client.
    /// </summary>
    [Authorize(Policy = "DefaultPolicy")]
    [ApiController]
    [Route("[controller]")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IResourceProviderService _attachmentResourceProvider;
#pragma warning disable IDE0052 // Remove unread private members.
        private readonly ILogger<AttachmentsController> _logger;

        public AttachmentsController(
            IEnumerable<IResourceProviderService> resourceProviderServices,
            ILogger<AttachmentsController> logger)
        {
            _logger = logger;
            var resourceProviderServicesDictionary = resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
            if (!resourceProviderServicesDictionary.TryGetValue(ResourceProviderNames.FoundationaLLM_Attachment, out var attachmentResourceProvider))
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Attachment} was not loaded.");
            _attachmentResourceProvider = attachmentResourceProvider;
        }

        /// <summary>
        /// Retrieves the uploaded attachments.
        /// </summary>
        [HttpGet(Name = "Get")]
        public IActionResult Index() =>
            Ok();

        /// <summary>
        /// Uploads an attachment.
        /// </summary>
        /// <param name="file">The file sent with the HTTP request.</param>
        /// <returns></returns>
        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not selected.");

            var fileName = file.FileName;
            var name = fileName.GenerateValidResourceName();
            var contentType = file.ContentType;

            using (var stream = file.OpenReadStream())
            {
                try
                {
                    var result = await _attachmentResourceProvider.UpsertResourceAsync(
                        $"attachments/{name}",
                        new AttachmentFile
                        {
                            Name = name,
                            Content = stream,
                            DisplayName = fileName,
                            ContentType = contentType
                        });
                    return Ok(result);
                }
                catch (ResourceProviderException e)
                {
                    return BadRequest(e.Message);
                }
            }
        }
    }
}
