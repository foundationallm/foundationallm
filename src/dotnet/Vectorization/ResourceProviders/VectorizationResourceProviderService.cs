using Azure.Messaging;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Vectorization.Models.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Vectorization.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class VectorizationResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILogger<VectorizationResourceProviderService> logger)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            logger,
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Vectorization
            ])
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            VectorizationResourceProviderMetadata.AllowedResourceTypes;

        private readonly ConcurrentDictionary<string, VectorizationProfileBase> _textPartitioningProfiles = [];
        private readonly ConcurrentDictionary<string, VectorizationProfileBase> _textEmbeddingProfiles = [];
        private readonly ConcurrentDictionary<string, VectorizationProfileBase> _indexingProfiles = [];
        private readonly ConcurrentDictionary<string, VectorizationPipeline> _pipelines = [];

        private string _defaultTextPartitioningProfileName = string.Empty;
        private string _defaultTextEmbeddingProfileName = string.Empty;
        private string _defaultIndexingProfileName = string.Empty;

        private const string TEXT_PARTITIONING_PROFILES_FILE_NAME = "vectorization-text-partitioning-profiles.json";
        private const string TEXT_EMBEDDING_PROFILES_FILE_NAME = "vectorization-text-embedding-profiles.json";
        private const string INDEXING_PROFILES_FILE_NAME = "vectorization-indexing-profiles.json";
        private const string PIPELINES_FILE_NAME = "vectorization-pipelines.json";

        private const string TEXT_PARTITIONING_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{TEXT_PARTITIONING_PROFILES_FILE_NAME}";
        private const string TEXT_EMBEDDING_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{TEXT_EMBEDDING_PROFILES_FILE_NAME}";
        private const string INDEXING_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{INDEXING_PROFILES_FILE_NAME}";
        private const string PIPELINES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{PIPELINES_FILE_NAME}";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Vectorization;

        #region Initialization

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            _defaultTextPartitioningProfileName =
                await LoadResourceStore<TextPartitioningProfile, VectorizationProfileBase>(TEXT_PARTITIONING_PROFILES_FILE_PATH, _textPartitioningProfiles);
            _defaultTextEmbeddingProfileName =
                await LoadResourceStore<TextEmbeddingProfile, VectorizationProfileBase>(TEXT_EMBEDDING_PROFILES_FILE_PATH, _textEmbeddingProfiles);
            _defaultIndexingProfileName =
                await LoadResourceStore<IndexingProfile, VectorizationProfileBase>(INDEXING_PROFILES_FILE_PATH, _indexingProfiles);
            _ = await LoadResourceStore<VectorizationPipeline, VectorizationPipeline>(PIPELINES_FILE_PATH, _pipelines);

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        #region Helpers for initialization

        private async Task<string> LoadResourceStore<T, TBase>(string resourceStoreFilePath, ConcurrentDictionary<string,
            TBase> resources)
            where T: TBase
            where TBase: ResourceBase
        {
            var defaultResourceName = string.Empty;
            if (await _storageService.FileExistsAsync(_storageContainerName, resourceStoreFilePath, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, resourceStoreFilePath, default);
                var resourceStore = JsonSerializer.Deserialize<ResourceStore<T>>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));
                if (resourceStore != null)
                {
                    foreach (var resource in resourceStore.Resources)
                        resources.AddOrUpdate(resource.Name, resource, (k, v) => v);
                    defaultResourceName = resourceStore.DefaultResourceName ?? string.Empty;
                }
            }
            return defaultResourceName;
        }

        #endregion

        #endregion

        #region Support for Management API

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> GetResourcesAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.TextPartitioningProfiles =>
                    LoadResources<TextPartitioningProfile, VectorizationProfileBase>(resourcePath.ResourceTypeInstances[0], _textPartitioningProfiles),
                VectorizationResourceTypeNames.TextEmbeddingProfiles =>
                    LoadResources<TextEmbeddingProfile, VectorizationProfileBase>(resourcePath.ResourceTypeInstances[0], _textEmbeddingProfiles),
                VectorizationResourceTypeNames.IndexingProfiles =>
                    LoadResources<IndexingProfile, VectorizationProfileBase>(resourcePath.ResourceTypeInstances[0], _indexingProfiles),
                VectorizationResourceTypeNames.VectorizationPipelines =>
                    LoadResources<VectorizationPipeline, VectorizationPipeline>(resourcePath.ResourceTypeInstances[0], _pipelines),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for GetResourcesAsyncInternal

        private List<TBase> LoadResources<T, TBase>(ResourceTypeInstance instance, ConcurrentDictionary<string, TBase> resourceStore)
            where T : TBase
            where TBase: ResourceBase
        {
            if (instance.ResourceId == null)
            {
                return
                    [.. resourceStore.Values
                            .Where(p => !p.Deleted)
                    ];
            }
            else
            {
                if (!resourceStore.TryGetValue(instance.ResourceId, out var resource)
                    || resource.Deleted)
                    throw new ResourceProviderException($"Could not locate the {instance.ResourceId} vectorization resource.",
                        StatusCodes.Status404NotFound);

                return [resource];
            }
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.TextPartitioningProfiles =>
                    await UpdateResource<TextPartitioningProfile, VectorizationProfileBase>(resourcePath, serializedResource, _textPartitioningProfiles, TEXT_PARTITIONING_PROFILES_FILE_PATH),
                VectorizationResourceTypeNames.TextEmbeddingProfiles =>
                    await UpdateResource<TextEmbeddingProfile, VectorizationProfileBase>(resourcePath, serializedResource, _textEmbeddingProfiles, TEXT_EMBEDDING_PROFILES_FILE_PATH),
                VectorizationResourceTypeNames.IndexingProfiles =>
                    await UpdateResource<IndexingProfile, VectorizationProfileBase>(resourcePath, serializedResource, _indexingProfiles, INDEXING_PROFILES_FILE_PATH),
                VectorizationResourceTypeNames.VectorizationPipelines =>
                    await UpdateResource<VectorizationPipeline, VectorizationPipeline>(resourcePath, serializedResource, _pipelines, PIPELINES_FILE_PATH),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateResource<T, TBase>(ResourcePath resourcePath, string serializedResource, ConcurrentDictionary<string, TBase> resourceStore, string storagePath)
            where T : TBase
            where TBase: ResourceBase
        {
            var resource = JsonSerializer.Deserialize<T>(serializedResource)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (resourceStore.TryGetValue(resource.Name, out var existingResource)
                && existingResource!.Deleted)
                throw new ResourceProviderException($"The resource {existingResource.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            resource.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var validator = _resourceValidatorFactory.GetValidator<T>();
            if (validator != null)
            {
                var validationResult = await validator.ValidateAsync(resource);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            if (resourcePath.ResourceTypeInstances[0].ResourceId != resource.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            resourceStore.AddOrUpdate(resource.Name, resource, (k,v) => resource);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    storagePath,
                    JsonSerializer.Serialize(ResourceStore<TBase>.FromDictionary(resourceStore.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = resource.ObjectId
            };
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances.Last().ResourceType switch
            {
                VectorizationResourceTypeNames.IndexingProfiles => resourcePath.ResourceTypeInstances.Last().Action switch
                {
                    VectorizationResourceProviderActions.CheckName => CheckProfileName<IndexingProfile>(serializedAction, _indexingProfiles),
                    VectorizationResourceProviderActions.Filter => Filter<IndexingProfile>(serializedAction, _indexingProfiles, _defaultIndexingProfileName),
                    _ => throw new ResourceProviderException($"The action {resourcePath.ResourceTypeInstances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                VectorizationResourceTypeNames.VectorizationPipelines => resourcePath.ResourceTypeInstances.Last().Action switch
                {
                    VectorizationResourceProviderActions.Activate => await SetPipelineActivation(resourcePath.ResourceTypeInstances.Last().ResourceId!, true),
                    VectorizationResourceProviderActions.Deactivate => await SetPipelineActivation(resourcePath.ResourceTypeInstances.Last().ResourceId!, false),
                    _ => throw new ResourceProviderException($"The action {resourcePath.ResourceTypeInstances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} does not support actions in the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for ExecuteActionAsync

        private async Task<VectorizationResult> SetPipelineActivation(string pipelineName, bool active)
        {
            if (!_pipelines.TryGetValue(pipelineName, out var existingPipeline))
                throw new ResourceProviderException($"The resource {pipelineName} was not found.",
                    StatusCodes.Status404NotFound);

            if (existingPipeline.Deleted)
                throw new ResourceProviderException($"The resource {existingPipeline.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            existingPipeline.Active = active;

            // TODO: add the logic to activate the pipeline.

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PIPELINES_FILE_PATH,
                    JsonSerializer.Serialize(ResourceStore<VectorizationPipeline>.FromDictionary(_pipelines.ToDictionary())),
                    default,
                    default);

            return new VectorizationResult(
                existingPipeline.ObjectId!,
                true,
                null);
        }

        private ResourceNameCheckResult CheckProfileName<T>(string serializedAction, ConcurrentDictionary<string, VectorizationProfileBase> profileStore)
            where T : VectorizationProfileBase
        {
            var resourceName = JsonSerializer.Deserialize<ResourceName>(serializedAction);
            return profileStore.Values.Any(p => p.Name == resourceName!.Name)
                ? new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Denied,
                    Message = "A resource with the specified name already exists or was previously deleted and not purged."
                }
                : new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Allowed
                };
        }

        private List<VectorizationProfileBase> Filter<T>(string serializedAction, ConcurrentDictionary<string,
            VectorizationProfileBase> profileStore, string defaultProfileName) where T : VectorizationProfileBase
        {
            var resourceFilter = JsonSerializer.Deserialize<ResourceFilter>(serializedAction) ??
                                 throw new ResourceProviderException("The object definition is invalid. Please provide a resource filter.",
                                     StatusCodes.Status400BadRequest);
            if (resourceFilter.Default.HasValue)
            {
                if (resourceFilter.Default.Value)
                {
                    if (string.IsNullOrWhiteSpace(defaultProfileName))
                        throw new ResourceProviderException("The default profile name is not set.",
                            StatusCodes.Status404NotFound);

                    if (!profileStore.TryGetValue(defaultProfileName, out var profile)
                        || profile.Deleted)
                        throw new ResourceProviderException(
                            $"Could not locate the {defaultProfileName} profile resource.",
                            StatusCodes.Status404NotFound);

                    return [profile];
                }
                else
                {
                    return
                    [
                        .. profileStore.Values
                                .Where(dsr => !dsr.Deleted && (
                                    string.IsNullOrWhiteSpace(defaultProfileName) ||
                                    !dsr.Name.Equals(defaultProfileName, StringComparison.OrdinalIgnoreCase)))
                    ];
                }
            }
            else
            {
                // TODO: Apply other filters.
                return
                [
                    .. profileStore.Values
                            .Where(dsr => !dsr.Deleted)
                ];
            }
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case VectorizationResourceTypeNames.TextPartitioningProfiles:
                    await DeleteResource<TextPartitioningProfile, VectorizationProfileBase>(resourcePath, _textPartitioningProfiles, TEXT_PARTITIONING_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.TextEmbeddingProfiles:
                    await DeleteResource<TextEmbeddingProfile, VectorizationProfileBase>(resourcePath, _textEmbeddingProfiles, TEXT_EMBEDDING_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.IndexingProfiles:
                    await DeleteResource<IndexingProfile, VectorizationProfileBase>(resourcePath, _indexingProfiles, INDEXING_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.VectorizationPipelines:
                    await DeleteResource<VectorizationPipeline, VectorizationPipeline>(resourcePath, _pipelines, PIPELINES_FILE_PATH);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeleteResource<T, TBase>(ResourcePath resourcePath, ConcurrentDictionary<string, TBase> resourceStore, string storagePath)
            where T : TBase
            where TBase : ResourceBase
        {
            if (resourceStore.TryGetValue(resourcePath.ResourceTypeInstances[0].ResourceId!, out var resource)
                || resource!.Deleted)
            {
                resource.Deleted = true;

                await _storageService.WriteFileAsync(
                        _storageContainerName,
                        storagePath,
                        JsonSerializer.Serialize(ResourceStore<TBase>.FromDictionary(resourceStore.ToDictionary())),
                        default,
                        default);
            }
            else
                throw new ResourceProviderException($"Could not locate the {resourcePath.ResourceTypeInstances[0].ResourceId} vectorization resource.",
                            StatusCodes.Status404NotFound);
        }

        #endregion

        #endregion

        /// <inheritdoc/>
        protected override T GetResourceInternal<T>(ResourcePath resourcePath) where T : class =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.TextPartitioningProfiles => GetTextPartitioningProfile<T>(resourcePath),
                VectorizationResourceTypeNames.TextEmbeddingProfiles => GetTextEmbeddingProfile<T>(resourcePath),
                VectorizationResourceTypeNames.IndexingProfiles => GetIndexingProfile<T>(resourcePath),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourceInternal<T>

        private T GetTextPartitioningProfile<T>(ResourcePath resourcePath) where T : class
        {
            if (resourcePath.ResourceTypeInstances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(TextPartitioningProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({resourcePath.ResourceTypeInstances[0].ResourceType}).");

            _textPartitioningProfiles.TryGetValue(resourcePath.ResourceTypeInstances[0].ResourceId!, out var textPartitioningProfile);
            return textPartitioningProfile as T
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");
        }

        private T GetTextEmbeddingProfile<T>(ResourcePath resourcePath) where T : class
        {
            if (resourcePath.ResourceTypeInstances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(TextEmbeddingProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({resourcePath.ResourceTypeInstances[0].ResourceType}).");

            _textEmbeddingProfiles.TryGetValue(resourcePath.ResourceTypeInstances[0].ResourceId!, out var textEmbeddingProfile);
            return textEmbeddingProfile as T
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");
        }

        private T GetIndexingProfile<T>(ResourcePath resourcePath) where T : class
        {
            if (resourcePath.ResourceTypeInstances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(IndexingProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({resourcePath.ResourceTypeInstances[0].ResourceType}).");

            _indexingProfiles.TryGetValue(resourcePath.ResourceTypeInstances[0].ResourceId!, out var indexingProfile);
            return indexingProfile as T
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task UpsertResourceAsync<T>(ResourcePath resourcePath, T resource)
        {
            switch (resourcePath.ResourceTypeInstances[0].ResourceType)
            {
                case VectorizationResourceTypeNames.VectorizationRequests:
                    await UpdateVectorizationRequest(resourcePath, resource as VectorizationRequest ??
                        throw new ResourceProviderException($"The type {typeof(T)} was not VectorizationRequest.",
                            StatusCodes.Status400BadRequest));
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            }
        }

        #region Helpers for UpsertResourceAsync<T>

        private async Task UpdateVectorizationRequest(ResourcePath resourcePath, VectorizationRequest request)
        {
            request.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);
            await Task.CompletedTask;
        }

        #endregion

        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventSetEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.Namespace);

            switch (e.Namespace)
            {
                case EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Vectorization:
                    foreach (var @event in e.Events)
                        await HandleVectorizationResourceProviderEvent(@event);
                    break;
                default:
                    // Ignore sliently any event namespace that's of no interest.
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task HandleVectorizationResourceProviderEvent(CloudEvent e)
        {
            if (string.IsNullOrWhiteSpace(e.Subject))
                return;

            var fileName = e.Subject.Split("/").Last();

            _logger.LogInformation("The file [{FileName}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
                fileName, _name);

            switch (fileName)
            {
                case TEXT_PARTITIONING_PROFILES_FILE_NAME:
                    _defaultTextPartitioningProfileName = await LoadResourceStore<TextPartitioningProfile, VectorizationProfileBase>(TEXT_PARTITIONING_PROFILES_FILE_PATH, _textPartitioningProfiles);
                    break;
                case TEXT_EMBEDDING_PROFILES_FILE_NAME:
                    _defaultTextEmbeddingProfileName = await LoadResourceStore<TextEmbeddingProfile, VectorizationProfileBase>(TEXT_EMBEDDING_PROFILES_FILE_PATH, _textEmbeddingProfiles);
                    break;
                case INDEXING_PROFILES_FILE_NAME:
                    _defaultIndexingProfileName = await LoadResourceStore<IndexingProfile, VectorizationProfileBase>(INDEXING_PROFILES_FILE_PATH, _indexingProfiles);
                    break;
                default:
                    _logger.LogWarning("The file {FileName} is not managed by the FoundationaLLM.Vectorization resource provider.", fileName);
                    break;
            }
        }

        #endregion
    }
}
