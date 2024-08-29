using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Common.Services.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Conversation.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Conversation resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="cosmosDBService">The <see cref="ICosmosDBService"/> providing Cosmos DB services.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class ConversationResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        ICosmosDBService cosmosDBService,
        IServiceProvider serviceProvider,
        ILogger<ConversationResourceProviderService> logger)
        : ResourceProviderServiceBase<ResourceReference>(
            instanceOptions.Value,
            authorizationService,
            new NullStorageService(),
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            logger,
            eventNamespacesToSubscribe: null,
            useInternalStore: false)
    {
        private readonly ICosmosDBService _cosmosDBService = cosmosDBService;
    }
}
