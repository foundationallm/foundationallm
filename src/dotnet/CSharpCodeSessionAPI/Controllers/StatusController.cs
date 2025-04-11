using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.CSharpCodeSession.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class StatusController() : ControllerBase
    {
        /// <summary>
        /// Returns the status of the Orchestration API service.
        /// </summary>
        [AllowAnonymous]
        [HttpGet(Name = "GetServiceStatus")]
        public IActionResult Get() =>
            Ok(
            new {
                Name = "FoundationaLLM_CSharpCodeSessionAPI",
                Version = Environment.GetEnvironmentVariable("FOUNDATIONALLM_VERSION"),
                Status = "Ready"
            });

        /// <summary>
        /// Returns the allowed HTTP methods for the Orchestration API service.
        /// </summary>
        [AllowAnonymous]
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Append("Allow", new[] { "GET" });

            return Ok();
        }
    }
}
