﻿namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Types of text embeddings available for text embedding.
    /// </summary>
    public enum TextEmbeddingType
    {
        /// <summary>
        /// Text embedding that uses Semantic Kernel to embed text.
        /// </summary>
        SemanticKernelTextEmbedding,

        /// <summary>
        /// Text emebdding that uses the Gateway API to embed text.
        /// </summary>
        GatewayTextEmbedding
    }
}
