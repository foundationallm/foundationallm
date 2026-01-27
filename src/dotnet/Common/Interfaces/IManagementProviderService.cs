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
        /// <param name="options">The <see cref="ResourceProviderGetOptions"/> which provides operation parameters.</param>
        /// <param name="resourcePathAvailabilityChecker">An optional resource path availability checker used to block certain resource
        /// providers and resource types.</param>
        /// <returns>The serialized form of the result of handling the request.</returns>
        Task<object> HandleGetAsync(
            string resourcePath,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null,
            Func<HttpMethod, ResourcePath, bool>? resourcePathAvailabilityChecker = null);

        /// <summary>
        /// Attempts to extract a ResourceProviderFormFile from the specified result object.
        /// </summary>
        /// <param name="result">The object to inspect for a ResourceProviderFormFile instance. This parameter is expected to be the
        /// result of a <see cref="IManagementProviderService.HandleGetAsync"/> call.</param>
        /// <param name="formFile">When this method returns, contains the extracted ResourceProviderFormFile if successful; otherwise, null.
        /// This parameter is passed uninitialized.</param>
        /// <returns>true if a ResourceProviderFormFile was successfully extracted from the result object; otherwise, false.</returns>
        bool TryGetResourceProviderFormFile(
            object result,
            out ResourceProviderFormFile formFile);

        /// <summary>
        /// Handles a HTTP POST request for a specified resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="requestPayload">The optional request payload.</param>
        /// <param name="formFile">The optional file attached to the request.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="resourcePathAvailabilityChecker">An optional resource path availability checker used to block certain resource
        /// providers and resource types.</param>
        /// <param name="requestPayloadValidator">An optional function to validate the request payload after deserialization.</param>
        /// <param name="urlEncodedParentResourcePath">An optional URL-friendly identifier of the parent resource.</param>
        /// <remarks>The format of the parent resource identifier must be {resource_provider}|{resource_type}|{resource_name}.
        /// For example, an agent named MAA-01 will be identified by <code>FoundationaLLM.Agent|agents|MAA-01</code></remarks>
        /// <returns>The serialized form of the result of handling the request.</returns>
        Task<object> HandlePostAsync(
            string resourcePath,
            string? requestPayload,
            ResourceProviderFormFile? formFile,
            UnifiedUserIdentity userIdentity,
            Func<HttpMethod, ResourcePath, bool>? resourcePathAvailabilityChecker = null,
            Func<object, bool>? requestPayloadValidator = null,
            string? urlEncodedParentResourcePath = null);

        /// <summary>
        /// Handles a HTTP DELETE request for a specified resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="resourcePathAvailabilityChecker">An optional resource path availability checker used to block certain resource
        /// providers and resource types.</param>
        Task HandleDeleteAsync(
            string resourcePath,
            UnifiedUserIdentity userIdentity,
            Func<HttpMethod, ResourcePath, bool>? resourcePathAvailabilityChecker = null);
    }
}
