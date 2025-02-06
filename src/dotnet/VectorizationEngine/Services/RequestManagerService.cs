﻿using FoundationaLLM.Common.Tasks;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Vectorization.Handlers;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Vectorization.Extensions;
using FoundationaLLM.Vectorization.ResourceProviders;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;

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
        private readonly IConfigurationSection? _stepsConfiguration;
        private readonly IServiceProvider _serviceProvider;        
        private readonly ILogger<RequestManagerService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly CancellationToken _cancellationToken;
        private readonly TaskPool _taskPool;
                
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestManagerService"/> class with the configuration and services
        /// required to manage vectorization requests originating from a specific request source.
        /// </summary>
        /// <param name="settings">The configuration settings used to initialize the instance.</param>
        /// <param name="requestSourceServices">The dictionary with all the request source services registered in the vectorization platform.</param>
        /// <param name="vectorizationStateService">The service providing vectorization state management.</param>
        /// <param name="stepsConfiguration">The <see cref="IConfigurationSection"/> object providing access to the settings.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the work.</param>
        /// <exception cref="VectorizationException">The exception thrown when the initialization of the instance fails.</exception>
        public RequestManagerService(
            RequestManagerServiceSettings settings,
            Dictionary<string, IRequestSourceService> requestSourceServices,
            IVectorizationStateService vectorizationStateService,
            IConfigurationSection? stepsConfiguration,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            _settings = settings;
            _requestSourceServices = requestSourceServices;
            _vectorizationStateService = vectorizationStateService;
            _stepsConfiguration = stepsConfiguration;

            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<RequestManagerService>();
            _cancellationToken = cancellationToken;

            if (!_requestSourceServices.TryGetValue(_settings.RequestSourceName, out IRequestSourceService? value) || value == null)
                throw new VectorizationException($"Could not find a request source service for [{_settings.RequestSourceName}].");

            _incomingRequestSourceService = value;

            _taskPool = new TaskPool(_settings.MaxHandlerInstances, _loggerFactory.CreateLogger<TaskPool>());
        }

        /// <inheritdoc/>
        public async Task Run()
        {
            var vectorizationResourceProvider = GetVectorizationResourceProvider();
            _logger.LogInformation("The request manager service associated with source [{RequestSourceName}] started processing requests.", _settings.RequestSourceName);
            
            while (true)
            {
                if (_cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var taskPoolAvailableCapacity = _taskPool.AvailableCapacity;
                    
                    if (taskPoolAvailableCapacity > 0 && (await _incomingRequestSourceService.HasRequests().ConfigureAwait(false)))
                    {
                        var requests = await _incomingRequestSourceService.ReceiveRequests(taskPoolAvailableCapacity).ConfigureAwait(false);                        
                        var receivedTime = DateTimeOffset.UtcNow;
                        var validRequests = new List<VectorizationRequestProcessingContext>();
                        foreach (var dequeuedRequest in requests)
                        {
                            //hydrate the vectorization request
                            var request = await vectorizationResourceProvider.GetVectorizationRequestResource(dequeuedRequest.RequestName, ServiceContext.ServiceIdentity!);

                            var requestInstanceId = default(string);
                            if (string.IsNullOrWhiteSpace(request.ObjectId)
                                || !ResourcePath.TryParseInstanceId(request.ObjectId, out requestInstanceId))
                            {
                                // We are signaling that the request is invalid by setting the error count to int.MaxValue
                                request.ErrorCount = int.MaxValue;
                            }

                            request.ProcessingState = VectorizationProcessingState.InProgress;
                            if (request.ExecutionStart == null)
                                request.ExecutionStart = DateTime.UtcNow;

                            // Check if the request is still valid and the number of retries has not been exceeded.
                            if (request.Expired
                                || request.ErrorCount > _settings.QueueMaxNumberOfRetries)
                            {
                                var errorMessage = string.Empty;

                                if (request.Expired)
                                {
                                    _logger.LogWarning(
                                        "The message with id {MessageId} containing the request with id {RequestId} has expired and will be deleted (the last time a step was successfully processed was {LastSuccessfulStepTime}).",
                                        dequeuedRequest.MessageId,
                                        request.Name,
                                        request.LastSuccessfulStepTime);
                                    errorMessage = $"The message with id {dequeuedRequest.MessageId} containing the request with id {request.Name} has expired and will be deleted (the last time a step was successfully processed was {request.LastSuccessfulStepTime}).";
                                }
                                else if (request.ErrorCount.Equals(int.MaxValue))
                                {
                                    _logger.LogError(
                                        "The message with id {MessageId} contains the request with id {RequestId} which has an invalid object identifier. The request will be discarded and the message deleted.",
                                        dequeuedRequest.MessageId,
                                        request.Name);
                                    errorMessage = $"The message with id {dequeuedRequest.MessageId} contains the request with id {request.Name} which has an invalid object identifier. The request will be discarded and the message deleted.";
                                }
                                else
                                {
                                    _logger.LogWarning(
                                        "The message with id {MessageId} containing the request with id {RequestId} encountered {ErrorCount} consecutive errors while processing and will be deleted.",
                                        dequeuedRequest.MessageId,
                                        request.Name,
                                        request.ErrorCount);
                                    errorMessage = $"ERROR: The message with id {dequeuedRequest.MessageId} containing the request with id {request.Name} encountered {request.ErrorCount} consecutive errors while processing and will be deleted.";
                                }

                                // Retrieve the execution state of the request
                                var state = await GetVectorizationRequestState(request);

                                state.LogEntries.Add(
                                    new VectorizationLogEntry(
                                        request.Name!,
                                        dequeuedRequest.MessageId,
                                        request.CurrentStep ?? "N/A",
                                        errorMessage
                                    )
                                );

                                // Update vectorization request state if there's an error.
                                if (!string.IsNullOrWhiteSpace(errorMessage))
                                {
                                    request.ProcessingState = VectorizationProcessingState.Failed;
                                    request.ExecutionEnd = DateTime.UtcNow;
                                    request.ErrorMessages.Add(errorMessage);
                                }

                                // Remove the message from the queue
                                await _incomingRequestSourceService.DeleteRequest(dequeuedRequest.MessageId, dequeuedRequest.PopReceipt).ConfigureAwait(false);

                                // Persist the state of the vectorization request
                                await _vectorizationStateService.SaveState(state).ConfigureAwait(false);
                                // Update the vectorization request resource
                                if (!request.ErrorCount.Equals(int.MaxValue))
                                    await request.UpdateVectorizationRequestResource(requestInstanceId!, vectorizationResourceProvider, ServiceContext.ServiceIdentity!);
                                // Verify if the pipeline state needs to be updated
                                await UpdatePipelineState(request).ConfigureAwait(false);
                            }
                            else
                            {
                                validRequests.Add(new VectorizationRequestProcessingContext
                                {
                                    InstanceId = requestInstanceId!,
                                    Request = request,
                                    DequeuedRequest = dequeuedRequest
                                });
                            }
                        }


                        var ignoredRequests = validRequests
                            .Where(r => _taskPool.HasRunningTaskForPayload(r.Request.Name))
                            .Select(r => r.Request.Name)
                            .ToList();

                        if (ignoredRequests.Count > 0)
                            _logger.LogWarning("The following requests were dequeued while still being processed based on the previous dequeuing: {IgnoredRequestIds}. The requests will be ignored.",
                                string.Join(",", ignoredRequests));

                        // Add the request to the task pool for processing
                        // No need to use ConfigureAwait(false) since the code is going to be executed on a
                        // thread pool thread, with no user code higher on the stack (for details, see
                        // https://devblogs.microsoft.com/dotnet/configureawait-faq/).
                        _taskPool.Add(validRequests
                            .Where(r => !_taskPool.HasRunningTaskForPayload(r.Request.Name))
                            .Select(r => new TaskInfo
                                {
                                    PayloadId = r.Request.Name,
                                    Task = ProcessRequest(r.InstanceId, r.Request, r.DequeuedRequest.MessageId, r.DequeuedRequest.PopReceipt, ServiceContext.ServiceIdentity!, _cancellationToken),
                                    StartTime = DateTimeOffset.UtcNow
                                }));                            
                                               
                        // Pace retrieving requests by a pre-determined delay           
                        await Task.Delay(TimeSpan.FromSeconds(_settings.QueueProcessingPace));
                    }
                    else
                    {                        
                        // Wait a predefined amount of time before attempting to receive requests again.
                        await Task.Delay(TimeSpan.FromSeconds(_settings.QueuePollingInterval));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in request processing loop (request source name: {RequestSourceName}).", _settings.RequestSourceName);                   
                }
            }

            _logger.LogInformation("The request manager service associated with source [{RequestSourceName}] finished processing requests.", _settings.RequestSourceName);
        }

        private async Task ProcessRequest(string requestInstanceId, VectorizationRequest request, string messageId, string popReceipt, UnifiedUserIdentity userIdentity, CancellationToken cancellationToken)
        {
            var state = await GetVectorizationRequestState(request);

            try
            {
                if (await HandleRequest(request, state, messageId, userIdentity, cancellationToken).ConfigureAwait(false))
                {
                    // If the request was handled successfully, remove it from the current source and advance it to the next step.
                    await _incomingRequestSourceService.DeleteRequest(messageId, popReceipt).ConfigureAwait(false);
                    await AdvanceRequest(requestInstanceId, request).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request with id {RequestId}.", request.Name);
                request.ErrorCount++;               
            }
            finally
            {
                await _vectorizationStateService.SaveState(state).ConfigureAwait(false);
                await request.UpdateVectorizationRequestResource(requestInstanceId, GetVectorizationResourceProvider(), ServiceContext.ServiceIdentity!).ConfigureAwait(false);
                await UpdatePipelineState(request).ConfigureAwait(false);
            }
            
        }

        private async Task<bool> HandleRequest(VectorizationRequest request, VectorizationState state, string messageId, UnifiedUserIdentity userIdentity, CancellationToken cancellationToken)
        {
            var stepHandler = VectorizationStepHandlerFactory.Create(
                _settings.RequestSourceName,
                messageId,
                request[_settings.RequestSourceName]!.Parameters,
                _stepsConfiguration,
                _vectorizationStateService,
                _serviceProvider,
                _loggerFactory);
            var handlerSuccess = await stepHandler.Invoke(request, state, userIdentity, cancellationToken).ConfigureAwait(false);
            return handlerSuccess;
        }

        private async Task AdvanceRequest(string requestInstanceId, VectorizationRequest request)
        {
            var state = await GetVectorizationRequestState(request);

            var vectorizationResourceProvider = GetVectorizationResourceProvider();
            var (PreviousStep, CurrentStep) = request.MoveToNextStep();                      

            if (!string.IsNullOrEmpty(CurrentStep))
            {
                // The vectorization request still has steps to be processed
                if (!_requestSourceServices.TryGetValue(CurrentStep, out IRequestSourceService? value) || value == null)
                {
                    var errorMessage = $"Could not find the [{CurrentStep}] request source service for request id {request.Name}.";
                    request.ProcessingState = VectorizationProcessingState.Failed;
                    request.ErrorMessages.Add(errorMessage);
                    await request.UpdateVectorizationRequestResource(requestInstanceId, vectorizationResourceProvider, ServiceContext.ServiceIdentity!).ConfigureAwait(false);                    
                    throw new VectorizationException(errorMessage);
                }

                await value.SubmitRequest(request.Name).ConfigureAwait(false);

                _logger.LogInformation("The pipeline for request id {RequestId} was advanced from step [{PreviousStepName}] to step [{CurrentStepName}].",
                    request.Name, PreviousStep, CurrentStep);               
            }
            else
            {
                _logger.LogInformation("The pipeline for request id {RequestId} was advanced from step [{PreviousStepName}] to finalized state.",
                    request.Name, PreviousStep);
                request.ProcessingState = VectorizationProcessingState.Completed;
                request.ExecutionEnd = DateTime.UtcNow;               
            }
            state.UpdateRequest(request);            
        }

        /// <summary>
        /// Updates the state of the pipeline if the request is part of a pipeline.
        /// Expects the state of the request to already be persisted.
        /// </summary>
        /// <param name="request">The vectorization request</param>        
        private async Task UpdatePipelineState(VectorizationRequest request)
        {
            // check if the request is part of a pipeline and update the pipeline state if necessary
            if (
                request.ProcessingState != VectorizationProcessingState.InProgress
                && request.ProcessingState != VectorizationProcessingState.New
                && request.PipelineObjectId is not null
                && request.PipelineExecutionId is not null)
            {
                var pipelineName = request.PipelineObjectId.Split('/').Last();
                // obtain the current state of the pipeline based on child vectorization requests
                var currentPipelineState = await _vectorizationStateService.GetPipelineExecutionProcessingState(
                        GetVectorizationResourceProvider(),
                        pipelineName,
                        request.PipelineExecutionId,
                        ServiceContext.ServiceIdentity!);                

                // pipelines are automatically set to InProgress when executed, update if the current status is different
                if (currentPipelineState != VectorizationProcessingState.InProgress)
                {
                    //retrieve pipeline state file
                    var pipelineState = await _vectorizationStateService.ReadPipelineState(pipelineName, request.PipelineExecutionId);
                    pipelineState.ProcessingState = currentPipelineState;
                    if(pipelineState.ProcessingState == VectorizationProcessingState.Completed || pipelineState.ProcessingState == VectorizationProcessingState.Failed)
                    {
                        pipelineState.ExecutionEnd = DateTime.UtcNow;
                    }
                    await _vectorizationStateService.SavePipelineState(pipelineState);
                }

            }
        }

        /// <summary>
        /// Retrieves an instance of the vectorization resource provider service.
        /// </summary>
        /// <returns>An instance of the vectorization resource provider service.</returns>
        /// <exception cref="VectorizationException"></exception>
        private VectorizationResourceProviderService GetVectorizationResourceProvider()
        {
            var vectorizationResourceProviderService = _serviceProvider.GetRequiredService<IEnumerable<IResourceProviderService>>()
                        .Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Vectorization);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vectorization} was not loaded.");

            return (VectorizationResourceProviderService)vectorizationResourceProviderService;
        }

        /// <summary>
        /// Retrieve the state of the request.
        /// </summary>
        /// <param name="request">The vectorization request.</param>
        /// <returns>The VectorizationState associated with the vectorization request.</returns>
        private async Task<VectorizationState> GetVectorizationRequestState(VectorizationRequest request)
        {
            var state = await _vectorizationStateService.HasState(request).ConfigureAwait(false)
                ? await _vectorizationStateService.ReadState(request).ConfigureAwait(false)
                : VectorizationState.FromRequest(request);
            return state;
        }


    }
}
