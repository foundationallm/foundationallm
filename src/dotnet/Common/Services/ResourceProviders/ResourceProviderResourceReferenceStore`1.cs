﻿using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.ResourceProviders
{
    /// <summary>
    /// Maintains a collection of resource references used by FoundationaLLM resource providers.
    /// </summary>
    /// <typeparam name="T">The type of resource reference kept in the store.</typeparam>
    /// <param name="resourceProvider">The <see cref="IResourceProviderService"/> resource provider service that uses the reference store.</param>
    /// <param name="resourceProviderStorageService">The <see cref="IStorageService"/> used by the resource provider.</param>
    /// <param name="logger">The <see cref="ILogger"/> service used by the resource provider.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used by the resource provider to signal the need to cancel operations.</param>
    public class ResourceProviderResourceReferenceStore<T>(
        IResourceProviderService resourceProvider,
        IStorageService resourceProviderStorageService,
        ILogger logger,
        CancellationToken cancellationToken = default)where T : ResourceReference
    {
        private readonly IResourceProviderService _resourceProvider = resourceProvider;
        private readonly IStorageService _storage = resourceProviderStorageService;
        private readonly ILogger _logger = logger;
        private readonly CancellationToken _cancellationToken = cancellationToken;

        private readonly Dictionary<string, T> _resourceReferences = [];

        private const string RESOURCE_REFERENCES_FILE_NAME = "_resource-references.json";
        private string ResourceReferencesFilePath => $"/{_resourceProvider.Name}/{RESOURCE_REFERENCES_FILE_NAME}";

        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Loads the resource references from the storage service.
        /// </summary>
        /// <returns></returns>
        public async Task LoadResourceReferences()
        {
            _logger.LogInformation($"Starting to load the references for the {_resourceProvider.Name} resource provider...");

            await _lock.WaitAsync();
            try
            {
                if (await _storage.FileExistsAsync(
                    _resourceProvider.StorageContainerName,
                    ResourceReferencesFilePath,
                    _cancellationToken))
                {
                   await LoadAndMergeResourceReferences();
                }
                else
                {
                    // The resource references file does not exist, so we need to create it.
                    await SaveResourceReferences();
                }

                _logger.LogInformation(
                    "The references for the {ResourceProviderName} were successfully loaded.",
                    _resourceProvider.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "There was an error while loading the resource references for the {ResourceProviderName} resource provider.",
                    _resourceProvider.Name);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gets a resource reference by the unique name of the resource.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns></returns>
        public async Task<T?> GetResourceReference(string resourceName)
        {
            await _lock.WaitAsync();
            try
            {
                var resourceReference = GetResourceReferenceInternal(resourceName);

                if (resourceReference == null)
                {
                    // The reference was not found which means it either does not exist or has been created by another instance of the resource provider.

                    // Wait for a second to ensure that potential reference creation processes happening in different instances of the resource provider have completed.
                    await Task.Delay(1000, _cancellationToken);

                    await LoadAndMergeResourceReferences();
                }

                // Try getting the reference again.
                resourceReference = GetResourceReferenceInternal(resourceName);

                // Return the result, regardless of whether it is null or not.
                // If it is null, the caller will have to handle the situation.
                return resourceReference;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gets all resource references in the store.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> contain</returns>
        public async Task<List<T>> GetAllResourceReferences()
        {
            await _lock.WaitAsync();
            try
            {
                return [.. _resourceReferences.Values];
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Adds a resource reference to the store.
        /// </summary>
        /// <param name="resourceReference">The resource reference to add.</param>
        /// <returns></returns>
        public async Task AddResourceReference(T resourceReference)
        {
            await _lock.WaitAsync();
            try
            {
                var existingResourceReference = GetResourceReferenceInternal(resourceReference.Name);

                if (existingResourceReference != null)
                    throw new ResourceProviderException(
                        $"A resource reference for the resource {resourceReference.Name} already exists.",
                        StatusCodes.Status400BadRequest);

                _resourceReferences[resourceReference.Name] = resourceReference;

                await SaveResourceReferences();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Adds a resource reference to the store.
        /// </summary>
        /// <param name="resourceReferences">The list of resource references to add.</param>
        /// <returns></returns>
        public async Task AddResourceReferences(IEnumerable<T> resourceReferences)
        {
            await _lock.WaitAsync();
            try
            {
                foreach (var resourceReference in resourceReferences)
                {
                    var existingResourceReference = GetResourceReferenceInternal(resourceReference.Name);

                    if (existingResourceReference != null)
                        throw new ResourceProviderException(
                            $"A resource reference for the resource {resourceReference.Name} already exists.",
                            StatusCodes.Status400BadRequest);

                    _resourceReferences[resourceReference.Name] = resourceReference;
                }

                await SaveResourceReferences();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Loads the persisted resource references and merges them with the existing references.
        /// </summary>
        /// <remarks>
        /// IMPORTANT!
        /// Never call this method without acquiring the lock first.
        /// </remarks>
        private async Task LoadAndMergeResourceReferences()
        {
            var fileContent = await _storage.ReadFileAsync(
                        _resourceProvider.StorageContainerName,
                        ResourceReferencesFilePath,
                        _cancellationToken);
            var _persistedReferences = JsonSerializer.Deserialize<ResourceReferenceList<T>>(
                Encoding.UTF8.GetString(fileContent.ToArray()))!.ResourceReferences;

            foreach (var reference in _persistedReferences.Values)
            {
                if (!_resourceReferences.ContainsKey(reference.Name))
                {
                    _resourceReferences[reference.Name] = reference;
                }
            }
        }

        /// <summary>
        /// Saves the resource references to the storage service.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// IMPORTANT!
        /// Never call this method without acquiring the lock first.
        /// </remarks>
        private async Task SaveResourceReferences() =>
            await _storage.WriteFileAsync(
                _resourceProvider.StorageContainerName,
                ResourceReferencesFilePath,
                JsonSerializer.Serialize(new ResourceReferenceList<T>
                {
                    ResourceReferences = _resourceReferences
                }),
                default,
                _cancellationToken);

        /// <summary>
        /// Gets a resource reference by the unique name of the resource.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        /// <remarks>
        /// <para>If the resource exists and it was deleted without being also purged, an exception is thrown.</para>
        /// <para>
        /// IMPORTANT!
        /// Never call this method without acquiring the lock first.
        /// </para>
        /// </remarks>
        private T? GetResourceReferenceInternal(string resourceName)
        {
            if (_resourceReferences.TryGetValue(resourceName, out var reference))
            {
                if (reference == null
                    || reference.Deleted)
                    throw new ResourceProviderException(
                        $"The resource reference for the resource {resourceName} cannot be retrieved. "
                        + "It is either null or points to a resource that has been deleted but not purged yet.",
                        StatusCodes.Status400BadRequest);

                return reference;
            }
            else
                return null;
        }
    }
}
