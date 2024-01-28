using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
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
    /// <param name="logger">The logger instance used for logging.</param>
    public class VectorizationService(
        IRequestSourcesCache requestSourcesCache,
        ILogger<VectorizationService> logger) : IVectorizationService
    {
        private readonly Dictionary<string, IRequestSourceService> _requestSources = requestSourcesCache.RequestSources;
        private readonly ILogger<VectorizationService> _logger = logger;

        /// <inheritdoc/>
        public async Task ProcessRequest(VectorizationRequest vectorizationRequest)
        {
            ValidateRequest(vectorizationRequest);

            var firstRequestSource = _requestSources[vectorizationRequest.Steps.First().Id];
            await firstRequestSource.SubmitRequest(vectorizationRequest);
        }

        private void ValidateRequest(VectorizationRequest vectorizationRequest)
        {
            if (vectorizationRequest == null)
                HandleValidationError("The vectorization request should not be null.");

            if (String.IsNullOrWhiteSpace(vectorizationRequest!.Id))
                HandleValidationError("The vectorization request id should not be null.");

            if (vectorizationRequest.ContentIdentifier == null
                || String.IsNullOrWhiteSpace(vectorizationRequest.ContentIdentifier.UniqueId)
                || String.IsNullOrWhiteSpace(vectorizationRequest.ContentIdentifier.CanonicalId))
                HandleValidationError("The vectorization request content identifier is invalid.");

            if (vectorizationRequest.Steps == null || vectorizationRequest.Steps.Count == 0)
                HandleValidationError("The list of the vectorization steps should not be empty.");

            if (vectorizationRequest.Steps!.Select(x=>x.Id).Distinct().Count() != vectorizationRequest.Steps!.Count)
                HandleValidationError("The list of vectorization steps must contain unique names.");

            if (vectorizationRequest.CompletedSteps != null && vectorizationRequest.CompletedSteps!.Count > 0)
                HandleValidationError("The completed steps of the vectorization request must be empty.");

            if (vectorizationRequest.RemainingSteps == null || vectorizationRequest.RemainingSteps.Count == 0)
                HandleValidationError("The list of the remaining steps of the vectorization request should not be empty.");
        }

        private void HandleValidationError(string validationError)
        {
            _logger.LogError(validationError);
            throw new VectorizationException(validationError);
        }
    }
}
