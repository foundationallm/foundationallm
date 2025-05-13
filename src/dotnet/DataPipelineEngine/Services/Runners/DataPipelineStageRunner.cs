using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.DataPipelineEngine.Services.Runners
{
    /// <summary>
    /// Provides capabilities for running data pipelines.
    /// </summary>
    /// <param name="stateService">The Data Pipeline State service.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineStageRunner(
        IDataPipelineStateService stateService,
        ILogger<DataPipelineStageRunner> logger)
    {
        private readonly IDataPipelineStateService _stateService = stateService;
        private readonly ILogger<DataPipelineStageRunner> _logger = logger;
    }
}
