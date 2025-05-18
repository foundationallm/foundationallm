using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
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
        Task LoadArtifacts(VectorizationState state, VectorizationArtifactType artifactType);

        /// <summary>
        /// Saves a specified vectorization state.
        /// </summary>
        /// <param name="state">The <see cref="VectorizationState"/> item to be saved.</param>
        Task SaveState(VectorizationState state);

        /// <summary>
        /// Saves the state of a vectorization pipeline execution.
        /// </summary>
        /// <param name="pipelineExecution">The main state of the pipeline execution.</param>
        /// <param name="pipelineExecutionDetail">The details associated with the main state of the pipeline execution.</param>
        Task SavePipelineState(
            VectorizationPipelineExecution pipelineExecution,
            VectorizationPipelineExecutionDetail? pipelineExecutionDetail);

        /// <summary>
        /// Updates the state of a vectorization pipeline execution based on the state of the specified vectorization request.
        /// </summary>
        /// <param name="vectorizationRequest">The vectorization request based on which the pipeline execution state is being updated.</param>
        Task UpdatePipelineStateFromVectorizationRequest(
            VectorizationRequest vectorizationRequest);
    }
}
