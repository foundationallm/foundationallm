using FoundationaLLM.Vectorization.Models;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Manages vectorization requests.
    /// </summary>
    public interface IRequestSourceService
    {
        /// <summary>
        /// The name of the vectorization request source.
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// Indicates whether the source has pending requests.
        /// </summary>
        /// <returns>A <see cref="bool"/> indicating whether the source has pending requests or not.</returns>
        Task<bool> HasRequests();

        /// <summary>
        /// Receives the specified number of requests.
        /// The received requests will be invisible for other clients for a default timeout of 30 seconds.
        /// They should be removed from the source by calling <see cref="DeleteRequest(string)"/> before the timeout expires.
        /// </summary>
        /// <param name="count">The number of requests to receive.</param>
        /// <returns>A collection of <see cref="VectorizationRequest"/> items.</returns>
        Task<IEnumerable<VectorizationRequest>> ReceiveRequests(int count);

        /// <summary>
        /// Removes a specified vectorization request from the source.
        /// This should happen when the request was successfully processed.
        /// </summary>
        /// <param name="requestId">The identifier of the request being removed.</param>
        /// <returns></returns>
        Task DeleteRequest(string requestId);

        /// <summary>
        /// Submits a new vectorization request to the source.
        /// </summary>
        /// <param name="request">The <see cref="VectorizationRequest"/> item describing the vectorization request.</param>
        /// <returns></returns>
        Task SubmitRequest(VectorizationRequest request);
    }
}
