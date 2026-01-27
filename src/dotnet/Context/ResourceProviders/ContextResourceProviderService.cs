using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.Services;
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
                        authorizationResult,
                        userIdentity),
                ContextResourceTypeNames.KnowledgeSources =>
                    await UpdateKnowledgeResource<KnowledgeSource>(
                        resourcePath,
                        JsonSerializer.Deserialize<KnowledgeSource>(serializedResource!)!,
                        authorizationResult,
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
                    ResourceProviderActions.CheckName => await CheckKnowledgeUnitName(
                        resourcePath,
                        authorizationResult,
                        serializedAction,
                        userIdentity),
                    ResourceProviderActions.CheckVectorStoreId => await CheckVectorStoreId(
                        resourcePath,
                        authorizationResult,
                        serializedAction,
                        userIdentity),
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
                    ResourceProviderActions.CheckName => await CheckKnowledgeSourceName(
                        resourcePath,
                        authorizationResult,
                        serializedAction,
                        userIdentity),
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

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(
            ResourcePath resourcePath,
            UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case ContextResourceTypeNames.KnowledgeUnits:
                    await DeleteKnowledgeResource<KnowledgeUnit>(resourcePath, userIdentity);
                    break;
                case ContextResourceTypeNames.KnowledgeSources:
                    await DeleteKnowledgeResource<KnowledgeSource>(resourcePath, userIdentity);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            }

            await SendResourceProviderEvent(EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
        }

        /// <inheritdoc/>
        protected override Dictionary<
            string,
            (
                string ResourceProviderName,
                Func<IResourceProviderService, string, string, UnifiedUserIdentity, Task<ResourceBase?>> BuildInstanceAsync,
                 Dictionary<
                    string,
                    Func<ResourcePath, string, ResourceBase, UnifiedUserIdentity, Task<bool>>
                > InstanceValidators
            )> _parentResourceFactory => new()
            {
                {
                    AgentResourceTypeNames.Agents, // The parent resource type name (e.g., "agents")
                    (
                        // 1. The name of the resource provider that manages this parent resource type.
                        ResourceProviderNames.FoundationaLLM_Agent,
                        // 2. Async function to build/load the parent resource instance
                        async (resourceProviderService, instanceId, resourceName, userIdentity) =>
                            await resourceProviderService.GetResourceAsync<AgentBase>(
                                instanceId,
                                resourceName,
                                userIdentity),
                        // 3. Dictionary of validators: maps resource type -> validation function
                        new()
                        {
                            {
                                ContextResourceTypeNames.KnowledgeUnits,
                                async (resourcePath, actionType, parentResourceInstance, userIdentity) =>
                                {
                                    // Validate that the parent (agent) actually references this knowledge unit.
                                    if (parentResourceInstance is AgentBase agent)
                                        return
                                            agent.HasResourceReference(resourcePath.ObjectId!)
                                            && actionType == AuthorizableOperations.Read;
                                    return false;
                                }
                            },
                            {
                                ContextResourceTypeNames.KnowledgeSources,
                                async (resourcePath, actionType, parentResourceInstance, userIdentity) =>
                                {
                                    // Validate that the parent (agent) actually references this knowledge source.
                                    if (parentResourceInstance is AgentBase agent)
                                        return
                                            agent.HasResourceReference(resourcePath.ObjectId!)
                                            && actionType == AuthorizableOperations.Read;
                                    return false;
                                }
                            }
                        }
                    )
                }
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
                        authorizationResult,
                        userIdentity)) as TResult)!,
                Type t when t == typeof(KnowledgeSource) =>
                    ((await UpdateKnowledgeResource<KnowledgeSource>(
                        resourcePath,
                        (resource as KnowledgeSource)!,
                        authorizationResult,
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
                        ResourceProviderActions.CheckName =>
                            ((await CheckKnowledgeUnitName(
                                resourcePath,
                                authorizationResult,
                                (actionPayload as ResourceName)!,
                                userIdentity)) as TResult)!,
                        ResourceProviderActions.CheckVectorStoreId =>
                            ((await CheckVectorStoreId(
                                resourcePath,
                                authorizationResult,
                                (actionPayload as CheckVectorStoreIdRequest)!,
                                userIdentity)) as TResult)!,
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
                Type t when t == typeof(KnowledgeSource) =>
                    resourcePath.Action! switch
                    {
                        ResourceProviderActions.CheckName =>
                            ((await CheckKnowledgeSourceName(
                                resourcePath,
                                authorizationResult,
                                (actionPayload as ResourceName)!,
                                userIdentity)) as TResult)!,
                        _ => throw new ResourceProviderException(
                                $"The action {resourcePath.Action} on resource type {ContextResourceTypeNames.KnowledgeSources} is not supported by the {_name} resource provider.",
                                    StatusCodes.Status400BadRequest)
                    },
                _ => throw new ResourceProviderException($"Action on resource type {resourcePath.ResourceTypeName} are not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsyncInternal<T>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity) =>
            await DeleteKnowledgeResource<T>(
                resourcePath,
                userIdentity);

        #endregion

        #region Resource management

        #region Check resource names and identifiers

        private async Task<ResourceNameCheckResult> CheckKnowledgeUnitName(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            var actionPayload = JsonSerializer.Deserialize<ResourceName>(serializedAction)
                ?? throw new ResourceProviderException(
                    "The action payload is not valid.",
                    StatusCodes.Status400BadRequest);

            return await CheckKnowledgeUnitName(
                resourcePath,
                authorizationResult,
                actionPayload,
                userIdentity);
        }

        private async Task<ResourceNameCheckResult> CheckKnowledgeUnitName(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            ResourceName actionPayload,
            UnifiedUserIdentity userIdentity)
        {
            if (!authorizationResult.Authorized
                && !authorizationResult.HasRequiredRole)
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);

            if (!_proxyMode)
                return await CheckResourceName<KnowledgeUnit>(actionPayload);

            var contextServiceClient = GetContextServiceClient(userIdentity);
            var contextResponse = await contextServiceClient.CheckKnowledgeUnitName(
                resourcePath.InstanceId!,
                actionPayload);

            if (contextResponse.TryGetValue(out var resourceNameCheckResult))
                return resourceNameCheckResult;
            else
                throw new ResourceProviderException(
                    $"The following error occured when checking name for knowledge unit in the {_name} resource provider: {contextResponse.Error?.Detail ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
        }

        private async Task<ResourceNameCheckResult> CheckVectorStoreId(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            var actionPayload = JsonSerializer.Deserialize<CheckVectorStoreIdRequest>(serializedAction)
                ?? throw new ResourceProviderException("The action payload is not valid.", StatusCodes.Status400BadRequest);

            return await CheckVectorStoreId(
                resourcePath,
                authorizationResult,
                actionPayload,
                userIdentity);
        }

        private async Task<ResourceNameCheckResult> CheckVectorStoreId(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            CheckVectorStoreIdRequest actionPayload,
            UnifiedUserIdentity userIdentity)
        {
            if (!authorizationResult.Authorized
                && !authorizationResult.HasRequiredRole)
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);

            if (!_proxyMode)
            {
                // Load all knowledge unit resource references and check if the vector store ID is already in use.
                var knowledgeUnitReferences = await _resourceReferenceStore!.GetAllResourceReferences<KnowledgeUnit>();

                foreach (var reference in knowledgeUnitReferences)
                {
                    var knowledgeUnit = await LoadResource<KnowledgeUnit>(reference);
                    if (knowledgeUnit is not null
                        && knowledgeUnit.VectorDatabaseObjectId == actionPayload.VectorDataBaseObjectId
                        && knowledgeUnit.VectorStoreId == actionPayload.VectorStoreId)
                    {
                        return new ResourceNameCheckResult(
                            actionPayload.VectorStoreId,
                            ContextTypes.KnowledgeUnit,
                            NameCheckResultType.Denied,
                            true,
                            false,
                            $"The vector store ID '{actionPayload.VectorStoreId}' is already in use by knowledge unit '{knowledgeUnit.Name}'.");
                    }
                }

                return new ResourceNameCheckResult(
                    actionPayload.VectorStoreId,
                    ContextTypes.KnowledgeUnit,
                    NameCheckResultType.Allowed,
                    false,
                    false);
            }

            var contextServiceClient = GetContextServiceClient(userIdentity);
            var contextResponse = await contextServiceClient.CheckVectorStoreId(
                resourcePath.InstanceId!,
                actionPayload);

            if (contextResponse.TryGetValue(out var resourceNameCheckResult))
                return resourceNameCheckResult;
            else
                throw new ResourceProviderException(
                    $"The following error occured when checking vector store identifier in the {_name} resource provider: {contextResponse.Error?.Detail ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
        }

        private async Task<ResourceNameCheckResult> CheckKnowledgeSourceName(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            var actionPayload = JsonSerializer.Deserialize<ResourceName>(serializedAction)
                ?? throw new ResourceProviderException(
                    "The action payload is not valid.",
                    StatusCodes.Status400BadRequest);

            return await CheckKnowledgeSourceName(
                resourcePath,
                authorizationResult,
                actionPayload,
                userIdentity);
        }

        private async Task<ResourceNameCheckResult> CheckKnowledgeSourceName(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            ResourceName actionPayload,
            UnifiedUserIdentity userIdentity)
        {
            if (!authorizationResult.Authorized
                && !authorizationResult.HasRequiredRole)
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);

            if (!_proxyMode)
                return await CheckResourceName<KnowledgeSource>(actionPayload);

            var contextServiceClient = GetContextServiceClient(userIdentity);
            var contextResponse = await contextServiceClient.CheckKnowledgeSourceName(
                resourcePath.InstanceId!,
                actionPayload);

            if (contextResponse.TryGetValue(out var resourceNameCheckResult))
                return resourceNameCheckResult;
            else
                throw new ResourceProviderException(
                    $"The following error occured when checking name for knowledge source in the {_name} resource provider: {contextResponse.Error?.Detail ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
        }

        #endregion

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

            if (contextResponse.TryGetValue(out var resourceProviderResult))
                return resourceProviderResult.Resource;
            else
                throw new ResourceProviderException(
                    $"The following error occured when retrieving the knowledge source {resourcePath.MainResourceId!} from the {_name} resource provider: {contextResponse.Error?.Detail ?? "N/A"}.",
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

            if (contextResponse.TryGetValue(out var resourceProviderResult))
                return resourceProviderResult.Resource;
            else
                throw new ResourceProviderException(
                    $"The following error occured when retrieving the knowledge unit {resourcePath.MainResourceId!} from the {_name} resource provider: {contextResponse.Error?.Detail ?? "N/A"}.",
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
            Result<IEnumerable<ResourceProviderGetResult<KnowledgeSource>>> contextResponse = null!;
            if (!resourcePath.IsResourceTypePath)
            {
                contextResponse = await contextServiceClient!.GetKnowledgeSources(
                    resourcePath.InstanceId!,
                    [resourcePath.MainResourceId!],
                    options: options);
            }
            else
            {
                contextResponse = authorizationResult.Authorized
                    ? await contextServiceClient!.GetKnowledgeSources(
                        resourcePath.InstanceId!,
                        options: options)
                    : await contextServiceClient!.GetKnowledgeSources(
                        resourcePath.InstanceId!,
                        authorizationResult.SubordinateResourcePathsAuthorizationResults.Values
                                   .Where(sarp => !string.IsNullOrWhiteSpace(sarp.ResourceName) && sarp.Authorized)
                                   .Select(sarp => sarp.ResourceName!)
                                   .ToList(),
                        options: options);
            }
            if (contextResponse.TryGetValue(out var resourceProviderResults))
                return [..
                        resourceProviderResults
                ];
            else
                throw new ResourceProviderException(
                    $"The following error occured when retrieving knowledge sources from the {_name} resource provider: {contextResponse.Error?.Detail ?? "N/A"}.",
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
            Result<IEnumerable<ResourceProviderGetResult<KnowledgeUnit>>> contextResponse = null!;
            if (!resourcePath.IsResourceTypePath)
            {
                contextResponse = await contextServiceClient!.GetKnowledgeUnits(
                    resourcePath.InstanceId!,
                    [resourcePath.MainResourceId!],
                    options: options);
            }
            else
            {
                contextResponse = authorizationResult.Authorized
                    ? await contextServiceClient!.GetKnowledgeUnits(
                        resourcePath.InstanceId!,
                        options: options)
                    : await contextServiceClient!.GetKnowledgeUnits(
                        resourcePath.InstanceId!,
                        authorizationResult.SubordinateResourcePathsAuthorizationResults.Values
                                   .Where(sarp => !string.IsNullOrWhiteSpace(sarp.ResourceName) && sarp.Authorized)
                                   .Select(sarp => sarp.ResourceName!)
                                   .ToList(),
                        options: options);
            }

            if (contextResponse.TryGetValue(out var resourceProviderResults))
                return [..
                        resourceProviderResults
                ];
            else
                throw new ResourceProviderException(
                    $"The following error occured when retrieving knowledge unit from the {_name} resource provider: {contextResponse.Error?.Detail ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
        }

        private async Task<ResourceProviderUpsertResult<T>> UpdateKnowledgeResource<T>(
            ResourcePath resourcePath,
            T resource,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity) where T : ResourceBase
        {
            if (!_proxyMode)
            {
                var existingResourceReference =
                    await _resourceReferenceStore!.GetResourceReference(resource.Name);

                if (existingResourceReference is not null
                    && !authorizationResult.Authorized)
                {
                    // The resource already exists and the user is not authorized to update it.
                    // Irrespective of whether the user has the required role or not, we need to throw an exception in the case of existing resources.
                    // The required role only allows the user to create a new resource.
                    // This check is needed because it's only here that we can determine if the resource exists.
                    _logger.LogWarning("Access to the resource path {ResourcePath} was not authorized for user {UserName} : userId {UserId}.",
                        resourcePath.RawResourcePath, userIdentity!.Username, userIdentity!.UserId);
                    throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);
                }

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
                    if (knowledgeUnitResult.TryGetValue(out var resourceProviderResultKnowledgeUnit))
                        return (resourceProviderResultKnowledgeUnit as ResourceProviderUpsertResult<T>)!;
                    throw new ResourceProviderException(
                        $"The following error occured when upserting knowledge unit {knowledgeUnit.Name} in the {_name} resource provider: {knowledgeUnitResult.Error?.Detail ?? "N/A"}.",
                        StatusCodes.Status500InternalServerError);

                case KnowledgeSource knowledgeSource:
                    var knowledgeSourceResult = await contextServiceClient!.UpsertKnowledgeSource(
                        resourcePath.InstanceId!,
                        knowledgeSource);
                    if (knowledgeSourceResult.TryGetValue(out var resourceProviderResultKnowledgeSource))
                        return (resourceProviderResultKnowledgeSource as ResourceProviderUpsertResult<T>)!;
                    throw new ResourceProviderException(
                        $"The following error occured when upserting knowledge source {knowledgeSource.Name} in the {_name} resource provider: {knowledgeSourceResult.Error?.Detail ?? "N/A"}.",
                        StatusCodes.Status500InternalServerError);

                default:
                    throw new ResourceProviderException(
                        $"The resource type {resource.GetType().Name} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest);
            };
        }

        private async Task DeleteKnowledgeResource<T>(
            ResourcePath resourcePath,
            UnifiedUserIdentity userIdentity) where T : ResourceBase
        {
            if (!_proxyMode)
            {
                //TODO: Add checks to prevent deletion if resource is in use
                await DeleteResource<T>(resourcePath);
                return;
            }

            var contextServiceClient = GetContextServiceClient(userIdentity);
            switch (typeof(T))
            {
                case Type t when t == typeof(KnowledgeUnit):
                    var knowledgeUnitResult = await contextServiceClient.DeleteKnowledgeUnit(
                        resourcePath.InstanceId!,
                        resourcePath.MainResourceId!);
                    if (knowledgeUnitResult.IsSuccess)
                        return;
                    throw new ResourceProviderException(
                        $"The following error occured when deleting knowledge unit {resourcePath.MainResourceId!} in the {_name} resource provider: {knowledgeUnitResult.Error?.Detail ?? "N/A"}.",
                        StatusCodes.Status500InternalServerError);
                case Type t when t == typeof(KnowledgeSource):
                    var knowledgeSourceResult = await contextServiceClient.DeleteKnowledgeSource(
                        resourcePath.InstanceId!,
                        resourcePath.MainResourceId!);
                    if (knowledgeSourceResult.IsSuccess)
                        return;
                    throw new ResourceProviderException(
                        $"The following error occured when deleting knowledge source {resourcePath.MainResourceId!} in the {_name} resource provider: {knowledgeSourceResult.Error?.Detail ?? "N/A"}.",
                        StatusCodes.Status500InternalServerError);
                default:
                    throw new ResourceProviderException(
                        $"The resource type {typeof(T).Name} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest);
            }
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
            if (actionResult.TryGetValue(out var resourceProviderResult))
                return resourceProviderResult;

            _logger.LogError(
                "The following error occured when setting the knowledge graph for knowledge unit {KnowledgeUnitName} in the {ResourceProviderName} resource provider: {ErrorMessage}",
                resourcePath.MainResourceId,
                _name,
                actionResult.Error?.Detail ?? "N/A");
            return new ResourceProviderActionResult(
                resourcePath.ObjectId!,
                false,
                actionResult.Error?.Detail);
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

            if (response.TryGetValue(out var queryResponse))
                return queryResponse;
            else
                throw new ResourceProviderException(
                    $"The following error occured when querying the knowledge source {resourcePath.MainResourceId!} from the {_name} resource provider: {response.Error?.Detail ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
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

            if (response.TryGetValue(out var renderResponse))
                return renderResponse;
            else
                throw new ResourceProviderException(
                    $"The following error occured when rendering the knowledge graph for knowledge unit {resourcePath.MainResourceId!} from the {_name} resource provider: {response.Error?.Detail ?? "N/A"}.",
                    StatusCodes.Status500InternalServerError);
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
