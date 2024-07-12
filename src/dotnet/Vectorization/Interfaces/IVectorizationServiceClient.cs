using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Defines the interface for the Vectorization API client.
    /// </summary>
    public interface IVectorizationServiceClient
    {
        /// <summary>
        /// Processes an incoming vectorization request.
        /// </summary>
        /// <param name="vectorizationRequest">The <see cref="VectorizationRequest"/> object containing the details of the vectorization request.</param>
        /// <param name="userIdentity">The user identity.</param>
        /// <returns>The result of the request including the resource object id, success or failure plus any error messages.</returns>
        Task<VectorizationResult> ProcessRequest(VectorizationRequest vectorizationRequest, UnifiedUserIdentity? userIdentity);
    }
}
