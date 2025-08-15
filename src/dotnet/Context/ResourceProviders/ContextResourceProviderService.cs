using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
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
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The factory responsible for creating loggers.</param>
    /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
    public class ContextResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        bool proxyMode = false)
        : ResourceProviderServiceBase<ResourceReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<ContextResourceProviderService>(),
            [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: true,
            proxyMode: proxyMode)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            ContextResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Context;

        /// <inheritdoc/>
        protected override string _storageContainerName => _instanceSettings.Id;

        /// <inheritdoc/>
        protected override string _storageRootPath => "/resource-provider";

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
                    userIdentity,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
                ContextResourceTypeNames.KnowledgeUnits => await GetKnowledgeUnits(
                    resourcePath,
                    authorizationResult,
                    userIdentity,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
                _ => throw new ResourceProviderException(
                        $"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
            };

        protected override async Task<object> UpsertResourceAsync(
            ResourcePath resourcePath,
            string? serializedResource,
            ResourceProviderFormFile? formFile,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                ContextResourceTypeNames.KnowledgeUnits =>
                    await UpdateKnowledgeResource<KnowledgeUnit>(
                        resourcePath,
                        JsonSerializer.Deserialize<KnowledgeUnit>(serializedResource!)!,
                        userIdentity),
                ContextResourceTypeNames.KnowledgeSources =>
                    await UpdateKnowledgeResource<KnowledgeSource>(
                        resourcePath,
                        JsonSerializer.Deserialize<KnowledgeSource>(serializedResource!)!,
                        userIdentity),
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
                    ResourceProviderActions.RenderGraph => await RenderKnowledgeSourceGraph(
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
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions options)
        {
            if (!_proxyMode)
                return await LoadResources<KnowledgeSource>(
                    resourcePath.ResourceTypeInstances.First(),
                    authorizationResult,
                    options);

            var contextServiceClient = GetContextServiceClient(userIdentity);
            ContextServiceResponse<IEnumerable<ResourceProviderGetResult<KnowledgeSource>>> contextResponse = null!;
            if (!resourcePath.IsResourceTypePath)
            {
                contextResponse = await contextServiceClient!.GetKnowledgeSources(
                    resourcePath.InstanceId!,
                    [resourcePath.MainResourceId!]);
            }
            else
            {
                contextResponse = authorizationResult.Authorized
                    ? await contextServiceClient!.GetKnowledgeSources(
                        resourcePath.InstanceId!)
                    : await contextServiceClient!.GetKnowledgeSources(
                        resourcePath.InstanceId!,
                        authorizationResult.SubordinateResourcePathsAuthorizationResults.Values
                                   .Where(sarp => !string.IsNullOrWhiteSpace(sarp.ResourceName) && sarp.Authorized)
                                   .Select(sarp => sarp.ResourceName!)
                                   .ToList());
            }
            if (contextResponse.Success)
                return [..
                        contextResponse.Result!
                ];
            else
                throw new ResourceProviderException(
                    $"The following error occured when retrieving knowledge sources from the {_name} resource provider: {contextResponse.ErrorMessage ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
        }

        private async Task<List<ResourceProviderGetResult<KnowledgeUnit>>> GetKnowledgeUnits(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions options)
        {
            if (!_proxyMode)
                return await LoadResources<KnowledgeUnit>(
                    resourcePath.ResourceTypeInstances.First(),
                    authorizationResult,
                    options);

            var contextServiceClient = GetContextServiceClient(userIdentity);
            ContextServiceResponse<IEnumerable<ResourceProviderGetResult<KnowledgeUnit>>> contextResponse = null!;
            if (!resourcePath.IsResourceTypePath)
            {
                contextResponse = await contextServiceClient!.GetKnowledgeUnits(
                    resourcePath.InstanceId!,
                    [resourcePath.MainResourceId!]);
            }
            else
            {
                contextResponse = authorizationResult.Authorized
                    ? await contextServiceClient!.GetKnowledgeUnits(
                        resourcePath.InstanceId!)
                    : await contextServiceClient!.GetKnowledgeUnits(
                        resourcePath.InstanceId!,
                        authorizationResult.SubordinateResourcePathsAuthorizationResults.Values
                                   .Where(sarp => !string.IsNullOrWhiteSpace(sarp.ResourceName) && sarp.Authorized)
                                   .Select(sarp => sarp.ResourceName!)
                                   .ToList());
            }

            if (contextResponse.Success)
                return [..
                        contextResponse.Result!
                ];
            else
                throw new ResourceProviderException(
                    $"The following error occured when retrieving knowledge unit from the {_name} resource provider: {contextResponse.ErrorMessage ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
        }

        private async Task<ResourceProviderUpsertResult> UpdateKnowledgeResource<T>(
            ResourcePath resourcePath,
            T resource,
            UnifiedUserIdentity userIdentity) where T : ResourceBase
        {
            if (!_proxyMode)
                return await UpdateResource<T>(
                    resourcePath,
                    resource,
                    userIdentity.NormalizedName);

            var contextServiceClient = GetContextServiceClient(userIdentity);
            switch (resource)
            {
                case KnowledgeUnit knowledgeUnit:
                    var knowledgeUnitResult = await contextServiceClient!.UpsertKnowledgeUnit(
                        resourcePath.InstanceId!,
                        knowledgeUnit);
                    if (knowledgeUnitResult.Success)
                        return (ResourceProviderUpsertResult)knowledgeUnitResult.Result!;
                    throw new ResourceProviderException(
                        $"The following error occured when upserting knowledge unit {knowledgeUnit.Name} in the {_name} resource provider: {knowledgeUnitResult.ErrorMessage ?? "N/A"}.",
                        StatusCodes.Status500InternalServerError);

                case KnowledgeSource knowledgeSource:
                    var knowledgeSourceResult = await contextServiceClient!.UpsertKnowledgeSource(
                        resourcePath.InstanceId!,
                        knowledgeSource);
                    if (knowledgeSourceResult.Success)
                        return (ResourceProviderUpsertResult)knowledgeSourceResult.Result!;
                    throw new ResourceProviderException(
                        $"The following error occured when upserting knowledge source {knowledgeSource.Name} in the {_name} resource provider: {knowledgeSourceResult.ErrorMessage ?? "N/A"}.",
                        StatusCodes.Status500InternalServerError);

                default:
                    throw new ResourceProviderException(
                        $"The resource type {resource.GetType().Name} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest);
            };
        }

        private async Task<ContextKnowledgeSourceQueryResponse> QueryKnowledgeSource(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            var queryRequest = JsonSerializer.Deserialize<ContextKnowledgeSourceQueryRequest>(serializedAction)
                ?? throw new ResourceProviderException("The query request is not valid.", StatusCodes.Status400BadRequest);

            var contextServiceClient = GetContextServiceClient(userIdentity);
            var response = await contextServiceClient!.QueryKnowledgeSource(
                resourcePath.InstanceId!,
                resourcePath.MainResourceId!,
                queryRequest);

            return response;
        }

        private async Task<ContextKnowledgeSourceRenderGraphResponse> RenderKnowledgeSourceGraph(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            //var queryRequest = JsonSerializer.Deserialize<ContextKnowledgeSourceQueryRequest>(serializedAction)
            //    ?? throw new ResourceProviderException("The query request is not valid.", StatusCodes.Status400BadRequest);

            var contextServiceClient = GetContextServiceClient(userIdentity);

            var response = await contextServiceClient!.RenderKnowledgeSourceGraph(
                resourcePath.InstanceId!,
                resourcePath.MainResourceId!,
                null);

            return response;
        }

        #endregion

        #region Utils

        private IContextServiceClient GetContextServiceClient(
            UnifiedUserIdentity userIdentity) =>
            new ContextServiceClient(
                new OrchestrationContext { CurrentUserIdentity = userIdentity },
                _serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                _serviceProvider.GetRequiredService<ILogger<ContextServiceClient>>());

        #endregion
    }
}
