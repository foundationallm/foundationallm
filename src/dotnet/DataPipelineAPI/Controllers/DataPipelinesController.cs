using FoundationaLLM.Common.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.DataPipeline.API.Controllers
{
    /// <summary>
    /// Provides the routes for the Data Pipelines API data pipelines controller.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}")]
    public class DataPipelinesController(
        ILogger<DataPipelinesController> logger): ControllerBase
    {
        private readonly ILogger<DataPipelinesController> _logger = logger;
    }
}
