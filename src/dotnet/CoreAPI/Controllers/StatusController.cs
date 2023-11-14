using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

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
        /// <summary>
        /// Returns the status of the Core API service.
        /// </summary>
        [HttpGet(Name = "GetServiceStatus")]
        public IActionResult Get()
        {
            return Ok();
        }

        /// <summary>
        /// Returns the allowed HTTP methods for the Core API service.
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Add("Allow", new[] { "GET", "POST", "OPTIONS" });
            
            return Ok();
        }
    }
}
