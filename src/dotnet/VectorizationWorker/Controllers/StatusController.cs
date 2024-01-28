using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Vectorization.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Vectorization.Worker.Controllers
{
    /// <summary>
    /// Methods for managing vectorization requests.
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Gets the status of the vectorization worker.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<bool> GetWorkerStatus()
        {
            await Task.CompletedTask;
            return true;
        }
    }
}
