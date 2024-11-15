using FoundationaLLM.SemanticKernel.Core.Services;

namespace FoundationaLLM.SemanticKernel.Core.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings for the <see cref="SemanticKernelService"/> service.
    /// </summary>
    public class SemanticKernelServiceSettings
    {
        /// <summary>
        /// The maximum number of background completion operations allowed to run in parallel.
        /// </summary>
        /// <remarks>
        /// If a new completion request comes in and the maximum number is already reached, the request will generate an error.
        /// </remarks>
        public required int MaxConcurrentCompletions { get; set; }
    }
}
