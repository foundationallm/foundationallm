using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.Pipelines
{
    /// <summary>
    /// Background worker used to manage vectorization data pipelines.
    /// </summary>
    /// <param name="pipelineExecutionService">The <see cref="IPipelineExecutionService"/> managing the execution of vectorization data pipelines.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class PipelineWorker(
        IPipelineExecutionService pipelineExecutionService,
        ILogger<PipelineWorker> logger) : BackgroundService
    {
        private readonly IPipelineExecutionService _pipelineExecutionService = pipelineExecutionService;
        private readonly ILogger<PipelineWorker> _logger = logger;

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The pipeline worker is starting up...");

            _logger.LogInformation("The pipeline worker is executing the pipeline execution service.");
            await _pipelineExecutionService.ExecuteAsync(stoppingToken);

            _logger.LogInformation("The pipeline execution service completed its execution.");
        }

        /// <inheritdoc/>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("The pipeline worker is stopping up the pipeline execution service...");
            await _pipelineExecutionService.StopAsync(cancellationToken);
        }
    }
}
