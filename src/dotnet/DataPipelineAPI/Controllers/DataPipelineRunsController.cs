using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.DataPipeline.API.Controllers
{
    /// <summary>
    /// Provides the routes for the Data Pipelines API data pipelines controller.
    /// </summary>
    /// <param name="dataPipelineService">The service used to manage data pipeline runs.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}")]
    public class DataPipelineRunsController(
        IDataPipelineService dataPipelineService,
        ILogger<DataPipelineRunsController> logger): ControllerBase
    {
        private readonly IDataPipelineService _dataPipelineService = dataPipelineService;
        private readonly ILogger<DataPipelineRunsController> _logger = logger;

        /// <summary>
        /// Retrieves a data pipeline run by its name.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRunName">The name of the data pipeline run.</param>
        /// <returns>The data pipeline run identified by the provided name.</returns>
        [HttpGet("datapipelineruns/{dataPipelineRunName}")]
        public async Task<IActionResult> GetDataPipelineRun(
            string instanceId,
            string dataPipelineRunName)
        {
            var dataPipelineRun = await _dataPipelineService.GetDataPipelineRun(
                instanceId,
                dataPipelineRunName);

            return Ok(dataPipelineRun);
        }

        /// <summary>
        /// Creates a new data pipeline run.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRun">The object with the properties of the new data pipeline run.</param>
        /// <returns>The newly created data pipeline run.</returns>
        [HttpPost("datapipelineruns")]
        public async Task<IActionResult> CreateDataPipelineRun(
            string instanceId,
            [FromBody] DataPipelineRun dataPipelineRun)
        {
            var updatedDataPipelineRun = await _dataPipelineService.CreateDataPipelineRun(
                instanceId,
                dataPipelineRun);

            return Ok(updatedDataPipelineRun);
        }
    }
}
