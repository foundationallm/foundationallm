using DocumentFormat.OpenXml.Office.Word;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Vectorization.Constants;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Handlers;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Resources;
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
    /// <param name="vectorizationResourceProvider">The <see cref="IResourceProviderService"/> implementing the vectorization resource provider.</param>
    /// <param name="stepsConfiguration">The <see cref="IConfigurationSection"/> object providing access to the settings.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class VectorizationService(
        IRequestSourcesCache requestSourcesCache,
        IVectorizationStateService vectorizationStateService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization)] IResourceProviderService vectorizationResourceProvider,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_Steps)] IConfigurationSection stepsConfiguration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationService
    {
        private readonly Dictionary<string, IRequestSourceService> _requestSources = requestSourcesCache.RequestSources;
        private readonly IVectorizationStateService _vectorizationStateService = vectorizationStateService;
        private readonly IResourceProviderService _vectorizationResourceProviderService = vectorizationResourceProvider;
        private readonly IConfigurationSection? _stepsConfiguration = stepsConfiguration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly ILogger<VectorizationService> _logger = loggerFactory.CreateLogger<VectorizationService>();

        /// <inheritdoc/>
        public async Task<VectorizationProcessingResult> ProcessRequest(VectorizationRequest vectorizationRequest)
        {
            try
            {
                // Pre-process the vectorization request
                vectorizationRequest.Id = Guid.NewGuid().ToString();
                vectorizationRequest.CompletedSteps = [];
                vectorizationRequest.RemainingSteps = vectorizationRequest.Steps.Select(s => s.Id).ToList();

                ValidateRequest(vectorizationRequest);

                await _vectorizationResourceProviderService.UpsertResourceAsync<VectorizationRequest>(
                    $"/{VectorizationResourceTypeNames.VectorizationRequests}/{vectorizationRequest.Id}",
                    vectorizationRequest);

                switch (vectorizationRequest.ProcessingType)
                {
                    case VectorizationProcessingType.Asynchronous:
                        var firstRequestSource = _requestSources[vectorizationRequest.Steps.First().Id];
                        await firstRequestSource.SubmitRequest(vectorizationRequest);
                        return new VectorizationProcessingResult(vectorizationRequest.ObjectId!, true, null);
                    case VectorizationProcessingType.Synchronous:
                        return await ProcessRequestInternal(vectorizationRequest);
                    default:
                        throw new VectorizationException($"The vectorization processing type {vectorizationRequest.ProcessingType} is not supported.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new VectorizationProcessingResult(vectorizationRequest.ObjectId!, false, ex.Message);
            }
        }

        private void ValidateRequest(VectorizationRequest vectorizationRequest)
        {
            if (vectorizationRequest == null)
                throw new VectorizationException("The vectorization request should not be null.");

            if (String.IsNullOrWhiteSpace(vectorizationRequest!.Id))
                throw new VectorizationException("The vectorization request id should not be null.");

            if (vectorizationRequest.ContentIdentifier == null
                || String.IsNullOrWhiteSpace(vectorizationRequest.ContentIdentifier.UniqueId)
                || String.IsNullOrWhiteSpace(vectorizationRequest.ContentIdentifier.CanonicalId))
                throw new VectorizationException("The vectorization request content identifier is invalid.");

            if (vectorizationRequest.Steps == null || vectorizationRequest.Steps.Count == 0)
                throw new VectorizationException("The list of the vectorization steps should not be empty.");

            if (vectorizationRequest.Steps!.Select(x=>x.Id).Distinct().Count() != vectorizationRequest.Steps!.Count)
                throw new VectorizationException("The list of vectorization steps must contain unique names.");

            if (vectorizationRequest.CompletedSteps != null && vectorizationRequest.CompletedSteps!.Count > 0)
                throw new VectorizationException("The completed steps of the vectorization request must be empty.");

            if (vectorizationRequest.RemainingSteps == null || vectorizationRequest.RemainingSteps.Count == 0)
                throw new VectorizationException("The list of the remaining steps of the vectorization request should not be empty.");

            // check request content source profile -> content_source type for multi-part validation
            var contentSourceProfile = _vectorizationResourceProviderService.GetResource<ContentSourceProfile>(
                $"/{VectorizationResourceTypeNames.ContentSourceProfiles}/{vectorizationRequest.ContentIdentifier.ContentSourceProfileName}");

            switch (contentSourceProfile.ContentSource)
            {
                // file-based content sources
                case ContentSourceType.AzureDataLake:
                case ContentSourceType.SharePointOnline:
                case ContentSourceType.AzureSQLDatabase:
                    // Validate the file extension is supported by vectorization
                    string fileNameExtension = Path.GetExtension(vectorizationRequest.ContentIdentifier!.FileName);
                    if (string.IsNullOrWhiteSpace(fileNameExtension))
                        throw new VectorizationException("The file does not have an extension.");

                    if (!FileExtensions.AllowedFileExtensions
                        .Select(ext => ext.ToLower())
                        .Contains(fileNameExtension.ToLower()))
                        throw new VectorizationException($"The file extension {fileNameExtension} is not supported.");
                    break;
                case ContentSourceType.Web:
                    // Validate the protocol passed in is http or https
                    string protocol = vectorizationRequest.ContentIdentifier[0];
                    if (!new[] { "http", "https" }.Contains(protocol.ToLower()))
                        throw new VectorizationException($"The protocol {protocol} is not supported.");
                    break;
                default:
                    throw new VectorizationException($"The content source type {contentSourceProfile.ContentSource} is not supported.");
            } 
        }

        private async Task<VectorizationProcessingResult> ProcessRequestInternal(VectorizationRequest request)
        {
            _logger.LogInformation("Starting synchronous processing for request {RequestId}.", request.Id);

            var state = VectorizationState.FromRequest(request);

            foreach (var step in request.Steps)
            {
                _logger.LogInformation("Starting step [{Step}] for request {RequestId}.", step.Id, request.Id);

                var stepHandler = VectorizationStepHandlerFactory.Create(
                    step.Id,
                    "N/A",
                    step.Parameters,
                    _stepsConfiguration,
                    _vectorizationStateService,
                    _serviceProvider,
                    _loggerFactory);
                var handlerSuccess = await stepHandler.Invoke(request, state, default).ConfigureAwait(false);
                if (!handlerSuccess)
                    break;

                var steps = request.MoveToNextStep();

                if (!string.IsNullOrEmpty(steps.CurrentStep))
                    _logger.LogInformation("The pipeline for request id {RequestId} was advanced from step [{PreviousStepName}] to step [{CurrentStepName}].",
                        request.Id, steps.PreviousStep, steps.CurrentStep);
                else
                    _logger.LogInformation("The pipeline for request id {RequestId} was advanced from step [{PreviousStepName}] to finalized state.",
                        request.Id, steps.PreviousStep);

                await _vectorizationStateService.SaveState(state).ConfigureAwait(false);
            }

            if (request.Complete)
            {
                _logger.LogInformation("Finished synchronous processing for request {RequestId}. All steps were processed successfully.", request.Id);
                return new VectorizationProcessingResult(request.ObjectId!, true, null);
            }
            else
            {
                var errorMessage =
                    $"Execution stopped at step [{request.CurrentStep}] due to an error.";
                _logger.LogInformation("Finished synchronous processing for request {RequestId}. {ErrorMessage}", request.Id, errorMessage);
                return new VectorizationProcessingResult(request.ObjectId!, false, errorMessage);
            }
        }
    }
}
