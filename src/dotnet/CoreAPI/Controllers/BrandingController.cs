using Asp.Versioning;
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
    [Route("[controller]")]
    public class BrandingController : ControllerBase
    {
        private readonly ClientBrandingConfiguration _settings;

        /// <summary>
        /// Constructor for the Branding Controller.
        /// </summary>
        /// <param name="settings"></param>
        public BrandingController(IOptions<ClientBrandingConfiguration> settings) =>
            _settings = settings.Value;

        /// <summary>
        /// Retrieves the branding information for the client.
        /// </summary>
        [AllowAnonymous]
        [HttpGet(Name = "GetBranding")]
        public IActionResult Index() =>
            Ok(_settings);
    }
}
