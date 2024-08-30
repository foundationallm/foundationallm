using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
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
    /// <param name="resourceProviderServices">The collection of registered resource providers.</param>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> providing dependency injection services.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class TextEmbeddingServiceFactory(
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationServiceFactory<ITextEmbeddingService>
    {       
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);

        /// <inheritdoc/>
        public async Task<ITextEmbeddingService> GetService(string serviceName, UnifiedUserIdentity userIdentity)
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

            var textEmbeddingProfile = await vectorizationResourceProviderService.GetResource<TextEmbeddingProfile>(
                $"/{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{serviceName}", userIdentity);

            return textEmbeddingProfile.TextEmbedding switch
            {                
                TextEmbeddingType.GatewayTextEmbedding => CreateGatewayTextEmbeddingService(),
                _ => throw new VectorizationException($"The text embedding type {textEmbeddingProfile.TextEmbedding} is not supported."),
            };
        }

        /// <inheritdoc/>
        public async Task<(ITextEmbeddingService Service, ResourceBase Resource)> GetServiceWithResource(string serviceName, UnifiedUserIdentity userIdentity)
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

            var textEmbeddingProfile = await vectorizationResourceProviderService.GetResource<TextEmbeddingProfile>(
                $"/{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{serviceName}", userIdentity);

            return textEmbeddingProfile.TextEmbedding switch
            {                
                TextEmbeddingType.GatewayTextEmbedding => (CreateGatewayTextEmbeddingService(), textEmbeddingProfile),
                _ => throw new VectorizationException($"The text embedding type {textEmbeddingProfile.TextEmbedding} is not supported."),
            };
        }

        private ITextEmbeddingService CreateGatewayTextEmbeddingService()
        {
            using var scope = _serviceProvider.CreateScope();
            var textEmbeddingService = scope.ServiceProvider.GetKeyedService<ITextEmbeddingService>(
                DependencyInjectionKeys.FoundationaLLM_Vectorization_TextEmbedding_Gateway)
                ?? throw new VectorizationException($"Could not retrieve the Gateway text embedding service instance.");

            return textEmbeddingService!;
        } 
    }
}
