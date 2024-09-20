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
    }
}
