using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;

namespace FoundationaLLM.Vectorization.Services.VectorizationStates
{
    /// <summary>
    /// Provides in-memory vectorization state persistence.
    /// </summary>
    public class MemoryVectorizationStateService : VectorizationStateServiceBase, IVectorizationStateService
    {
        private readonly Dictionary<string, VectorizationState> _vectorizationStateDictionary = [];
        private readonly Dictionary<string, VectorizationPipelineState> _pipelineStateDictionary = [];

        /// <inheritdoc/>
        public async Task<bool> HasState(VectorizationRequest request)
        {
            await Task.CompletedTask;

            return _vectorizationStateDictionary.ContainsKey(
                GetPersistenceIdentifier(request));
        }

        /// <inheritdoc/>
        public async Task<VectorizationState> ReadState(VectorizationRequest request)
        {
            await Task.CompletedTask;
            var id = GetPersistenceIdentifier(request);

            if (!_vectorizationStateDictionary.TryGetValue(id, out VectorizationState? value))
                throw new ArgumentException($"Vectorization state for content id [{id}] could not be found.");

            return value;
        }

        /// <inheritdoc/>
        public async Task LoadArtifacts(VectorizationState state, VectorizationArtifactType artifactType) =>
            await Task.CompletedTask;

        /// <inheritdoc/>
        public async Task SaveState(VectorizationState state)
        {
            await Task.CompletedTask;
            var id = GetPersistenceIdentifier(state);

            ArgumentNullException.ThrowIfNull(state);

            if (!_vectorizationStateDictionary.TryAdd(id, state))
                _vectorizationStateDictionary[id] = state;
        }

        /// <inheritdoc/>
        public async Task SavePipelineState(VectorizationPipelineState state)
        {
            await Task.CompletedTask;
            ArgumentNullException.ThrowIfNull(state);
            if (!_pipelineStateDictionary.TryAdd(state.ExecutionId, state))
                _pipelineStateDictionary[state.ExecutionId] = state;
        }

        /// <inheritdoc/>
        public async Task<VectorizationPipelineState> ReadPipelineState(string pipelineName, string pipelineExecutionId)
        {
            await Task.CompletedTask;           
            if (!_pipelineStateDictionary.TryGetValue(pipelineExecutionId, out VectorizationPipelineState? value))
                throw new ArgumentException($"Vectorization state for pipeline {pipelineName} execution [{pipelineExecutionId}] could not be found.");

            return value;
        }
    }
}
