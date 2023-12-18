using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using FoundationaLLM.Vectorization.Services;
using FoundationaLLM.Vectorization.Services.RequestSources;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization
{
    public class VectorizationWorkerBuilder
    {
        private VectorizationWorkerSettings? _settings;
        private IVectorizationStateService? _stateService;
        private CancellationToken _cancellationToken = default(CancellationToken);
        private ILoggerFactory? _loggerFactory;

        public VectorizationWorkerBuilder()
        {
        }

        public VectorizationWorker Build()
        {
            if (_stateService == null)
                throw new VectorizationException("Cannot build a vectorization worker without a valid vectorization state service.");

            if (_settings == null)
                throw new VectorizationException("Cannot build a vectorization worker without valid settings.");

            if (_loggerFactory == null)
                throw new VectorizationException("Cannot build a vectorization worker without a valid logger factory.");

            var requestSourceServices = _settings!.RequestManagers!
                .Select(rm => GetRequestSourceService(rm.RequestSourceName, _settings.QueuingEngine))
                .ToDictionary(rs => rs.SourceName);

            var requestManagerServices = _settings!.RequestManagers!
                .Select(rm => new RequestManagerService(
                    rm,
                    requestSourceServices,
                    _stateService,
                    _loggerFactory!.CreateLogger<RequestManagerService>(),
                    _cancellationToken))
                .ToList();

            var vectorizationWorker = new VectorizationWorker(
                _stateService,
                requestSourceServices,
                requestManagerServices,
                _loggerFactory!.CreateLogger<VectorizationWorker>(),
                _cancellationToken);

            return vectorizationWorker;
        }

        public VectorizationWorkerBuilder WithStateService(IVectorizationStateService stateService)
        {
            _stateService = stateService;
            return this;
        }

        public VectorizationWorkerBuilder WithSettings(VectorizationWorkerSettings settings) 
        {
            ValidateSettings(settings);

            _settings = settings;
            return this;
        }

        public VectorizationWorkerBuilder WithCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        public VectorizationWorkerBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            return this;
        }

        private void ValidateSettings(VectorizationWorkerSettings settings)
        {
            if (
                settings == null 
                || settings.RequestManagers == null
                || settings.RequestManagers.Count == 0)
                throw new ArgumentNullException(nameof(settings));

            foreach (var rm in settings.RequestManagers)
                if (string.IsNullOrEmpty(rm.RequestSourceName))
                    throw new ArgumentException("Invalid request source name.");
        }

        private IRequestSourceService GetRequestSourceService(string name, VectorizationQueuing queuing)
        {
            switch (queuing)
            {
                case VectorizationQueuing.None:
                    return new MemoryRequestSourceService(name, _loggerFactory!.CreateLogger<MemoryRequestSourceService>());
                case VectorizationQueuing.AzureStorageQueue:
                    return new QueueRequestSourceService(name, _loggerFactory!.CreateLogger<QueueRequestSourceService>());
                default:
                    throw new VectorizationException($"The vectorization queuing mechanism [{queuing}] is not supported.");
            }
        }
    }
}
