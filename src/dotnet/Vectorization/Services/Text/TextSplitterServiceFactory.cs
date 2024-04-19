using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Text;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Services.TextSplitters;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Vectorization.Services.Text
{
    /// <summary>
    /// Creates text splitter service instances.
    /// </summary>
    /// <param name="vectorizationResourceProviderService">The vectorization resource provider service.</param>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> providing dependency injection services.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class TextSplitterServiceFactory(
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization)] IResourceProviderService vectorizationResourceProviderService,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationServiceFactory<ITextSplitterService>
    {
        private readonly IResourceProviderService _vectorizationResourceProviderService = vectorizationResourceProviderService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        /// <inheritdoc/>
        public ITextSplitterService GetService(string serviceName)
        {
            var textPartitionProfile = _vectorizationResourceProviderService.GetResource<TextPartitioningProfile>(
                $"/{VectorizationResourceTypeNames.TextPartitioningProfiles}/{serviceName}");

            return textPartitionProfile.TextSplitter switch
            {
                TextSplitterType.TokenTextSplitter => CreateTokenTextSplitterService(
                    TokenTextSplitterServiceSettings.FromDictionary(textPartitionProfile.Settings!)),
                _ => throw new VectorizationException($"The text splitter type {textPartitionProfile.TextSplitter} is not supported."),
            };
        }

        /// <inheritdoc/>
        public (ITextSplitterService Service, ResourceBase Resource) GetServiceWithResource(string serviceName)
        {
            var textPartitionProfile = _vectorizationResourceProviderService.GetResource<TextPartitioningProfile>(
                $"/{VectorizationResourceTypeNames.TextPartitioningProfiles}/{serviceName}");

            return textPartitionProfile.TextSplitter switch
            {
                TextSplitterType.TokenTextSplitter => (CreateTokenTextSplitterService(
                    TokenTextSplitterServiceSettings.FromDictionary(textPartitionProfile.Settings!)), textPartitionProfile),
                _ => throw new VectorizationException($"The text splitter type {textPartitionProfile.TextSplitter} is not supported."),
            };
        }

        private TokenTextSplitterService CreateTokenTextSplitterService(TokenTextSplitterServiceSettings settings)
        {
            var tokenizerService = _serviceProvider.GetKeyedService<ITokenizerService>(settings.Tokenizer)
                ?? throw new VectorizationException($"Could not retrieve the {settings.Tokenizer} tokenizer service instance.");

            return new TokenTextSplitterService(
                tokenizerService,
                Options.Create<TokenTextSplitterServiceSettings>(settings),
                _loggerFactory.CreateLogger<TokenTextSplitterService>());
        }
    }
}
