using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.Text
{
    /// <summary>
    /// Creates text splitter service instances.
    /// </summary>
    /// <param name="vectorizationResourceProviderService">The vectorization resource provider service.</param>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> providing dependency injection services.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class TextEmbeddingServiceFactory(
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization)] IResourceProviderService vectorizationResourceProviderService,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationServiceFactory<ITextEmbeddingService>
    {
        private readonly IResourceProviderService _vectorizationResourceProviderService = vectorizationResourceProviderService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        /// <inheritdoc/>
        public ITextEmbeddingService GetService(string serviceName)
        {
            var textEmbeddingProfile = _vectorizationResourceProviderService.GetResource<TextEmbeddingProfile>(
                $"/{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{serviceName}");

            return textEmbeddingProfile.TextEmbedding switch
            {
                TextEmbeddingType.SemanticKernelTextEmbedding => CreateSemanticKernelTextEmbeddingService(),
                TextEmbeddingType.GatewayTextEmbedding => CreateGatewayTextEmbeddingService(),
                _ => throw new VectorizationException($"The text embedding type {textEmbeddingProfile.TextEmbedding} is not supported."),
            };
        }

        /// <inheritdoc/>
        public (ITextEmbeddingService Service, ResourceBase Resource) GetServiceWithResource(string serviceName)
        {
            var textEmbeddingProfile = _vectorizationResourceProviderService.GetResource<TextEmbeddingProfile>(
                $"/{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{serviceName}");

            return textEmbeddingProfile.TextEmbedding switch
            {
                TextEmbeddingType.SemanticKernelTextEmbedding => (CreateSemanticKernelTextEmbeddingService(), textEmbeddingProfile),
                TextEmbeddingType.GatewayTextEmbedding => (CreateGatewayTextEmbeddingService(), textEmbeddingProfile),
                _ => throw new VectorizationException($"The text embedding type {textEmbeddingProfile.TextEmbedding} is not supported."),
            };
        }

        private ITextEmbeddingService CreateSemanticKernelTextEmbeddingService()
        {
            var textEmbeddingService = _serviceProvider.GetKeyedService<ITextEmbeddingService>(
                DependencyInjectionKeys.FoundationaLLM_Vectorization_SemanticKernelTextEmbeddingService)
                ?? throw new VectorizationException($"Could not retrieve the Semantic Kernel text embedding service instance.");

            return textEmbeddingService!;
        }

        private ITextEmbeddingService CreateGatewayTextEmbeddingService()
        {
            using var scope = _serviceProvider.CreateScope();
            var textEmbeddingService = scope.ServiceProvider.GetKeyedService<ITextEmbeddingService>(
                DependencyInjectionKeys.FoundationaLLM_Vectorization_GatewayTextEmbeddingService)
                ?? throw new VectorizationException($"Could not retrieve the Gateway text embedding service instance.");

            return textEmbeddingService!;
        }
    }
}
