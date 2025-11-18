namespace FoundationaLLM.Common.Models.ContentSafety
{
    /// <summary>
    /// Represents a document to be analyzed for content safety.
    /// </summary>
    public class ContentSafetyDocument
    {
        /// <summary>
        /// Gets or sets the identifier of the document.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the content of the document
        /// </summary>
        public required string Content { get; set; }
    }
}
