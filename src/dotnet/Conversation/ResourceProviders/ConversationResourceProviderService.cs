using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Chat;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using ConversationModels = FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Common.Services.Storage;
using Microsoft.AspNetCore.Http;
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
    /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> providing Cosmos DB services.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class ConversationResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IAzureCosmosDBService cosmosDBService,
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
            useInternalReferencesStore: false)
    {
        private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;

        /// <inheritdoc />
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            ConversationResourceProviderMetadata.AllowedResourceTypes;

        protected override string _name => ResourceProviderNames.FoundationaLLM_Conversation;

        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        // This resource provider does not support the Management API.

        #endregion

        #region Resource provider strongly typed operations

        protected override async Task<object> GetResourcesAsync(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity, ResourceProviderLoadOptions? options = null)
        {
            var policyDefinition = EnsureValidatePolicyDefinitions(resourcePath, authorizationResult);

            var result = await _cosmosDBService.GetSessionsAsync(
                ConversationTypes.Session,
                userIdentity.UPN!);

            return result.Select(r => new ResourceProviderGetResult<ConversationModels.Conversation>
            {
                Resource = r,
                Actions = [],
                Roles = []
            });
        }

        #endregion

        #region Utils

        private PolicyDefinition EnsureValidatePolicyDefinitions(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult)
        {
            if (authorizationResult.PolicyDefinitionIds.Count == 0)
                throw new ResourceProviderException(
                    $"The {_name} resource provider requires PBAC policy assignments to load the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status500InternalServerError);

            if (authorizationResult.PolicyDefinitionIds.Count > 1)
                throw new ResourceProviderException(
                    $"The {_name} resource provider requires exactly one PBAC policy assignment to load the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status500InternalServerError);

            if (!PolicyDefinitions.All.TryGetValue(authorizationResult.PolicyDefinitionIds[0], out var policyDefinition))
                throw new ResourceProviderException(
                    $"The {_name} resource provider did not find the PBAC policy with id {authorizationResult.PolicyDefinitionIds[0]} required to load the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status500InternalServerError);

            var userIdentityProperties = policyDefinition.MatchingStrategy?.UserIdentityProperties ?? [];
            if (userIdentityProperties.Count != 1
                || userIdentityProperties[0] != UserIdentityPropertyNames.UserPrincipalName)
                throw new ResourceProviderException(
                    $"The {_name} resource provider requires one PBAC policy assignment with a matching strategy based on the user principal name (UPN) to load the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status500InternalServerError);

            return policyDefinition;
        }

        #endregion
    }
}
