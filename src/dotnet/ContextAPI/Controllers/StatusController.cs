using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Context.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class StatusController() : ControllerBase
    {
        /// <summary>
        /// Returns the status of the Orchestration API service.
        /// </summary>
        [AllowAnonymous]
        [HttpGet(Name = "GetServiceStatus")]
        public async Task<ServiceStatusInfo> Get(string instanceId) =>
            new ServiceStatusInfo
            {
                Name = ServiceNames.ContextAPI,
                InstanceId = instanceId,
                InstanceName = ValidatedEnvironment.MachineName,
                Version = Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_Version),
                Status = ServiceStatuses.Ready
            };

        /// <summary>
        /// Returns the allowed HTTP methods for the Orchestration API service.
        /// </summary>
        [AllowAnonymous]
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Append("Allow", new[] { "GET", "POST", "OPTIONS" });

            return Ok();
        }
    }
}
