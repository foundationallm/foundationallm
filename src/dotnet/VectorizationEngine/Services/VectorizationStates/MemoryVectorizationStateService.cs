using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
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
        private readonly Dictionary<string, VectorizationPipelineExecution> _pipelineExecutionDictionary = [];
        private readonly Dictionary<string, VectorizationPipelineExecutionDetail> _pipelineExecutionDetailDictionary = [];

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
        public async Task SavePipelineState(
            VectorizationPipelineExecution pipelineExecution,
            VectorizationPipelineExecutionDetail? pipelineExecutionDetail)
        {
            await Task.CompletedTask;
            ArgumentNullException.ThrowIfNull(pipelineExecution);
            _pipelineExecutionDictionary.TryAdd(pipelineExecution.Name, pipelineExecution);

            if (pipelineExecutionDetail != null)
                _pipelineExecutionDetailDictionary.TryAdd(
                    pipelineExecution.Name,
                    pipelineExecutionDetail);
        }

        /// <inheritdoc/>
        public Task UpdatePipelineStateFromVectorizationRequest(
            VectorizationRequest vectorizationRequest) => throw new NotImplementedException();

        /// <inheritdoc/>
        //public async Task<VectorizationPipelineState> ReadPipelineState(string pipelineName, string pipelineExecutionId)
        //{
        //    await Task.CompletedTask;           
        //    if (!_pipelineStateDictionary.TryGetValue(pipelineExecutionId, out VectorizationPipelineState? value))
        //        throw new ArgumentException($"Vectorization state for pipeline {pipelineName} execution [{pipelineExecutionId}] could not be found.");

        //    return value;
        //}
    }
}
