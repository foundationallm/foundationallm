using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <returns>The serialized form of the result of handling the request.</returns>
        Task<object> HandleGetAsync(string resourcePath);

        /// <summary>
        /// Handles a HTTP POST request for a specified resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="requestPayload">The request payload.</param>
        /// <returns>The serialized form of the result of handling the request.</returns>
        Task<object> HandlePostAsync(string resourcePath, string requestPayload);

        /// <summary>
        /// Handles a HTTP DELETE request for a specified resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        Task HandleDeleteAsync(string resourcePath);
    }
}
