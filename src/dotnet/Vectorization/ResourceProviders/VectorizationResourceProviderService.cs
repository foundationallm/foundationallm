using Azure.Messaging;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Vectorization.Constants;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace FoundationaLLM.Vectorization.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    public class VectorizationResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        ILogger<VectorizationResourceProviderService> logger)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            storageService,
            eventService,
            resourceValidatorFactory,
            logger,
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Vectorization
            ])
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() => new()
        {
            {
                VectorizationResourceTypeNames.VectorizationRequests,
                new ResourceTypeDescriptor(
                        VectorizationResourceTypeNames.VectorizationRequests)
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(VectorizationRequest)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(VectorizationRequest)], [typeof(VectorizationProcessingResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ]
                }
            },
            {
                VectorizationResourceTypeNames.ContentSourceProfiles,
                new ResourceTypeDescriptor(
                    VectorizationResourceTypeNames.ContentSourceProfiles)
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(ContentSourceProfile)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ContentSourceProfile)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(VectorizationResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ])
                        ]
                }
            },
            {
                VectorizationResourceTypeNames.TextPartitioningProfiles,
                new ResourceTypeDescriptor(
                    VectorizationResourceTypeNames.TextPartitioningProfiles)
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(TextPartitioningProfile)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(TextPartitioningProfile)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(VectorizationResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ])
                        ]
                }
            },
            {
                VectorizationResourceTypeNames.TextEmbeddingProfiles,
                new ResourceTypeDescriptor(
                    VectorizationResourceTypeNames.TextEmbeddingProfiles)
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(TextEmbeddingProfile)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(TextEmbeddingProfile)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(VectorizationResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ])
                        ]
                }
            },
            {
                VectorizationResourceTypeNames.IndexingProfiles,
                new ResourceTypeDescriptor(
                    VectorizationResourceTypeNames.IndexingProfiles)
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(IndexingProfile)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(IndexingProfile)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(VectorizationResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ])
                        ]
                }
            }
        };

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
        protected override async Task<object> GetResourcesAsyncInternal(List<ResourceTypeInstance> instances) =>
            instances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => LoadProfiles<ContentSourceProfile>(instances[0], _contentSourceProfiles),
                VectorizationResourceTypeNames.TextPartitioningProfiles => LoadProfiles<TextPartitioningProfile>(instances[0], _textPartitioningProfiles),
                VectorizationResourceTypeNames.TextEmbeddingProfiles => LoadProfiles<TextEmbeddingProfile>(instances[0], _textEmbeddingProfiles),
                VectorizationResourceTypeNames.IndexingProfiles => LoadProfiles<IndexingProfile>(instances[0], _indexingProfiles),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource provider.",
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
        protected override async Task<object> UpsertResourceAsync(List<ResourceTypeInstance> instances, string serializedResource) =>
            instances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => await UpdateProfile<ContentSourceProfile>(instances, serializedResource, _contentSourceProfiles, CONTENT_SOURCE_PROFILES_FILE_PATH),
                VectorizationResourceTypeNames.TextPartitioningProfiles => await UpdateProfile<TextPartitioningProfile>(instances, serializedResource, _textPartitioningProfiles, TEXT_PARTITIONING_PROFILES_FILE_PATH),
                VectorizationResourceTypeNames.TextEmbeddingProfiles => await UpdateProfile<TextEmbeddingProfile>(instances, serializedResource, _textEmbeddingProfiles, TEXT_EMBEDDING_PROFILES_FILE_PATH),
                VectorizationResourceTypeNames.IndexingProfiles => await UpdateProfile<IndexingProfile>(instances, serializedResource, _indexingProfiles, INDEXING_PROFILES_FILE_PATH),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateProfile<T>(List<ResourceTypeInstance> instances, string serializedProfile, ConcurrentDictionary<string, VectorizationProfileBase> profileStore, string storagePath)
            where T : VectorizationProfileBase
        {
            var profile = JsonSerializer.Deserialize<T>(serializedProfile)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (profileStore.TryGetValue(profile.Name, out var existingProfile)
                && existingProfile!.Deleted)
                throw new ResourceProviderException($"The agent resource {existingProfile.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            profile.ObjectId = GetObjectId(instances);

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

            if (instances[0].ResourceId != profile.Name)
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
        protected override async Task<object> ExecuteActionAsync(List<ResourceTypeInstance> instances, string serializedAction) =>
            instances.Last().ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => instances.Last().Action switch
                {
                    VectorizationResourceProviderActions.CheckName => CheckProfileName<ContentSourceProfile>(serializedAction, _contentSourceProfiles),
                    _ => throw new ResourceProviderException($"The action {instances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} does not support actions in the {_name} resource provider.",
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
        protected override async Task DeleteResourceAsync(List<ResourceTypeInstance> instances)
        {
            switch (instances.Last().ResourceType)
            {
                case VectorizationResourceTypeNames.ContentSourceProfiles:
                    await DeleteProfile<ContentSourceProfile>(instances, _contentSourceProfiles, CONTENT_SOURCE_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.TextPartitioningProfiles:
                    await DeleteProfile<TextPartitioningProfile>(instances, _textPartitioningProfiles, TEXT_PARTITIONING_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.TextEmbeddingProfiles:
                    await DeleteProfile<TextEmbeddingProfile>(instances, _textEmbeddingProfiles, TEXT_EMBEDDING_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.IndexingProfiles:
                    await DeleteProfile<IndexingProfile>(instances, _indexingProfiles, INDEXING_PROFILES_FILE_PATH);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeleteProfile<T>(List<ResourceTypeInstance> instances, ConcurrentDictionary<string, VectorizationProfileBase> profileStore, string storagePath)
            where T : VectorizationProfileBase
        {
            if (profileStore.TryGetValue(instances[0].ResourceId!, out var profile)
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
                throw new ResourceProviderException($"Could not locate the {instances[0].ResourceId} vectorization profile resource.",
                            StatusCodes.Status404NotFound);
        }

        #endregion

        #endregion

        /// <inheritdoc/>
        protected override T GetResourceInternal<T>(List<ResourceTypeInstance> instances) where T : class =>
            instances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => GetContentSourceProfile<T>(instances),
                VectorizationResourceTypeNames.TextPartitioningProfiles => GetTextPartitioningProfile<T>(instances),
                VectorizationResourceTypeNames.TextEmbeddingProfiles => GetTextEmbeddingProfile<T>(instances),
                VectorizationResourceTypeNames.IndexingProfiles => GetIndexingProfile<T>(instances),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourceInternal<T>

        private T GetContentSourceProfile<T>(List<ResourceTypeInstance> instances) where T : class
        {
            if (instances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(ContentSourceProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).");

            _contentSourceProfiles.TryGetValue(instances[0].ResourceId!, out var contentSource);
            return contentSource as T
                ?? throw new ResourceProviderException($"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.");
        }

        private T GetTextPartitioningProfile<T>(List<ResourceTypeInstance> instances) where T : class
        {
            if (instances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(TextPartitioningProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).");

            _textPartitioningProfiles.TryGetValue(instances[0].ResourceId!, out var textPartitioningProfile);
            return textPartitioningProfile as T
                ?? throw new ResourceProviderException($"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.");
        }

        private T GetTextEmbeddingProfile<T>(List<ResourceTypeInstance> instances) where T : class
        {
            if (instances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(TextEmbeddingProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).");

            _textEmbeddingProfiles.TryGetValue(instances[0].ResourceId!, out var textEmbeddingProfile);
            return textEmbeddingProfile as T
                ?? throw new ResourceProviderException($"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.");
        }

        private T GetIndexingProfile<T>(List<ResourceTypeInstance> instances) where T : class
        {
            if (instances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(IndexingProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).");

            _indexingProfiles.TryGetValue(instances[0].ResourceId!, out var indexingProfile);
            return indexingProfile as T
                ?? throw new ResourceProviderException($"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.");
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task UpsertResourceAsync<T>(List<ResourceTypeInstance> instances, T resource)
        {
            switch (instances[0].ResourceType)
            {
                case VectorizationResourceTypeNames.VectorizationRequests:
                    await UpdateVectorizationRequest(instances, resource as VectorizationRequest ??
                        throw new ResourceProviderException($"The type {typeof(T)} was not VectorizationRequest.",
                            StatusCodes.Status400BadRequest));
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            }
        }

        #region Helpers for UpsertResourceAsync<T>

        private async Task UpdateVectorizationRequest(List<ResourceTypeInstance> instances, VectorizationRequest request)
        {
            request.ObjectId = GetObjectId(instances);
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
