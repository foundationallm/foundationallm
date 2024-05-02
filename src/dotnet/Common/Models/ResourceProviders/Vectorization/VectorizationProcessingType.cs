namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// The type of vectorization request processing.
    /// </summary>
    public enum VectorizationProcessingType
    {
        /// <summary>
        /// Asynchronous processing using vectorization workers.
        /// </summary>
        Asynchronous,

        /// <summary>
        /// Synchronous processing using the vectorization API.
        /// </summary>
        Synchronous
    }
}
