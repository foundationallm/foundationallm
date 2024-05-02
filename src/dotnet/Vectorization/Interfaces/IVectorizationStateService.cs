using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Models;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Provides persistence services for vectorization pipeline states.
    /// </summary>
    public interface IVectorizationStateService
    {
        /// <summary>
        /// Checks if a vectorization request has a persisted vectorization state.
        /// </summary>
        /// <param name="request">The <see cref="VectorizationRequest"/> object.</param>
        /// <returns>True if a persisted state was found.</returns>
        Task<bool> HasState(VectorizationRequest request);

        /// <summary>
        /// Reads the state associated with a vectorization request.
        /// </summary>
        /// <param name="request">The <see cref="VectorizationRequest"/> object..</param>
        /// <returns>A <see cref="VectorizationState"/> item containe the requested vectorization state.</returns>
        Task<VectorizationState> ReadState(VectorizationRequest request);

        /// <summary>
        /// Loads into the state the specified type of artifact(s).
        /// </summary>
        /// <param name="state">The vectorization state in which the artifacts will be loaded.</param>
        /// <param name="artifactType">The type of artifact(s) to load.</param>
        /// <returns></returns>
        Task LoadArtifacts(VectorizationState state, VectorizationArtifactType artifactType);

        /// <summary>
        /// Saves a specified vectorization state.
        /// </summary>
        /// <param name="state">The <see cref="VectorizationState"/> item to be saved.</param>
        /// <returns></returns>
        Task SaveState(VectorizationState state);
    }
}
