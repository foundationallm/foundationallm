using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides the core services implemented by all resource providers. 
    /// </summary>
    public interface IResourceProviderService : IManagementProviderService
    {
        /// <summary>
        /// The name of the resource provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates whether the resource provider is initialized or not.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes { get; }

        /// <summary>
        /// The name of the storage account used by the resource provider.
        /// </summary>
        string StorageAccountName { get; }

        /// <summary>
        /// The name of the storage account container used by the resource provider.
        /// </summary>
        string StorageContainerName { get; }

        /// <summary>
        /// Gets resources of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of resource to return.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="options">The <see cref="ResourceProviderLoadOptions"/> which provides operation parameters.</param>
        /// <returns>A list of <see cref="ResourceProviderGetResult{T}"/> containing the loaded resources.</returns>
        /// <returns></returns>
        Task<List<ResourceProviderGetResult<T>>> GetResourcesAsync<T>(
           string instanceId, UnifiedUserIdentity userIdentity, ResourceProviderLoadOptions? options = null)
           where T : ResourceBase;

        /// <summary>
        /// Gets a resource based on its logical path.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="resourcePath">The logical path of the resource.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="options">The <see cref="ResourceProviderLoadOptions"/> which provides operation parameters.</param>
        /// <returns>The instance of the resource corresponding to the specified logical path.</returns>
       Task<T> GetResourceAsync<T>(string resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderLoadOptions? options = null)
            where T : ResourceBase;

        /// <summary>
        /// Gets a resource based on its name.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="resourceName">The logical path of the resource.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="options">The <see cref="ResourceProviderLoadOptions"/> which provides operation parameters.</param>
        /// <returns>The instance of the resource corresponding to the specified logical path.</returns>
        Task<T> GetResourceAsync<T>(string instanceId, string resourceName, UnifiedUserIdentity userIdentity, ResourceProviderLoadOptions? options = null)
            where T : ResourceBase;

        /// <summary>
        /// Creates or updates a resource based on its logical path.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <typeparam name="TResult">The type of the result returned</typeparam>
        /// <param name="resourcePath">The logical path of the resource.</param>
        /// <param name="resource">The instance of the resource being created or updated.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <returns>The object id of the resource.</returns>
        Task<TResult> UpsertResourceAsync<T, TResult>(string resourcePath, T resource, UnifiedUserIdentity userIdentity)
            where T : ResourceBase
            where TResult : ResourceProviderUpsertResult;

        /// <summary>
        /// Checks if a resource exists.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="resourceName">The resource name being checked.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A tuple indicating whether the resource exists or not and whether it is logically deleted or not.</returns>
        /// <remarks>
        /// If a resource was logically deleted but not purged, this method will return True, indicating the existence of the resource.
        /// </remarks>
        Task<(bool Exists, bool Deleted)> ResourceExists<T>(string instanceId, string resourceName, UnifiedUserIdentity userIdentity)
            where T : ResourceBase;

        /// <summary>
        /// Initializes the resource provider.
        /// </summary>
        /// <returns></returns>
        Task Initialize();

        /// <summary>
        /// Waits for the resource provider service to be initialized.
        /// </summary>
        Task WaitForInitialization();
    }
}
