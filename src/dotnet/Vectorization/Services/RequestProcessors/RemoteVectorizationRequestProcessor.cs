using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Client;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Vectorization.Services.RequestProcessors
{
    /// <summary>
    /// Processes the vectorization request remotely using the <see cref="IVectorizationServiceClient"/> over HTTP.
    /// </summary>
    /// <param name="httpClientFactoryService">The factory service responsible for HTTP connections.</param>
    /// <param name="vectorizationServiceSettings">The settings for the vectorization service.</param>
    /// <param name="loggerFactory">The logger factory responsible for creating loggers.</param>
    public class RemoteVectorizationRequestProcessor(
        IHttpClientFactoryService httpClientFactoryService,
        IOptions<VectorizationServiceSettings> vectorizationServiceSettings,
        ILoggerFactory loggerFactory) : IVectorizationRequestProcessor
    {
        /// <inheritdoc/>
        public async Task<VectorizationResult> ProcessRequest(VectorizationRequest vectorizationRequest, UnifiedUserIdentity? userIdentity)
        {
            var vectorizationServiceClient = new VectorizationServiceClient(
                httpClientFactoryService,
                vectorizationServiceSettings!,
                loggerFactory.CreateLogger<VectorizationServiceClient>());
            return await vectorizationServiceClient.ProcessRequest(vectorizationRequest, userIdentity);
        }
    }
}
