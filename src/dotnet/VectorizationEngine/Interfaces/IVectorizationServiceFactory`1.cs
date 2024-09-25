using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Creates typed service instances.
    /// </summary>
    public interface IVectorizationServiceFactory<T>

    {
        /// <summary>
        /// Retrieves a service instance of type T specified by name.
        /// </summary>
        /// <param name="serviceName">The name of the service instance to create.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>The service instance created by name.</returns>
        Task<T> GetService(string serviceName, UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves a service instance of type T specified by name and its associated resource object (if any).
        /// </summary>
        /// <param name="serviceName">The name of the service instance to create.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>The service instance and its associated resource object (if any).</returns>
        Task<(T Service, ResourceBase Resource)> GetServiceWithResource(string serviceName, UnifiedUserIdentity userIdentity);
    }
}
