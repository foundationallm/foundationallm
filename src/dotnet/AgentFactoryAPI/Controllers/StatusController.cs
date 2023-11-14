using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.AgentFactory.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Returns the status of the Agent Factory API service.
        /// </summary>
        [HttpGet(Name = "GetServiceStatus")]
        public IActionResult Get()
        {
            return Ok();
        }

        /// <summary>
        /// Returns the allowed HTTP methods for the Agent Factory API service.
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Add("Allow", new[] { "GET", "POST", "OPTIONS" });

            return Ok();
        }
    }
}
