using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines a service for creating client instances used to process requests and responses.
    /// </summary>
    public interface IClientFactoryService
    {
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
    }
}
