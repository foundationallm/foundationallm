using FoundationaLLM.Vectorization.Models;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Provides persistence services for vectorization pipeline states.
    /// </summary>
    public interface IVectorizationStateService
    {
        /// <summary>
        /// Reads a the content of a specified vectorization requests.
        /// </summary>
        /// <param name="id">The identifier of the vectorization request.</param>
        /// <returns>A <see cref="VectorizationState"/> item containe the requested vectorization state.</returns>
        Task<VectorizationState> ReadState(string id);

        /// <summary>
        /// Saves a specified vectorization state.
        /// </summary>
        /// <param name="state">The <see cref="VectorizationState"/> item to be saved.</param>
        /// <returns></returns>
        Task SaveState(VectorizationState state);
    }
}
