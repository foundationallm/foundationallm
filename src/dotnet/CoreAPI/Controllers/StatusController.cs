using Asp.Versioning;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using FoundationaLLM.Common.Interfaces;
using Microsoft.Identity.Web;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ICoreService _coreService;
        private readonly ILogger<StatusController> _logger;

        /// <summary>
        /// Constructor for the Status Controller.
        /// </summary>
        /// <param name="coreService">The Core service provides status methods.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="StatusController"/> type name.</param>
        public StatusController(ICoreService coreService,
            ILogger<StatusController> logger)
        {
            _coreService = coreService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the status of the Core service.
        /// </summary>
        [HttpGet(Name = "GetServiceStatus")]
        public string Get()
        {
            return _coreService.Status;
        }

        /// <summary>
        /// Returns the allowed HTTP methods for the Core service.
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Add("Allow", new[] { "GET", "POST", "OPTIONS" });
            
            return Ok();
        }
    }
}
