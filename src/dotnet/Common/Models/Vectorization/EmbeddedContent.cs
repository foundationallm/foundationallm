namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// Provides information about embedded content.
    /// </summary>
    public class EmbeddedContent
    {
        /// <summary>
        /// The canonical identifier of the content.
        /// </summary>
        public required ContentIdentifier ContentId { get; set; }

        /// <summary>
        /// The list of conent 
        /// </summary>
        public required List<EmbeddedContentPart> ContentParts { get; set; } = [];
    }
}
