using FoundationaLLM.Common.Constants.ResourceProviders;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    /// <summary>
    /// Provides the properties for AI models used for embeddings.
    /// </summary>
    public class EmbeddingAIModel : AIModelBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EmbeddingAIModel"/> AI model.
        /// </summary>
        public EmbeddingAIModel() =>
            Type = AIModelTypes.Embedding;
    }
}
