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
        /// The task that is executed when the resource provider is initialized.
        /// </summary>
        /// <remarks>
        /// This task enables the consumers of the resource provider to wait for the initialization to be completed.
        /// </remarks>
        Task InitializationTask { get; }

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
        /// Gets the optional storage root path used by the resource provider.
        /// </summary>
        string? StorageRootPath { get; }

        /// <summary>
        /// Gets resources of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of resource to return.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="options">The <see cref="ResourceProviderGetOptions"/> which provides operation parameters.</param>
        /// <returns>A list of <see cref="ResourceProviderGetResult{T}"/> containing the loaded resources.</returns>
        /// <returns></returns>
        Task<List<ResourceProviderGetResult<T>>> GetResourcesAsync<T>(
            string instanceId,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null)
            where T : ResourceBase;

        /// <summary>
        /// Gets a resource based on its logical path.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="resourcePath">The logical path of the resource.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="options">The <see cref="ResourceProviderGetOptions"/> which provides operation parameters.</param>
        /// <param name="parentResourceInstance">The optional parent resource of the resource identified by <paramref name="resourcePath"/>.</param>
        /// <returns>The instance of the resource corresponding to the specified logical path.</returns>
        /// <remarks>
        /// When the parent resource instance is provided, and it specifies inheritable authorizable actions,
        /// the parent resource instance is used to authorize the request for any of those actions.
        /// </remarks>
        Task<T> GetResourceAsync<T>(
            string resourcePath,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null,
            ResourceBase? parentResourceInstance = null)
            where T : ResourceBase;

        /// <summary>
        /// Gets a resource based on its name.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">The logical path of the resource.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="options">The <see cref="ResourceProviderGetOptions"/> which provides operation parameters.</param>
        /// <param name="parentResourceInstance">The optional parent resource of the resource identified by
        /// <paramref name="instanceId"/> and <paramref name="resourceName"/>.</param>
        /// <returns>The instance of the resource corresponding to the specified logical path.</returns>
        /// <remarks>
        /// When the parent resource instance is provided, and it specifies inheritable authorizable actions,
        /// the parent resource instance is used to authorize the request for any of those actions.
        /// </remarks>
        Task<T> GetResourceAsync<T>(
            string instanceId,
            string resourceName,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null,
            ResourceBase? parentResourceInstance = null)
            where T : ResourceBase;

        /// <summary>
        /// Creates or updates a resource based on its logical path.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <typeparam name="TResult">The type of the result returned</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resource">The instance of the resource being created or updated.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="options">The <see cref="ResourceProviderUpsertOptions"/> which provides operation parameters.</param>
        /// <returns>The object id of the resource.</returns>
        Task<TResult> UpsertResourceAsync<T, TResult>(
            string instanceId,
            T resource,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null)
            where T : ResourceBase
            where TResult : ResourceProviderUpsertResult<T>;

        /// <summary>
        /// Updates a subset of the properties of a resource.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <typeparam name="TResult">The type of the result returned.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">The name of the resource being updated.</param>
        /// <param name="propertyValues">The dictionary with propery names and values to update.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        Task<TResult> UpdateResourcePropertiesAsync<T, TResult>(
            string instanceId,
            string resourceName,
            Dictionary<string, object?> propertyValues,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase
            where TResult : ResourceProviderUpsertResult<T>;

        /// <summary>
        /// Executes an action on a resource.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <typeparam name="TAction">The type of the action payload providing details about the action to be executed.</typeparam>
        /// <typeparam name="TResult">The type of the result returned.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">The name of the resource on which the action is executed.</param>
        /// <param name="actionName">The name of the action being executed.</param>
        /// <param name="actionPayload">The payload of the action providing details about it.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="parentResourceInstance">The optional parent resource of the resource identified by
        /// <paramref name="instanceId"/> and <paramref name="resourceName"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// When the parent resource instance is provided, and it specifies inheritable authorizable actions,
        /// the parent resource instance is used to authorize the request for any of those actions.
        /// </remarks>
        Task<TResult> ExecuteResourceActionAsync<T, TAction, TResult>(
            string instanceId,
            string? resourceName,
            string actionName,
            TAction actionPayload,
            UnifiedUserIdentity userIdentity,
            ResourceBase? parentResourceInstance = null)
            where T : ResourceBase
            where TAction : class?
            where TResult : ResourceProviderActionResult;

        /// <summary>
        /// Executes an action on a main resource and it subordinate resource.
        /// </summary>
        /// <typeparam name="TMain">The type of the main resource.</typeparam>
        /// <typeparam name="TSubordinate">The type of the subordinate resource.</typeparam>
        /// <typeparam name="TAction">The type of the action payload providing details about the action to be executed.</typeparam>
        /// <typeparam name="TResult">The type of the result returned.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="mainResourceName">The name of the main resource on which the action is executed.</param>
        /// <param name="resourceName">The name of the subordinate resource on which the action is executed.</param>
        /// <param name="actionName">The name of the action being executed.</param>
        /// <param name="actionPayload">The payload of the action providing details about it.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="parentResourceInstance">The optional parent resource of the resource identified by
        /// <paramref name="instanceId"/> and <paramref name="resourceName"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// When the parent resource instance is provided, and it specifies inheritable authorizable actions,
        /// the parent resource instance is used to authorize the request for any of those actions.
        /// </remarks>
        Task<TResult> ExecuteResourceActionAsync<TMain, TSubordinate, TAction, TResult>(
            string instanceId,
            string mainResourceName,
            string? resourceName,
            string actionName,
            TAction actionPayload,
            UnifiedUserIdentity userIdentity,
            ResourceBase? parentResourceInstance = null)
            where TMain : ResourceBase
            where TSubordinate : ResourceBase
            where TAction : class?
            where TResult : ResourceProviderActionResult;

        /// <summary>
        /// Checks if a resource exists.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">The resource name being checked.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A tuple indicating whether the resource exists or not and whether it is logically deleted or not.</returns>
        /// <remarks>
        /// If a resource was logically deleted but not purged, this method will return True, indicating the existence of the resource.
        /// </remarks>
        Task<(bool Exists, bool Deleted)> ResourceExistsAsync<T>(
            string instanceId,
            string resourceName,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase;

        /// <summary>
        /// Deletes logically a resource based on its logical path.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceName">The name of the resource being logically deleted.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns></returns>
        Task DeleteResourceAsync<T>(
            string instanceId,
            string resourceName,
            UnifiedUserIdentity userIdentity)
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
