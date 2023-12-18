
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Vectorization.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IVectorizationStateService _stateService;
        private readonly VectorizationWorkerSettings _settings;
        private readonly ILogger<Worker> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public Worker(
            IVectorizationStateService stateService,
            IOptions<VectorizationWorkerSettings> settings,
            ILoggerFactory loggerFactory)
        {
            _stateService = stateService;
            _settings = settings.Value;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<Worker>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var vectorizationWorker = new VectorizationWorkerBuilder()
                .WithStateService(_stateService)
                .WithSettings(_settings)
                .WithLoggerFactory(_loggerFactory)
                .WithCancellationToken(stoppingToken)
                .Build();

            await vectorizationWorker.Run();
        }
    }
}
