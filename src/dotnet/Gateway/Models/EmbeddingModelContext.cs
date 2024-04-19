using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Metadata;
using FoundationaLLM.Gateway.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Provides context associated with an embedding model.
    /// </summary>
    public class EmbeddingModelContext
    {
        /// <summary>
        /// The name of the embedding model.
        /// </summary>
        public required string ModelName { get; set; }

        /// <summary>
        /// A list of <see cref="EmbeddingModelDeploymentContext"/> objects providing contexts for the available deployments for the model.
        /// </summary>
        public required List<EmbeddingModelDeploymentContext> DeploymentContexts { get; set; } = [];

        /// <summary>
        /// A queue of active embedding operation identifiers.
        /// </summary>
        public required ConcurrentQueue<string> EmbeddingOperationIds { get; set; } = [];

        public ITextEmbeddingService SelectTextEmbeddingService() =>
            // For now, we are using the first model in the list, which is supposed to always be from the primary account.
            // In the future, we will add load balancing approach to offload work to other accounts as welll.
            DeploymentContexts.First().TextEmbeddingService;
    }
}
