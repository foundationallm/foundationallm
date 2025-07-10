using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Context.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Context resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>    
    /// <param name="cacheOptions">The options providing the <see cref="ResourceProviderCacheSettings"/> with settings for the resource provider cache.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The factory responsible for creating loggers.</param>    
    public class ContextResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
        : ResourceProviderServiceBase<ResourceReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            null!,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<ContextResourceProviderService>(),
            [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: false)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            ContextResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Context;

        /// <inheritdoc/>
        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            resourcePath.ResourceTypeName switch
            {
                ContextResourceTypeNames.KnowledgeSources => await GetKnowledgeSources(
                    resourcePath,
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
            };


        /// <inheritdoc/>
        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                ContextResourceTypeNames.KnowledgeSources => resourcePath.Action switch
                {
                    ResourceProviderActions.Query => await QueryKnowledgeSource(
                        resourcePath,
                        authorizationResult,
                        serializedAction,
                        userIdentity),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider..")
                },
                _ => throw new ResourceProviderException(
                    $"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #endregion

        #region Resource management

        private async Task<List<ResourceProviderGetResult<KnowledgeSource>>> GetKnowledgeSources(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            ResourceProviderGetOptions options)
        {
            if (!resourcePath.IsResourceTypePath)
                throw new ResourceProviderException(
                    "The operation is not supported", StatusCodes.Status400BadRequest);

            using var scope = _serviceProvider.CreateScope();
            var contextServiceClient = new ContextServiceClient(
                new OrchestrationContext { CurrentUserIdentity = ServiceContext.ServiceIdentity },
                scope.ServiceProvider.GetRequiredService<IHttpClientFactoryService>(),
                scope.ServiceProvider.GetRequiredService<ILogger<ContextServiceClient>>());

            var response = authorizationResult.Authorized
                ? await contextServiceClient.GetKnowledgeSources(
                    resourcePath.InstanceId!)
                : await contextServiceClient.GetKnowledgeSources(
                    resourcePath.InstanceId!,
                    authorizationResult.SubordinateResourcePathsAuthorizationResults.Values
                               .Where(sarp => !string.IsNullOrWhiteSpace(sarp.ResourceName) && sarp.Authorized)
                               .Select(sarp => sarp.ResourceName!)
                               .ToList());

            return [.. response.Select(ks => new ResourceProviderGetResult<KnowledgeSource>
            {
                Resource = ks,
                Actions = [],
                Roles = []
            })];
        }

        private async Task<ContextKnowledgeSourceQueryResponse> QueryKnowledgeSource(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            var queryRequest = JsonSerializer.Deserialize<ContextKnowledgeSourceQueryRequest>(serializedAction)
                ?? throw new ResourceProviderException("The query request is not valid.", StatusCodes.Status400BadRequest);

            using var scope = _serviceProvider.CreateScope();

            var contextServiceClient = new ContextServiceClient(
                new OrchestrationContext { CurrentUserIdentity = ServiceContext.ServiceIdentity },
                scope.ServiceProvider.GetRequiredService<IHttpClientFactoryService>(),
                scope.ServiceProvider.GetRequiredService<ILogger<ContextServiceClient>>());

            var response = await contextServiceClient.QueryKnowledgeSource(
                resourcePath.InstanceId!,
                resourcePath.MainResourceId!,
                queryRequest);

            return response;
        }

        #endregion
    }
}
