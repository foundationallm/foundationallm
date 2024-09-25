using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents a vectorization request processing context.
    /// This class associates a dequeued request with the corresponding vectorization request resource.
    /// </summary>
    public class VectorizationRequestProcessingContext
    {
        /// <summary>
        /// Gets or sets the FoundationaLLM instance identifier.
        /// </summary>
        /// <remarks>
        /// This is the identifier of the FoundationaLLM instance that the request is associated with.
        /// </remarks>
        public required string InstanceId { get; set; }

        /// <summary>
        /// The message that was dequeued.
        /// </summary>
        public required VectorizationDequeuedRequest DequeuedRequest { get; set; }

        /// <summary>
        /// The vectorization request resource.
        /// </summary>
        public required VectorizationRequest Request { get; set; }
    }
}
