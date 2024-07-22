using FoundationaLLM.Common.Models.Configuration.Branding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides branding information for the client.
    /// </summary>
    [Authorize(Policy = "DefaultPolicy")]
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class BrandingController : ControllerBase
    {
        private readonly ClientBrandingConfiguration _settings;

        /// <summary>
        /// Constructor for the Branding Controller.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="settings">The branding settings.</param>
        public BrandingController(string instanceId, IOptions<ClientBrandingConfiguration> settings) =>
            _settings = settings.Value;

        /// <summary>
        /// Retrieves the branding information for the client.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        [AllowAnonymous]
        [HttpGet(Name = "GetBranding")]
        public IActionResult Index(string instanceId) =>
            Ok(_settings);
    }
}
