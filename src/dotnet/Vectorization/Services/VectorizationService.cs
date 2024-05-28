using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Extensions;
using FoundationaLLM.Vectorization.Handlers;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services
{
    /// <summary>
    /// Implements the <see cref="IVectorizationService"/> interface.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the <see cref="VectorizationService"/> service.
    /// </remarks>
    /// <param name="requestSourcesCache">The <see cref="IRequestSourcesCache"/> cache of request sources.</param>
    /// <param name="vectorizationStateService">The service providing vectorization state management.</param>
    /// <param name="resourceProviderServices">Retrieves all registered resource provider services <see cref="IResourceProviderService"/>.</param>    
    /// <param name="stepsConfiguration">The <see cref="IConfigurationSection"/> object providing access to the settings.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class VectorizationService(
        IRequestSourcesCache requestSourcesCache,
        IVectorizationStateService vectorizationStateService,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_Steps)] IConfigurationSection stepsConfiguration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationService
    {
        private readonly Dictionary<string, IRequestSourceService> _requestSources = requestSourcesCache.RequestSources;
        private readonly IVectorizationStateService _vectorizationStateService = vectorizationStateService;
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);       
        private readonly IConfigurationSection? _stepsConfiguration = stepsConfiguration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly ILogger<VectorizationService> _logger = loggerFactory.CreateLogger<VectorizationService>();        

        /// <inheritdoc/>
        public async Task<VectorizationResult> ProcessRequest(VectorizationRequest vectorizationRequest)
        {            
            try
            {                
                switch (vectorizationRequest.ProcessingType)
                {
                    case VectorizationProcessingType.Asynchronous:
                        var firstRequestSource = _requestSources[vectorizationRequest.Steps.First().Id];
                        await firstRequestSource.SubmitRequest(vectorizationRequest);
                        return new VectorizationResult(vectorizationRequest.ObjectId!, true, null);
                    case VectorizationProcessingType.Synchronous:
                        return await ProcessRequestInternal(vectorizationRequest);
                    default:
                        throw new VectorizationException($"The vectorization processing type {vectorizationRequest.ProcessingType} is not supported.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new VectorizationResult(vectorizationRequest.ObjectId!, false, ex.Message);
            }
        }

        private async Task<VectorizationResult> ProcessRequestInternal(VectorizationRequest request)
        {
            var vectorizationResourceProvider = GetVectorizationResourceProvider();
            request.ProcessingState = VectorizationProcessingState.InProgress;
            request.ExecutionStart = DateTime.UtcNow;
            await request.UpdateVectorizationRequestResource(vectorizationResourceProvider).ConfigureAwait(false);


            _logger.LogInformation("Starting synchronous processing for request {RequestId}.", request.Name);

            var state = VectorizationState.FromRequest(request);

            foreach (var step in request.Steps)
            {
                _logger.LogInformation("Starting step [{Step}] for request {RequestId}.", step.Id, request.Name);

                var stepHandler = VectorizationStepHandlerFactory.Create(
                    step.Id,
                    "N/A",
                    step.Parameters,
                    _stepsConfiguration,
                    _vectorizationStateService,
                    _serviceProvider,
                    _loggerFactory);

                // vectorization request state is persisted in the Invoke method.
                var handlerSuccess = await stepHandler.Invoke(request, state, default).ConfigureAwait(false);
                if (!handlerSuccess)
                    break;

                var steps = request.MoveToNextStep();

                if (!string.IsNullOrEmpty(steps.CurrentStep))
                    _logger.LogInformation("The pipeline for request id {RequestId} was advanced from step [{PreviousStepName}] to step [{CurrentStepName}].",
                        request.Name, steps.PreviousStep, steps.CurrentStep);
                else                
                    _logger.LogInformation("The pipeline for request id {RequestId} was advanced from step [{PreviousStepName}] to finalized state.",
                       request.Name, steps.PreviousStep);                 
               
                // save execution state
                await _vectorizationStateService.SaveState(state).ConfigureAwait(false);
            }

            if (request.Complete)
            {
                // update the vectorization request state to Completed.
                request.ProcessingState = VectorizationProcessingState.Completed;
                request.ExecutionEnd = DateTime.UtcNow;
                await request.UpdateVectorizationRequestResource(vectorizationResourceProvider).ConfigureAwait(false);

                _logger.LogInformation("Finished synchronous processing for request {RequestId}. All steps were processed successfully.", request.Name);
                return new VectorizationResult(request.ObjectId!, true, null);
            }
            else
            {
                var errorMessage =
                    $"Execution stopped at step [{request.CurrentStep}] due to an error.";

                // update the vectorization request state to Completed.
                request.ProcessingState = VectorizationProcessingState.Failed;
                request.ExecutionEnd = DateTime.UtcNow;
                await request.UpdateVectorizationRequestResource(vectorizationResourceProvider).ConfigureAwait(false);
                _logger.LogInformation("Finished synchronous processing for request {RequestId}. {ErrorMessage}", request.Name, errorMessage);
                return new VectorizationResult(request.ObjectId!, false, errorMessage);
            }
        }

        /// <summary>
        /// Obtains the vectorization resource provider from available resource providers.
        /// </summary>
        /// <returns>The vectorization resource provider</returns>
        /// <exception cref="VectorizationException"></exception>
        private IResourceProviderService GetVectorizationResourceProvider()
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vectorization} was not loaded.");


            return vectorizationResourceProviderService;
        }
    }
}
