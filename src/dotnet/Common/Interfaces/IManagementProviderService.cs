using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides core services required by the Management API.
    /// </summary>
    public interface IManagementProviderService
    {
        /// <summary>
        /// Handles a HTTP GET request for a specified resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="options">The <see cref="ResourceProviderLoadOptions"/> which provides operation parameters.</param>
        /// <returns>The serialized form of the result of handling the request.</returns>
        Task<object> HandleGetAsync(string resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderLoadOptions? options = null);

        /// <summary>
        /// Handles a HTTP POST request for a specified resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="requestPayload">The request payload.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <returns>The serialized form of the result of handling the request.</returns>
        Task<object> HandlePostAsync(string resourcePath, string requestPayload, UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Handles a HTTP DELETE request for a specified resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        Task HandleDeleteAsync(string resourcePath, UnifiedUserIdentity userIdentity);
    }
}
