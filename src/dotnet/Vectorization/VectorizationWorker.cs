using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Services;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization
{
    public class VectorizationWorker
    {
        private readonly IDictionary<string, IRequestSourceService> _requestSourceServices;
        private readonly IVectorizationStateService _vectorizationStateService;
        private readonly IEnumerable<IRequestManagerService> _requestManagerServices;
        private readonly ILogger<VectorizationWorker> _logger;
        private readonly CancellationToken _cancellationToken;

        public VectorizationWorker(
            IVectorizationStateService vectorizationStateService,
            IDictionary<string, IRequestSourceService> requestSourceServices,
            IEnumerable<IRequestManagerService> requestManagerServices,
            ILogger<VectorizationWorker> logger,
            CancellationToken cancellationToken)
        {
            _vectorizationStateService = vectorizationStateService;
            _requestSourceServices = requestSourceServices;
            _requestManagerServices = requestManagerServices;
            _logger = logger;
            _cancellationToken = cancellationToken;
        }

        public async Task Run()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(10000, _cancellationToken);
            }
        }
    }
}
