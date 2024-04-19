using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Azure;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Provides context associated with an embedding model deployment.
    /// </summary>
    public class EmbeddingModelDeploymentContext
    {
        /// <summary>
        /// The Azure OpenAI deployment for the model.
        /// </summary>
        public required AzureOpenAIAccountDeployment Deployment { get; set; }

        /// <summary>
        /// The <see cref="ITextEmbeddingService"/> exposing the capabilities to interact with the model.
        /// </summary>
        public required ITextEmbeddingService TextEmbeddingService { get; set; }
    }
}
