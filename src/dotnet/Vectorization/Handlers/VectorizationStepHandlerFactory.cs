using FoundationaLLM.Common.Constants;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;

namespace FoundationaLLM.Vectorization.Handlers
{
    /// <summary>
    /// Implements a factory that creates vectorization step handlers.
    /// </summary>
    public class VectorizationStepHandlerFactory
    {
        /// <summary>
        /// Creates a vectorization step handler capable of handling a specified vectorization pipeline step.
        /// </summary>
        /// <param name="step">The identifier of the vectorization pipeline step for which the handler is created.</param>
        /// <param name="parameters">The parameters used to initialize the vectorization step handler.</param>
        /// <returns>A class implementing <see cref="IVectorizationStepHandler"/>.</returns>
        public static IVectorizationStepHandler Create(string step, Dictionary<string, string> parameters)
        {
            switch (step)
            {
                case VectorizationSteps.Extract:
                    return new ExtractionHandler();
                case VectorizationSteps.Partition:
                    return new PartitionHandler();
                case VectorizationSteps.Embed:
                    return new EmbeddingHandler();
                case VectorizationSteps.Index:
                    return new IndexingHandler();
                default:
                    throw new VectorizationException($"There is no handler available for the vectorization pipeline step [{step}].");
            }
        }
    }
}
