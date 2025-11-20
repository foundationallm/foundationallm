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
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parquet.Serialization;
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
        : ResourceProviderServiceBase<ContextReference>(
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

        /// <inheritdoc/>
        protected override async Task InitializeInternal() => await Task.CompletedTask;

        private const string KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME = "entities.parquet";
        private const string KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME = "relationships.parquet";

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
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>
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
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>
            resourcePath.ResourceTypeName switch
            {
                ContextResourceTypeNames.KnowledgeUnits => resourcePath.Action switch
                {
                    ResourceProviderActions.SetGraph => await SetKnowledgeUnitGraph(
                        resourcePath,
                        authorizationResult,
                        JsonSerializer.Deserialize<ContextKnowledgeUnitSetGraphRequest>(serializedAction)!,
                        userIdentity),
                    ResourceProviderActions.RenderGraph => await RenderKnowledgeUnitGraph(
                        resourcePath,
                        authorizationResult,
                        serializedAction,
                        userIdentity),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.")
                },
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

        #region Resource provider strongly typed operations

        protected override async Task<T> GetResourceAsyncInternal<T>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null,
            ResourceBase? parentResourceInstance = null) =>
            resourcePath.MainResourceTypeName switch
            {
                ContextResourceTypeNames.KnowledgeUnits =>
                    (await GetKnowledgeUnit(
                        resourcePath,
                        authorizationResult,
                        userIdentity,
                        options,
                        parentResourceInstance)) as T
                    ?? throw new ResourceProviderException(
                        $"The knowledge unit {resourcePath.MainResourceId} could not be loaded.",
                        StatusCodes.Status404NotFound),
                ContextResourceTypeNames.KnowledgeSources =>
                    (await GetKnowledgeSource(
                        resourcePath,
                        authorizationResult,
                        userIdentity,
                        options,
                        parentResourceInstance)) as T
                    ?? throw new ResourceProviderException(
                        $"The knowledge source {resourcePath.MainResourceId} could not be loaded.",
                        StatusCodes.Status404NotFound),
                _ =>
                    throw new ResourceProviderException(
                        $"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            T resource,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null) =>
            typeof(T) switch
            {
                Type t when t == typeof(KnowledgeUnit) =>
                    ((await UpdateKnowledgeResource<KnowledgeUnit>(
                        resourcePath,
                        (resource as KnowledgeUnit)!,
                        userIdentity)) as TResult)!,
                Type t when t == typeof(KnowledgeSource) =>
                    ((await UpdateKnowledgeResource<KnowledgeSource>(
                        resourcePath,
                        (resource as KnowledgeSource)!,
                        userIdentity)) as TResult)!,
                _ => throw new ResourceProviderException(
                    $"The resource type {typeof(T).Name} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<TResult> ExecuteResourceActionAsyncInternal<T, TAction, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            TAction actionPayload,
            UnifiedUserIdentity userIdentity) =>
            typeof(T) switch
            {
                Type t when t == typeof(KnowledgeUnit) =>
                    resourcePath.Action! switch
                    {
                        ResourceProviderActions.SetGraph =>
                            ((await SetKnowledgeUnitGraph(
                                resourcePath,
                                authorizationResult,
                                (actionPayload as ContextKnowledgeUnitSetGraphRequest)!,
                                userIdentity)) as TResult)!,
                        ResourceProviderActions.LoadGraph =>
                            ((await LoadKnowledgeUnitGraph(
                                resourcePath,
                                authorizationResult,
                                userIdentity)) as TResult)!,
                        _ => throw new ResourceProviderException(
                                $"The action {resourcePath.Action} on resource type {ContextResourceTypeNames.KnowledgeUnits} is not supported by the {_name} resource provider.",
                                    StatusCodes.Status400BadRequest)
                    },
                _ => throw new ResourceProviderException($"Action on resource type {resourcePath.ResourceTypeName} are not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #endregion

        #region Resource management

        private async Task<KnowledgeSource> GetKnowledgeSource(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options,
            ResourceBase? parentResourceInstance = null)
        {
            if (!_proxyMode)
                return (await LoadResource<KnowledgeSource>(
                    resourcePath.MainResourceId!))!;

            var contextServiceClient = GetContextServiceClient(userIdentity);
            var contextResponse = await contextServiceClient.GetKnowledgeSource(
                resourcePath.InstanceId!,
                resourcePath.MainResourceId!,
                parentResourceInstance is AgentBase
                    ? parentResourceInstance.Name
                    : null);

            if (contextResponse.IsSuccess)
                return contextResponse.Result!.Resource;
            else
                throw new ResourceProviderException(
                    $"The following error occured when retrieving the knowledge source {resourcePath.MainResourceId!} from the {_name} resource provider: {contextResponse.ErrorMessage ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
        }
        private async Task<KnowledgeUnit> GetKnowledgeUnit(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options,
            ResourceBase? parentResourceInstance = null)
        {
            if (!_proxyMode)
                return (await LoadResource<KnowledgeUnit>(
                    resourcePath.MainResourceId!))!;

            var contextServiceClient = GetContextServiceClient(userIdentity);
            var contextResponse = await contextServiceClient.GetKnowledgeUnit(
                resourcePath.InstanceId!,
                resourcePath.MainResourceId!,
                parentResourceInstance is AgentBase
                    ? parentResourceInstance.Name
                    : null);

            if (contextResponse.IsSuccess)
                return contextResponse.Result!.Resource;
            else
                throw new ResourceProviderException(
                    $"The following error occured when retrieving the knowledge unit {resourcePath.MainResourceId!} from the {_name} resource provider: {contextResponse.ErrorMessage ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
        }


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
            if (contextResponse.IsSuccess)
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

            if (contextResponse.IsSuccess)
                return [..
                        contextResponse.Result!
                ];
            else
                throw new ResourceProviderException(
                    $"The following error occured when retrieving knowledge unit from the {_name} resource provider: {contextResponse.ErrorMessage ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
        }

        private async Task<ResourceProviderUpsertResult<T>> UpdateKnowledgeResource<T>(
            ResourcePath resourcePath,
            T resource,
            UnifiedUserIdentity userIdentity) where T : ResourceBase
        {
            if (!_proxyMode)
            {
                var existingResourceReference =
                    await _resourceReferenceStore!.GetResourceReference(resource.Name);

                var resourceReference = new ContextReference
                {
                    Name = resource.Name,
                    Type = resource.Type!,
                    Filename = $"{_storageRootPath ?? string.Empty}/{_name}/{resource.Name}.json",
                    Deleted = false
                };

                if (string.IsNullOrWhiteSpace(resource.ObjectId))
                    resource.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);
                UpdateBaseProperties(resource, userIdentity, isNew: existingResourceReference is null);

                if (existingResourceReference is null)
                    await CreateResource<T>(resourceReference, resource);
                else
                    await SaveResource<T>(existingResourceReference, resource);

                return new ResourceProviderUpsertResult<T>
                {
                    ObjectId = resource.ObjectId!,
                    ResourceExists = existingResourceReference is not null,
                    Resource = resource
                };
            }

            var contextServiceClient = GetContextServiceClient(userIdentity);
            switch (resource)
            {
                case KnowledgeUnit knowledgeUnit:
                    var knowledgeUnitResult = await contextServiceClient!.UpsertKnowledgeUnit(
                        resourcePath.InstanceId!,
                        knowledgeUnit);
                    if (knowledgeUnitResult.IsSuccess)
                        return (knowledgeUnitResult.Result! as ResourceProviderUpsertResult<T>)!;
                    throw new ResourceProviderException(
                        $"The following error occured when upserting knowledge unit {knowledgeUnit.Name} in the {_name} resource provider: {knowledgeUnitResult.ErrorMessage ?? "N/A"}.",
                        StatusCodes.Status500InternalServerError);

                case KnowledgeSource knowledgeSource:
                    var knowledgeSourceResult = await contextServiceClient!.UpsertKnowledgeSource(
                        resourcePath.InstanceId!,
                        knowledgeSource);
                    if (knowledgeSourceResult.IsSuccess)
                        return (knowledgeSourceResult.Result! as ResourceProviderUpsertResult<T>)!;
                    throw new ResourceProviderException(
                        $"The following error occured when upserting knowledge source {knowledgeSource.Name} in the {_name} resource provider: {knowledgeSourceResult.ErrorMessage ?? "N/A"}.",
                        StatusCodes.Status500InternalServerError);

                default:
                    throw new ResourceProviderException(
                        $"The resource type {resource.GetType().Name} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest);
            };
        }

        private async Task<ResourceProviderActionResult> SetKnowledgeUnitGraph(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            ContextKnowledgeUnitSetGraphRequest actionPayload,
            UnifiedUserIdentity userIdentity)
        {
            if (!_proxyMode)
            {
                await _storageService.CopyFileAsync(
                    resourcePath.InstanceId!,
                    actionPayload.EntitiesSourceFilePath,
                    string.Join('/',
                        [
                            _storageRootPath ?? string.Empty,
                            _name,
                            resourcePath.MainResourceId,
                            KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                        ]));

                await _storageService.CopyFileAsync(
                    resourcePath.InstanceId!,
                    actionPayload.RelationshipsSourceFilePath!,
                    string.Join('/',
                        [
                            _storageRootPath ?? string.Empty,
                            _name,
                            resourcePath.MainResourceId,
                            KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                        ]));

                return new ResourceProviderActionResult(
                    resourcePath.ObjectId!,
                    true);
            }

            var contextServiceClient = GetContextServiceClient(userIdentity);

            var actionResult = await contextServiceClient!.SetKnowledgeUnitGraph(
                resourcePath.InstanceId!,
                resourcePath.MainResourceId!,
                actionPayload);
            if (actionResult.IsSuccess)
                return actionResult.Result!;

            _logger.LogError(
                "The following error occured when setting the knowledge graph for knowledge unit {KnowledgeUnitName} in the {ResourceProviderName} resource provider: {ErrorMessage}",
                resourcePath.MainResourceId,
                _name,
                actionResult.ErrorMessage ?? "N/A");
            return new ResourceProviderActionResult(
                resourcePath.ObjectId!,
                false,
                actionResult.ErrorMessage);
        }

        private async Task<ResourceProviderActionResult<KnowledgeGraph>> LoadKnowledgeUnitGraph(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity)
        {
            if (_proxyMode)
                throw new ResourceProviderException(
                    $"The action {ResourceProviderActions.LoadGraph} is not supported by the {_name} resource provider in proxy mode.",
                    StatusCodes.Status400BadRequest);

            var entitiesBinaryContent = await _storageService.ReadFileAsync(
                resourcePath.InstanceId!,
                string.Join('/',
                    [
                        _storageRootPath ?? string.Empty,
                        _name,
                        resourcePath.MainResourceId,
                        KNOWLEDGE_GRAPH_ENTITIES_FILE_NAME
                    ]),
                default);

            var entities = await ParquetSerializer.DeserializeAsync<KnowledgeEntity>(
                entitiesBinaryContent.ToStream());

            var relationshipsBinaryContent = await _storageService.ReadFileAsync(
                resourcePath.InstanceId!,
                string.Join('/',
                    [
                        _storageRootPath ?? string.Empty,
                        _name,
                        resourcePath.MainResourceId,
                        KNOWLEDGE_GRAPH_RELATIONSHIPS_FILE_NAME
                    ]),
                default);

            var relationships = await ParquetSerializer.DeserializeAsync<KnowledgeRelationship>(
                relationshipsBinaryContent.ToStream());

            var graph = new KnowledgeGraph
            {
                Name = resourcePath.MainResourceId!,
                Entities = [.. entities],
                Relationships = [.. relationships]
            };

            return new ResourceProviderActionResult<KnowledgeGraph>(
                resourcePath.ObjectId!,
                true)
            {
                Resource = graph
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

        private async Task<ContextKnowledgeUnitRenderGraphResponse> RenderKnowledgeUnitGraph(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            //var queryRequest = JsonSerializer.Deserialize<ContextKnowledgeSourceQueryRequest>(serializedAction)
            //    ?? throw new ResourceProviderException("The query request is not valid.", StatusCodes.Status400BadRequest);

            var contextServiceClient = GetContextServiceClient(userIdentity);

            var response = await contextServiceClient!.RenderKnowledgeUnitGraph(
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
