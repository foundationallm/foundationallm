using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Client;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.RequestProcessors
{
    /// <summary>
    /// Processes the vectorization request remotely using the <see cref="IVectorizationServiceClient"/> over HTTP.
    /// </summary>
    /// <param name="httpClientFactoryService">The factory service responsible for HTTP connections.</param>
    /// <param name="loggerFactory">The logger factory responsible for creating loggers.</param>
    public class RemoteVectorizationRequestProcessor(
        IHttpClientFactoryService httpClientFactoryService,
        ILoggerFactory loggerFactory) : IVectorizationRequestProcessor
    {
        /// <inheritdoc/>
        public async Task<VectorizationResult> ProcessRequest(string instanceId, VectorizationRequest vectorizationRequest, UnifiedUserIdentity? userIdentity)
        {
            var vectorizationServiceClient = new VectorizationServiceClient(
                httpClientFactoryService,
                loggerFactory.CreateLogger<VectorizationServiceClient>());
            return await vectorizationServiceClient.ProcessRequest(instanceId, vectorizationRequest, userIdentity);
        }
    }
}
