using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Service that provides a common interface for creating <see cref="HttpClient"/>
    /// instances from <see cref="IHttpClientFactory"/>. and ensures that all
    /// necessary headers are added to the request.
    /// </summary>
    public interface IHttpClientFactoryService
    {
        /// <summary>
        /// Creates a <see cref="HttpClient"/> instance based on the client name.
        /// The client name must be registered in the <see cref="IHttpClientFactory"/> configuration.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="clientName">The name of the HTTP client to create. This name must be registered as an <see cref="APIEndpointConfiguration"/> resource in the FoundationaLLM.Configuration resource provider.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> of the caller requesting the client.</param>
        /// <returns>An <see cref="HttpClient"/> instance.</returns>
        Task<HttpClient> CreateClient(
            string instanceId,
            string clientName,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates a <typeparamref name="T"/> client instance based on the client name and a client builder delegate.
        /// </summary>
        /// <typeparam name="T">The type of the client to create.</typeparam>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="clientName">The name of the HTTP client to create. This name must be registered as an <see cref="APIEndpointConfiguration"/> resource in the FoundationaLLM.Configuration resource provider.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> of the caller requesting the client.</param>
        /// <param name="clientBuilder">A delegate that creates the <typeparamref name="T"/> client instance based on a dictionary of values. The keys available in the dictionary are defined in <see cref="HttpClientFactoryServiceKeyNames"/>.</param>
        /// <param name="clientBuilderParameters">A dictionary of parameters to pass to the client builder delegate.</param>
        /// <returns>A <typeparamref name="T"/> client instance.</returns>
        Task<T> CreateClient<T>(
            string instanceId,
            string clientName,
            UnifiedUserIdentity userIdentity,
            Func<Dictionary<string, object>, T> clientBuilder,
            Dictionary<string, object>? clientBuilderParameters = null);

        /// <summary>
        /// Creates a <see cref="HttpClient"/> instance based on the endpoint configuration.
        /// </summary>
        /// <param name="endpointConfiguration">The <see cref="APIEndpointConfiguration"/> resource used to create the client.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> of the caller requesting the client.</param>
        /// <returns>An <see cref="HttpClient"/> instance.</returns>
        Task<HttpClient> CreateClient(
            APIEndpointConfiguration endpointConfiguration,
            UnifiedUserIdentity? userIdentity);

        /// <summary>
        /// Creates a <see cref="HttpClient"/> instance based on the client name and sets the base address to the status endpoint.
        /// The client name must be registered in the <see cref="IHttpClientFactory"/> configuration.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="clientName">The name of the HTTP client to create. This name must be registered as an <see cref="APIEndpointConfiguration"/> resource in the FoundationaLLM.Configuration resource provider.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> of the caller requesting the client.</param>
        /// <returns>An <see cref="HttpClient"/> instance.</returns>
        /// <exception cref="Exception">When if the service base address is null or the status endpoint is null or empty.</exception>
        Task<HttpClient> CreateClientForStatus(
            string instanceId,
            string clientName,
            UnifiedUserIdentity userIdentity);
    }
}
