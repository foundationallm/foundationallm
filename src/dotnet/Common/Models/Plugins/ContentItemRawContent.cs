namespace FoundationaLLM.Common.Models.Plugins
{
    /// <summary>
    /// Provides the raw binary content of a content item.
    /// </summary>
    public class ContentItemRawContent
    {
        /// <summary>
        /// Gets or sets the name of the content item.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the content type of the content item.
        /// </summary>
        public required string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the raw binary content of the content item.
        /// </summary>
        public required BinaryData RawContent { get; set; }
    }
}
