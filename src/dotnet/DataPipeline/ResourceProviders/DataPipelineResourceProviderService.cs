using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.DataPipeline.Interfaces;
using FoundationaLLM.DataPipeline.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.DataPipeline.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.DataPipeline resource provider.
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
    public class DataPipelineResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline_Storage)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        bool proxyMode = false)
        : ResourceProviderServiceBase<DataPipelineReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<DataPipelineResourceProviderService>(),
            [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand,
                EventTypes.FoundationaLLM_ResourceProvider_State_ExportCommand
            ],
            useInternalReferencesStore: true,
            proxyMode: proxyMode)
    {
        private IDataPipelineServiceClient _dataPipelineServiceClient = null!;

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            DataPipelineResourceProviderMetadata.AllowedResourceTypes;

        protected override string _name => ResourceProviderNames.FoundationaLLM_DataPipeline;

        protected override async Task InitializeInternal()
        {
            // Defer resolving to this point to avoid circular dependency issues.
            _dataPipelineServiceClient = _serviceProvider.GetRequiredService<IDataPipelineServiceClient>();

            var pluginResourceProvider = _serviceProvider.GetRequiredService<IEnumerable<IResourceProviderService>>()
                .SingleOrDefault(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Plugin);

            List<IResourceProviderService> resourceProviders = pluginResourceProvider != null
                ? [pluginResourceProvider, this]
                : [this];

            (_dataPipelineServiceClient as IResourceProviderClient)!.ResourceProviders = resourceProviders;

            await Task.CompletedTask;
        }

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            resourcePath.ResourceTypeName switch
            {
                DataPipelineResourceTypeNames.DataPipelines => await LoadResources<DataPipelineDefinition>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
                DataPipelineResourceTypeNames.DataPipelineRuns => await GetDataPipelineRun(
                    resourcePath,
                    authorizationResult,
                    userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
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
                DataPipelineResourceTypeNames.DataPipelines => await UpdateDataPipeline(
                    resourcePath,
                    serializedResource!,
                    userIdentity),
                DataPipelineResourceTypeNames.DataPipelineRuns => await CreateDataPipelineRun(
                    resourcePath,
                    JsonSerializer.Deserialize<DataPipelineRun>(serializedResource!)
                        ?? throw new ResourceProviderException("CreateDataPipelineRun returned an unexpected null result.",
                            StatusCodes.Status400BadRequest),
                    userIdentity),
                _ => throw new ResourceProviderException(
                    $"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>
            resourcePath.ResourceTypeName switch
            {
                SharedResourceTypeNames.Management => resourcePath.Action switch
                {
                    ResourceProviderActions.TriggerCommand => await ExecuteManagementAction(
                        resourcePath,
                        authorizationResult,
                        serializedAction),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                DataPipelineResourceTypeNames.DataPipelines =>
                    resourcePath.Action switch
                    {
                        ResourceProviderActions.Trigger => (await CreateDataPipelineRun(
                            resourcePath,
                            DataPipelineRun.FromTriggerRequest(
                                JsonSerializer.Deserialize<DataPipelineTriggerRequest>(serializedAction)
                                    ?? throw new ResourceProviderException("The data pipeline trigger request is invalid.",
                                        StatusCodes.Status400BadRequest),
                                userIdentity),
                            userIdentity)).ToActionResult(), 
                        _ => throw new ResourceProviderException(
                            $"The action {resourcePath.Action} on resource type {DataPipelineResourceTypeNames.DataPipelines} is not supported by the {_name} resource provider.",
                                StatusCodes.Status400BadRequest)
                    },
                DataPipelineResourceTypeNames.DataPipelineRuns =>
                    resourcePath.Action switch
                    {
                        ResourceProviderActions.Filter =>
                            await GetDataPipelineRuns(
                                resourcePath,
                                authorizationResult,
                                () =>
                                {
                                    var dataPipelineRunFilter = JsonSerializer.Deserialize<DataPipelineRunFilter>(serializedAction)
                                        ?? throw new ResourceProviderException("The data pipeline run filter definition is invalid.",
                                            StatusCodes.Status400BadRequest);

                                    if (dataPipelineRunFilter.InstanceId != null
                                        || dataPipelineRunFilter.DataPipelineName != null)
                                        throw new ResourceProviderException("The data pipeline run filter definition is invalid.",
                                            StatusCodes.Status400BadRequest);

                                    dataPipelineRunFilter.InstanceId = resourcePath.InstanceId;
                                    dataPipelineRunFilter.DataPipelineName = resourcePath.MainResourceId;
                                    return dataPipelineRunFilter;
                                },
                                userIdentity),
                        _ => throw new ResourceProviderException(
                            $"The action {resourcePath.Action} on resource type {DataPipelineResourceTypeNames.DataPipelineRuns} is not supported by the {_name} resource provider.",
                                StatusCodes.Status400BadRequest)
                    },
                _ => throw new ResourceProviderException($"Actions on resource type {resourcePath.ResourceTypeName} are not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<List<ResourceProviderGetResult<DataPipelineRun>>> GetDataPipelineRun(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity)
        {
            var dataPipelineRun = await _dataPipelineServiceClient.GetDataPipelineRunAsync(
                resourcePath.InstanceId!,
                resourcePath.MainResourceId!,
                resourcePath.ResourceId!,
                userIdentity)
                ?? throw new ResourceProviderException("The data pipeline run resource could not be loaded.",
                    StatusCodes.Status404NotFound);

            return [
                new ResourceProviderGetResult<DataPipelineRun>
                {
                    Resource = dataPipelineRun,
                    Roles = [],
                    Actions = []
                }
            ];
        }

        private async Task<ResourceProviderActionResult<DataPipelineRunFilterResponse>> GetDataPipelineRuns(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            Func<DataPipelineRunFilter> getDataPipelineRunFilter,
            UnifiedUserIdentity userIdentity)
        {
            // Using this approach as the validation of the incoming filter depends on the inbound route of the call.
            // If the request is coming via the Management API, then the filter must not have and instance identifier and a
            // data pipeline name set.
            // If the request is coming via a strongly typed operation, then the filter must have the instance identifier
            // and a data pipeline name set.
            var dataPipelineRunFilter = getDataPipelineRunFilter();

            if (resourcePath.InstanceId != dataPipelineRunFilter.InstanceId
                || resourcePath.MainResourceId != dataPipelineRunFilter.DataPipelineName
                || (
                    resourcePath.MainResourceId != "all"
                    && dataPipelineRunFilter.DataPipelineNameFilter != resourcePath.MainResourceId
                ))
                throw new ResourceProviderException("The data pipeline run filter definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var result = await _dataPipelineServiceClient.GetDataPipelineRunsAsync(
                resourcePath.InstanceId!,
                dataPipelineRunFilter,
                userIdentity);

            return new ResourceProviderActionResult<DataPipelineRunFilterResponse>(
                string.Empty,
                true)
            {
                Resource = result
            };
        }

        private async Task<ResourceProviderUpsertResult> UpdateDataPipeline(
            ResourcePath resourcePath,
            string serializedDataPipeline,
            UnifiedUserIdentity userIdentity)
        {
            var dataPipeline = JsonSerializer.Deserialize<DataPipelineDefinition>(serializedDataPipeline)
                ?? throw new ResourceProviderException("The serialized data pipeline object could not be deserialized.",
                StatusCodes.Status400BadRequest);

            var existingDataPipelineReference = await _resourceReferenceStore!.GetResourceReference(dataPipeline.Name);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != dataPipeline.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                        StatusCodes.Status400BadRequest);

            var dataPipelineReference = new DataPipelineReference
            {
                Name = dataPipeline.Name!,
                Type = dataPipeline.Type!,
                Filename = $"/{_name}/{dataPipeline.Name}.json",
                Deleted = false
            };

            dataPipeline.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var validator = _resourceValidatorFactory.GetValidator(dataPipelineReference.ResourceType);
            if (validator is IValidator dataPipelineValidator)
            {
                var context = new ValidationContext<object>(dataPipeline);
                var validationResult = await dataPipelineValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
            }

            UpdateBaseProperties(dataPipeline, userIdentity, isNew: existingDataPipelineReference is null);

            var snapshotName =
                $"{dataPipeline.Name}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToBase64String()}";
            var snapshotObjectId =
                ResourcePath.Join(dataPipeline.ObjectId, $"{DataPipelineResourceTypeNames.DataPipelineSnapshots}/{snapshotName}");
            dataPipeline.MostRecentSnapshotObjectId = snapshotObjectId;

            if (existingDataPipelineReference is null)
                await CreateResource<DataPipelineDefinition>(dataPipelineReference, dataPipeline);
            else
                await SaveResource<DataPipelineDefinition>(dataPipelineReference, dataPipeline);

            #region Create a snapshot of the data pipeline every time it is updated

            var dataPipelineSnapshotReference = new DataPipelineReference
            {
                Name = snapshotName,
                Type = DataPipelineTypes.DataPipelineSnapshot,
                Filename = $"/{_name}/{dataPipeline.Name}/{snapshotName}.json"
            };

            // Using SaveResourcr to avoid polluting the resource reference store with snapshots.
            await SaveResource<DataPipelineDefinitionSnapshot>(
                dataPipelineSnapshotReference,
                new DataPipelineDefinitionSnapshot
                {
                    Name = snapshotName,
                    DataPipelineDefinition = dataPipeline,
                    ObjectId = snapshotObjectId,
                    CreatedOn = DateTimeOffset.UtcNow,
                    CreatedBy = userIdentity.UPN!
                });

            #endregion

            return new ResourceProviderUpsertResult
            {
                ObjectId = dataPipeline!.ObjectId,
                ResourceExists = existingDataPipelineReference is not null
            };
        }

        #endregion

        #endregion

        #region Resource provider strongly typed operations

        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            T resource,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null) =>
            typeof(T) switch
            {
                Type t when t == typeof(DataPipelineRun) => (await CreateDataPipelineRun(
                    resourcePath,
                    (resource as DataPipelineRun)!,
                    userIdentity) as TResult)!,
                _ => throw new ResourceProviderException(
                        $"The resource type {typeof(T).Name} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest)
            };

        protected override async Task<T> GetResourceAsyncInternal<T>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null,
            ResourceBase? parentResourceInstance = null) =>
            typeof(T) switch
            {
                Type t when t == typeof(DataPipelineDefinition) =>
                    await LoadResource<DataPipelineDefinition>(resourcePath.MainResourceId!) as T
                        ?? throw new ResourceProviderException($"LoadResource returned an unexpected null object for {resourcePath.MainResourceId!}.",
                            StatusCodes.Status400BadRequest),
                Type t when t == typeof(DataPipelineDefinitionSnapshot) =>
                    await LoadDataPipelineSnapshot(
                        resourcePath,
                        authorizationResult,
                        userIdentity) as T
                        ?? throw new ResourceProviderException($"LoadDataPipelineSnapshot returned an unexpected null object for {resourcePath.MainResourceId!}.",
                            StatusCodes.Status400BadRequest),
                Type t when t == typeof(DataPipelineRun) =>
                    await _dataPipelineServiceClient.GetDataPipelineRunAsync(
                        resourcePath.InstanceId!,
                        resourcePath.MainResourceId!,
                        resourcePath.ResourceId!,
                        userIdentity) as T
                        ?? throw new ResourceProviderException("The data pipeline run resource could not be loaded.",
                            StatusCodes.Status404NotFound),
                _ => throw new ResourceProviderException(
                        $"The resource type {typeof(T).Name} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest)
            };

        protected override async Task<TResult> ExecuteResourceActionAsyncInternal<T, TAction, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            TAction actionPayload, UnifiedUserIdentity userIdentity) =>
            typeof(T) switch
            {
                Type t when t == typeof(DataPipelineRun) =>
                    resourcePath.Action! switch
                    {
                        ResourceProviderActions.Filter =>
                            (await GetDataPipelineRuns(
                                resourcePath,
                                authorizationResult,
                                () => actionPayload as DataPipelineRunFilter
                                    ?? throw new ResourceProviderException("The data pipeline run filter definition is invalid.",
                                        StatusCodes.Status400BadRequest),
                                userIdentity) as TResult)!,
                        _ => throw new ResourceProviderException(
                            $"The action {resourcePath.Action} on resource type {DataPipelineResourceTypeNames.DataPipelineRuns} is not supported by the {_name} resource provider.",
                                StatusCodes.Status400BadRequest)
                    },
                _ => throw new ResourceProviderException($"Action on resource type {resourcePath.ResourceTypeName} are not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        private async Task<ResourceProviderUpsertResult<DataPipelineRun>> CreateDataPipelineRun(
            ResourcePath resourcePath,
            DataPipelineRun dataPipelineRun,
            UnifiedUserIdentity userIdentity)
        {
            var dataPipelineSnapshot = await GetResourceAsync<DataPipelineDefinitionSnapshot>(
                $"{dataPipelineRun.DataPipelineObjectId}/{DataPipelineResourceTypeNames.DataPipelineSnapshots}/latest",
                userIdentity);

            var result = await _dataPipelineServiceClient.CreateDataPipelineRunAsync(
                resourcePath.InstanceId!,
                dataPipelineRun,
                dataPipelineSnapshot,
                userIdentity)
                ?? throw new ResourceProviderException("The Data Pipeline client was not able to create the data pipeline run.",
                    StatusCodes.Status400BadRequest);

            return new ResourceProviderUpsertResult<DataPipelineRun>
            {
                ObjectId = result.ObjectId!,
                Resource = result,
                ResourceExists = false
            };
        }

        private async Task<DataPipelineDefinitionSnapshot> LoadDataPipelineSnapshot(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity)
        {
            var finalResourcePath = resourcePath;

            if (resourcePath.ResourceId! == "latest")
            {
                var dataPipeline = await LoadResource<DataPipelineDefinition>(resourcePath.ParentResourceId!);
                if (dataPipeline!.MostRecentSnapshotObjectId is null)
                    throw new ResourceProviderException("The object identifier is invalid.",
                        StatusCodes.Status400BadRequest);
                if (!ResourcePath.TryParse(
                    dataPipeline.MostRecentSnapshotObjectId,
                    [_name],
                    DataPipelineResourceProviderMetadata.AllowedResourceTypes,
                    false,
                    out finalResourcePath))
                    throw new ResourceProviderException($"The {dataPipeline.Name} has an invalid snapshot object identifier.",
                        StatusCodes.Status400BadRequest);
            }

            var dataPipelineSnapshotReference = new DataPipelineReference
            {
                Name = finalResourcePath!.ResourceId!,
                Type = DataPipelineTypes.DataPipelineSnapshot,
                Filename = $"/{_name}/{resourcePath.ParentResourceId}/{finalResourcePath.ResourceId!}.json"
            };

            return (await LoadResource<DataPipelineDefinitionSnapshot>(
                dataPipelineSnapshotReference))!;
        }

        #endregion

        #region Event handling

        /// <inheritdoc/>
        protected override async Task<BinaryData> GetResourceProviderState() =>
            await _dataPipelineServiceClient.GetServiceStateAsync();

        #endregion
    }
}
