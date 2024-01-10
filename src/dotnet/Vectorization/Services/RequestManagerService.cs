﻿using FoundationaLLM.Common.Tasks;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Handlers;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services
{
    /// <summary>
    /// Manages vectorization requests originating from a specific request source.
    /// </summary>
    public class RequestManagerService : IRequestManagerService
    {
        private readonly RequestManagerServiceSettings _settings;
        private readonly Dictionary<string, IRequestSourceService> _requestSourceServices;
        private readonly IRequestSourceService _incomingRequestSourceService;
        private readonly IVectorizationStateService _vectorizationStateService;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;
        private readonly TaskPool _taskPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestManagerService"/> class with the configuration and services
        /// required to manage vectorization requests originating from a specific request source.
        /// </summary>
        /// <param name="settings">The configuration settings used to initialize the instance.</param>
        /// <param name="requestSourceServices">The dictionary with all the request source services registered in the vectorization platform.</param>
        /// <param name="vectorizationStateService">The service providing vectorization state management.</param>
        /// <param name="logger">The logger service.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the work.</param>
        /// <exception cref="VectorizationException">The exception thrown when the initialization of the instance fails.</exception>
        public RequestManagerService(
            RequestManagerServiceSettings settings,
            Dictionary<string, IRequestSourceService> requestSourceServices,
            IVectorizationStateService vectorizationStateService,
            ILogger<RequestManagerService> logger,
            CancellationToken cancellationToken)
        {
            _settings = settings;
            _requestSourceServices = requestSourceServices;
            _vectorizationStateService = vectorizationStateService;

            _logger = logger;
            _cancellationToken = cancellationToken;

            if (!_requestSourceServices.TryGetValue(_settings.RequestSourceName, out IRequestSourceService? value) || value == null)
                throw new VectorizationException($"Could not find a request source service for [{_settings.RequestSourceName}].");

            _incomingRequestSourceService = value;

            _taskPool = new TaskPool(_settings.MaxHandlerInstances);
        }

        /// <inheritdoc/>
        public async Task Run()
        {
            _logger.LogInformation("The request manager service associated with source [{RequestSourceName}] started processing requests.", _settings.RequestSourceName);

            while (true)
            {
                if (_cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var taskPoolAvailableCapacity = _taskPool.AvailableCapacity;

                    if (taskPoolAvailableCapacity > 0 && (await _incomingRequestSourceService.HasRequests()))
                    {
                        var requests = await _incomingRequestSourceService.ReceiveRequests(taskPoolAvailableCapacity);

                        // No need to use ConfigureAwait(false) since the code is going to be executed on a
                        // thread pool thread, with no user code higher on the stack (for details, see
                        // https://devblogs.microsoft.com/dotnet/configureawait-faq/).
                        _taskPool.Add(
                            requests.Select(r => Task.Run(
                                async () => { await ProcessRequest(r.Request, r.MessageId, r.PopReceipt); },
                                _cancellationToken)));
                    }
                    else
                        // Either the task pool is at capacity or there are no new requests available.
                        // Wait a predefined amount of time before attempting to receive requests again.
                        await Task.Delay(TimeSpan.FromMinutes(1));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in request processing loop (request source name: {RequestSourceName}).", _settings.RequestSourceName);
                }
            }

            _logger.LogInformation("The request manager service associated with source [{RequestSourceName}] finished processing requests.", _settings.RequestSourceName);
        }

        private async Task ProcessRequest(VectorizationRequest request, string messageId, string popReceipt)
        {
            try
            {
                await HandleRequest(request);
                await AdvanceRequest(request);

                await _incomingRequestSourceService.DeleteRequest(messageId, popReceipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request with id {RequestId}.", request.Id);
            }
        }

        private async Task HandleRequest(VectorizationRequest request)
        {
            var state = await _vectorizationStateService.HasState(request)
                ? await _vectorizationStateService.ReadState(request)
                : VectorizationState.FromRequest(request);

            var stepHandler = VectorizationStepHandlerFactory.Create(_settings.RequestSourceName, request[_settings.RequestSourceName]!.Parameters);
            await stepHandler.Invoke(request, state, _cancellationToken);

            await _vectorizationStateService.SaveState(state);
        }

        private async Task AdvanceRequest(VectorizationRequest request)
        {
            var nextStep = request.MoveToNextStep();

            if (!string.IsNullOrEmpty(nextStep))
            {
                // The vectorization request still has steps to be processed

                if (!_requestSourceServices.TryGetValue(nextStep, out IRequestSourceService? value) || value == null)
                    throw new VectorizationException($"Could not find the [{nextStep}] request source service for request id {request.Id}.");

                await value.SubmitRequest(request);
            }
        }
    }
}
