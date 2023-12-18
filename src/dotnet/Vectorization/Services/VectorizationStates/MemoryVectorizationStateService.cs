using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using System;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Services.VectorizationStates
{
    public class MemoryVectorizationStateService : IVectorizationStateService
    {
        public MemoryVectorizationStateService() 
        { 
        }

        /// <inheritdoc/>
        public async Task<VectorizationState> ReadState(string id)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task SaveState(VectorizationState state)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
