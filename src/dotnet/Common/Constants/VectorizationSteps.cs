namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Core FoundationaLLM vectorization step names.
    /// </summary>
    public class VectorizationSteps
    {
        /// <summary>
        /// Extract the text from the content of the document.
        /// </summary>
        public const string Extract = "extract";

        /// <summary>
        /// Partition the text into multiple chunks.
        /// </summary>
        public const string Partition = "partition";

        /// <summary>
        /// Embed text chunks into a latent vector space.
        /// </summary>
        public const string Embed = "embed";

        /// <summary>
        /// Persist vector embeddings into a vector index.
        /// </summary>
        public const string Index = "index";
    }
}
