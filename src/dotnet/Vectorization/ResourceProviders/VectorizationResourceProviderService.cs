using Azure.Messaging;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Vectorization.Constants;
using FoundationaLLM.Vectorization.Models;
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
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class VectorizationResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        ILogger<VectorizationResourceProviderService> logger)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            logger,
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Vectorization
            ])
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            VectorizationResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, VectorizationProfileBase> _contentSourceProfiles = [];
        private ConcurrentDictionary<string, VectorizationProfileBase> _textPartitioningProfiles = [];
        private ConcurrentDictionary<string, VectorizationProfileBase> _textEmbeddingProfiles = [];
        private ConcurrentDictionary<string, VectorizationProfileBase> _indexingProfiles = [];

        private const string CONTENT_SOURCE_PROFILES_FILE_NAME = "vectorization-content-source-profiles.json";
        private const string TEXT_PARTITIONING_PROFILES_FILE_NAME = "vectorization-text-partitioning-profiles.json";
        private const string TEXT_EMBEDDING_PROFILES_FILE_NAME = "vectorization-text-embedding-profiles.json";
        private const string INDEXING_PROFILES_FILE_NAME = "vectorization-indexing-profiles.json";

        private const string CONTENT_SOURCE_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{CONTENT_SOURCE_PROFILES_FILE_NAME}";
        private const string TEXT_PARTITIONING_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{TEXT_PARTITIONING_PROFILES_FILE_NAME}";
        private const string TEXT_EMBEDDING_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{TEXT_EMBEDDING_PROFILES_FILE_NAME}";
        private const string INDEXING_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{INDEXING_PROFILES_FILE_NAME}";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Vectorization;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            await LoadProfileStore<ContentSourceProfile>(CONTENT_SOURCE_PROFILES_FILE_PATH, _contentSourceProfiles);
            await LoadProfileStore<TextPartitioningProfile>(TEXT_PARTITIONING_PROFILES_FILE_PATH, _textPartitioningProfiles);
            await LoadProfileStore<TextEmbeddingProfile>(TEXT_EMBEDDING_PROFILES_FILE_PATH, _textEmbeddingProfiles);
            await LoadProfileStore<IndexingProfile>(INDEXING_PROFILES_FILE_PATH, _indexingProfiles);

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        private async Task LoadProfileStore<T>(string profileStoreFilePath, ConcurrentDictionary<string, VectorizationProfileBase> profiles) where T: VectorizationProfileBase
        {
            if (await _storageService.FileExistsAsync(_storageContainerName, profileStoreFilePath, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, profileStoreFilePath, default);
                var profileStore = JsonSerializer.Deserialize<ProfileStore<T>>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));
                if (profileStore != null)
                    foreach (var profile in profileStore.Profiles)
                        profiles.AddOrUpdate(profile.Name, profile, (k, v) => v);
            }
        }

        #region Support for Management API

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> GetResourcesAsyncInternal(ResourcePath resourcePath) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => LoadProfiles<ContentSourceProfile>(resourcePath.ResourceTypeInstances[0], _contentSourceProfiles),
                VectorizationResourceTypeNames.TextPartitioningProfiles => LoadProfiles<TextPartitioningProfile>(resourcePath.ResourceTypeInstances[0], _textPartitioningProfiles),
                VectorizationResourceTypeNames.TextEmbeddingProfiles => LoadProfiles<TextEmbeddingProfile>(resourcePath.ResourceTypeInstances[0], _textEmbeddingProfiles),
                VectorizationResourceTypeNames.IndexingProfiles => LoadProfiles<IndexingProfile>(resourcePath.ResourceTypeInstances[0], _indexingProfiles),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for GetResourcesAsyncInternal

        private List<VectorizationProfileBase> LoadProfiles<T>(ResourceTypeInstance instance, ConcurrentDictionary<string, VectorizationProfileBase> profileStore) where T : VectorizationProfileBase
        {
            if (instance.ResourceId == null)
            {
                return
                    [.. profileStore.Values
                            .Where(p => !p.Deleted)
                    ];
            }
            else
            {
                if (!profileStore.TryGetValue(instance.ResourceId, out var profile)
                    || profile.Deleted)
                    throw new ResourceProviderException($"Could not locate the {instance.ResourceId} vectorization profile resource.",
                        StatusCodes.Status404NotFound);

                return [profile];
            }
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => await UpdateProfile<ContentSourceProfile>(resourcePath, serializedResource, _contentSourceProfiles, CONTENT_SOURCE_PROFILES_FILE_PATH),
                VectorizationResourceTypeNames.TextPartitioningProfiles => await UpdateProfile<TextPartitioningProfile>(resourcePath, serializedResource, _textPartitioningProfiles, TEXT_PARTITIONING_PROFILES_FILE_PATH),
                VectorizationResourceTypeNames.TextEmbeddingProfiles => await UpdateProfile<TextEmbeddingProfile>(resourcePath, serializedResource, _textEmbeddingProfiles, TEXT_EMBEDDING_PROFILES_FILE_PATH),
                VectorizationResourceTypeNames.IndexingProfiles => await UpdateProfile<IndexingProfile>(resourcePath, serializedResource, _indexingProfiles, INDEXING_PROFILES_FILE_PATH),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateProfile<T>(ResourcePath resourcePath, string serializedProfile, ConcurrentDictionary<string, VectorizationProfileBase> profileStore, string storagePath)
            where T : VectorizationProfileBase
        {
            var profile = JsonSerializer.Deserialize<T>(serializedProfile)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (profileStore.TryGetValue(profile.Name, out var existingProfile)
                && existingProfile!.Deleted)
                throw new ResourceProviderException($"The agent resource {existingProfile.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            profile.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var validator = _resourceValidatorFactory.GetValidator<T>();
            if (validator != null)
            {
                var validationResult = await validator.ValidateAsync(profile);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            if (resourcePath.ResourceTypeInstances[0].ResourceId != profile.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            profileStore.AddOrUpdate(profile.Name, profile, (k,v) => profile);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    storagePath,
                    JsonSerializer.Serialize(ProfileStore<VectorizationProfileBase>.FromDictionary(profileStore.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = profile.ObjectId
            };
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction) =>
            resourcePath.ResourceTypeInstances.Last().ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => resourcePath.ResourceTypeInstances.Last().Action switch
                {
                    VectorizationResourceProviderActions.CheckName => CheckProfileName<ContentSourceProfile>(serializedAction, _contentSourceProfiles),
                    _ => throw new ResourceProviderException($"The action {resourcePath.ResourceTypeInstances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} does not support actions in the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for ExecuteActionAsync

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

        #endregion

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case VectorizationResourceTypeNames.ContentSourceProfiles:
                    await DeleteProfile<ContentSourceProfile>(resourcePath, _contentSourceProfiles, CONTENT_SOURCE_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.TextPartitioningProfiles:
                    await DeleteProfile<TextPartitioningProfile>(resourcePath, _textPartitioningProfiles, TEXT_PARTITIONING_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.TextEmbeddingProfiles:
                    await DeleteProfile<TextEmbeddingProfile>(resourcePath, _textEmbeddingProfiles, TEXT_EMBEDDING_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.IndexingProfiles:
                    await DeleteProfile<IndexingProfile>(resourcePath, _indexingProfiles, INDEXING_PROFILES_FILE_PATH);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeleteProfile<T>(ResourcePath resourcePath, ConcurrentDictionary<string, VectorizationProfileBase> profileStore, string storagePath)
            where T : VectorizationProfileBase
        {
            if (profileStore.TryGetValue(resourcePath.ResourceTypeInstances[0].ResourceId!, out var profile)
                || profile!.Deleted)
            {
                profile.Deleted = true;

                await _storageService.WriteFileAsync(
                        _storageContainerName,
                        storagePath,
                        JsonSerializer.Serialize(ProfileStore<VectorizationProfileBase>.FromDictionary(profileStore.ToDictionary())),
                        default,
                        default);
            }
            else
                throw new ResourceProviderException($"Could not locate the {resourcePath.ResourceTypeInstances[0].ResourceId} vectorization profile resource.",
                            StatusCodes.Status404NotFound);
        }

        #endregion

        #endregion

        /// <inheritdoc/>
        protected override T GetResourceInternal<T>(ResourcePath resourcePath) where T : class =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => GetContentSourceProfile<T>(resourcePath),
                VectorizationResourceTypeNames.TextPartitioningProfiles => GetTextPartitioningProfile<T>(resourcePath),
                VectorizationResourceTypeNames.TextEmbeddingProfiles => GetTextEmbeddingProfile<T>(resourcePath),
                VectorizationResourceTypeNames.IndexingProfiles => GetIndexingProfile<T>(resourcePath),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourceInternal<T>

        private T GetContentSourceProfile<T>(ResourcePath resourcePath) where T : class
        {
            if (resourcePath.ResourceTypeInstances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(ContentSourceProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({resourcePath.ResourceTypeInstances[0].ResourceType}).");

            _contentSourceProfiles.TryGetValue(resourcePath.ResourceTypeInstances[0].ResourceId!, out var contentSource);
            return contentSource as T
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");
        }

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
                case CONTENT_SOURCE_PROFILES_FILE_NAME:
                    await LoadProfileStore<ContentSourceProfile>(CONTENT_SOURCE_PROFILES_FILE_PATH, _contentSourceProfiles);
                    break;
                case TEXT_PARTITIONING_PROFILES_FILE_NAME:
                    await LoadProfileStore<TextPartitioningProfile>(TEXT_PARTITIONING_PROFILES_FILE_PATH, _textPartitioningProfiles);
                    break;
                case TEXT_EMBEDDING_PROFILES_FILE_NAME:
                    await LoadProfileStore<TextEmbeddingProfile>(TEXT_EMBEDDING_PROFILES_FILE_PATH, _textEmbeddingProfiles);
                    break;
                case INDEXING_PROFILES_FILE_NAME:
                    await LoadProfileStore<IndexingProfile>(INDEXING_PROFILES_FILE_PATH, _indexingProfiles);
                    break;
                default:
                    _logger.LogWarning("The file {FileName} is not managed by the FoundationaLLM.Vectorization resource provider.", fileName);
                    break;
            }
        }

        #endregion
    }
}
