namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// Provides information about an embedded content part.
    /// </summary>
    public class EmbeddedContentPart
    {
        /// <summary>
        /// The unique identifier of the embedded content part.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// The text content that was embedded.
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// The vector embedding associated with the content.
        /// </summary>
        public required Embedding Embedding { get; set; }
    }
}
