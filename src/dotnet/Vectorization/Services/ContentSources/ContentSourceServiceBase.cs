using FoundationaLLM.Vectorization.Exceptions;

namespace FoundationaLLM.Vectorization.Services.ContentSources
{
    /// <summary>
    /// Provides common functionalities for all content sources.
    /// </summary>
    public class ContentSourceServiceBase
    {
        /// <summary>
        /// Validates a multipart unique content identifier.
        /// </summary>
        /// <param name="multipartId">The multipart identifier to validate.</param>
        /// <param name="partsCount">The required number of parts in the multipart identifier.</param>
        /// <exception cref="VectorizationException"></exception>
        public void ValidateMultipartId(List<string> multipartId, int partsCount)
        {
            if (multipartId == null
                || multipartId.Count != partsCount
                || multipartId.Any(t => string.IsNullOrWhiteSpace(t)))
                throw new VectorizationException("Invalid multipart identifier.");
        }
    }
}
