using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using System.Text.Json;

namespace FoundationaLLM.Common.Extensions
{
    /// <summary>
    /// Extends the <see cref="IResourceProviderService"/> interface with helper methods.
    /// </summary>
    public static class ResourceProviderServiceExtensions
    {
        /// <summary>
        /// Creates or updates a resource.
        /// </summary>
        /// <typeparam name="T">The object type of the resource being created or updated.</typeparam>
        /// <typeparam name="TResult">The object type of the response returned by the operation</typeparam>
        /// <param name="resourceProviderService">The <see cref="IResourceProviderService"/> providing the resource provider services.</param>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="resource">The resource object.</param>
        /// <param name="resourceTypeName">The name of the resource type.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result of the operation.</returns>
        /// <exception cref="ResourceProviderException"></exception>
        public static async Task<TResult> CreateOrUpdateResource<T, TResult>(
            this IResourceProviderService resourceProviderService,
            string instanceId,
            T resource,
            string resourceTypeName,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase
            where TResult : ResourceProviderUpsertResult
        {
            if (!resourceProviderService.IsInitialized)
                throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} is not initialized.");

            var result = await resourceProviderService.UpsertResourceAsync<T, TResult>(
                $"/instances/{instanceId}/providers/{resourceProviderService.Name}/{resourceTypeName}/{resource.Name}",
                resource,
                userIdentity);

            return (result as TResult)!;
        }

        /// <summary>
        /// Gets a resource from the resource provider service.
        /// </summary>
        /// <typeparam name="T">The object type of the resource being retrieved.</typeparam>
        /// <param name="resourceProviderService">The <see cref="IResourceProviderService"/> providing the resource provider services.</param>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="resourceName">The resource name being checked.</param>
        /// <param name="resourceTypeName">The name of the resource type.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A resource object of type <typeparamref name="T"/>.</returns>
        public static async Task<T> HandleGet<T>(
            this IResourceProviderService resourceProviderService,
            string instanceId,
            string resourceName,
            string resourceTypeName,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase
        {
            if (!resourceProviderService.IsInitialized)
                throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} is not initialized.");

            var result = await resourceProviderService.HandleGetAsync(
                $"/instances/{instanceId}/providers/{resourceProviderService.Name}/{resourceTypeName}/{resourceName}",
                userIdentity) as List<ResourceProviderGetResult<T>>;

            if (result == null || result.Count == 0)
                throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} is unable to retrieve the {resourceName} resource.");

            return result.First().Resource;
        }

        /// <summary>
        /// Gets a resource from the resource provider service.
        /// </summary>
        /// <typeparam name="T">The object type of the resource being retrieved.</typeparam>
        /// <param name="resourceProviderService">The <see cref="IResourceProviderService"/> providing the resource provider services.</param>
        /// <param name="objectId">The resource object identifier.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A resource object of type <typeparamref name="T"/>.</returns>
        public static async Task<T> HandleGet<T>(
            this IResourceProviderService resourceProviderService,
            string objectId,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase
        {
            if (!resourceProviderService.IsInitialized)
                throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} is not initialized.");

            var result = await resourceProviderService.HandleGetAsync(
                objectId,
                userIdentity) as List<ResourceProviderGetResult<T>>;

            if (result == null || result.Count == 0)
                throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} is unable to retrieve the {objectId} resource.");

            return result.First().Resource;
        }

        /// <summary>
        /// Gets a list of resources from the resource provider service.
        /// </summary>
        /// <typeparam name="T">The object type of the resources being retrieved.</typeparam>
        /// <param name="resourceProviderService">The <see cref="IResourceProviderService"/> providing the resource provider services.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A list of resource objects of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ResourceProviderException"></exception>
        public static async Task<List<T>> GetResources<T>(
            this IResourceProviderService resourceProviderService,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase
        {
            if (!resourceProviderService.IsInitialized)
                throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} is not initialized.");

            var resourceTypeDescriptor = resourceProviderService.AllowedResourceTypes.Values
                .SingleOrDefault(rtd => rtd.TypeAllowedForHttpGet(typeof(ResourceProviderGetResult<T>)))
                ?? throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} does not support retrieving resources of type {typeof(T).Name}.");

            var result = await resourceProviderService.HandleGetAsync(
                $"/{resourceTypeDescriptor.ResourceTypeName}",
                userIdentity);

            return (result as List<ResourceProviderGetResult<T>>)!.Select(x => x.Resource).ToList();
        }

        /// <summary>
        /// Gets a list of resources with RBAC information from the resource provider service.
        /// </summary>
        /// <typeparam name="T">The object type of the resources being retrieved.</typeparam>
        /// <param name="resourceProviderService">The <see cref="IResourceProviderService"/> providing the resource provider services.</param>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A list of resource objects of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ResourceProviderException"></exception>
        public static async Task<List<ResourceProviderGetResult<T>>> GetResourcesWithRBAC<T>(
            this IResourceProviderService resourceProviderService,
            string instanceId,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase
        {
            if (!resourceProviderService.IsInitialized)
                throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} is not initialized.");

            var resourceTypeDescriptor = resourceProviderService.AllowedResourceTypes.Values
                .SingleOrDefault(rtd => rtd.TypeAllowedForHttpGet(typeof(ResourceProviderGetResult<T>)))
                ?? throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} does not support retrieving resources of type {typeof(T).Name}.");

            var result = await resourceProviderService.HandleGetAsync(
                $"{resourceTypeDescriptor.ResourceTypeName}",
                userIdentity);

            return (result as List<ResourceProviderGetResult<T>>)!;
        }

        /// <summary>
        /// Checks a resource name for availability.
        /// </summary>
        /// <param name="resourceProviderService">The <see cref="IResourceProviderService"/> providing the resource provider services.</param>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="resourceName">The resource name being checked.</param>
        /// <param name="resourceTypeName">The name of the resource type.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A <see cref="ResourceNameCheckResult"/> object with the result of the name check.</returns>
        public static async Task<ResourceNameCheckResult> CheckResourceName(
            this IResourceProviderService resourceProviderService,
            string instanceId,
            string resourceName,
            string resourceTypeName,
            UnifiedUserIdentity userIdentity)
        {
            if (!resourceProviderService.IsInitialized)
                throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} is not initialized.");

            var result = await resourceProviderService.HandlePostAsync(
                $"/instances/{instanceId}/providers/{resourceProviderService.Name}/{resourceTypeName}/{ResourceProviderActions.CheckName}",
                JsonSerializer.Serialize(new ResourceName { Name = resourceName }),
                userIdentity);

            return (result as ResourceNameCheckResult)!;
        }

        /// <summary>
        /// Checks if a resource exists.
        /// </summary>
        /// <param name="resourceProviderService">The <see cref="IResourceProviderService"/> providing the resource provider services.</param>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="resourceName">The resource name being checked.</param>
        /// <param name="resourceTypeName">The name of the resource type.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>True if the resource exists, False otherwise.</returns>
        /// <remarks>
        /// If a resource was logically deleted but not purged, this method will return True, indicating the existence of the resource.
        /// </remarks>
        public static async Task<bool> ResourceExists(
            this IResourceProviderService resourceProviderService,
            string instanceId,
            string resourceName,
            string resourceTypeName,
            UnifiedUserIdentity userIdentity)
        {
            if (!resourceProviderService.IsInitialized)
                throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} is not initialized.");

            var result = await resourceProviderService.HandlePostAsync(
                $"/instances/{instanceId}/providers/{resourceProviderService.Name}/{resourceTypeName}/{ResourceProviderActions.CheckName}",
                JsonSerializer.Serialize(new ResourceName { Name = resourceName }),
                userIdentity);

            return (result as ResourceNameCheckResult)!.Status == NameCheckResultType.Denied;
        }

        /// <summary>
        /// Waits for the resource provider service to be initialized.
        /// </summary>
        /// <param name="resourceProviderService">The <see cref="IResourceProviderService"/> providing the resource provider services.</param>
        /// <returns></returns>
        public static async Task WaitForInitialization(
            this IResourceProviderService resourceProviderService)
        {
            if (resourceProviderService.IsInitialized)
                return;

            for (int i = 0; i < 6; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                if (resourceProviderService.IsInitialized)
                    return;
            }

            throw new ResourceProviderException($"The resource provider {resourceProviderService.Name} did not initialize within the expected time frame.");
        }
    }
}
