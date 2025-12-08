namespace FoundationaLLM.Common.Models.Files
{
    /// <summary>
    /// Represents the result of determining the content type for a file, including the detected MIME type and whether
    /// it matches the file's extension.
    /// </summary>
    public class FileContentTypeResult
    {
        /// <summary>
        /// Gets the content type of the file.
        /// </summary>
        public required string ContentType { get; init; }

        /// <summary>
        /// Gets a value indicating whether the content type is supported or not.
        /// </summary>
        public bool IsSupported { get; init; }

        /// <summary>
        /// Gets a value indicating whether the content type matches the file's extension.
        /// </summary>
        public bool MatchesExtension { get; init; }
    }
}
