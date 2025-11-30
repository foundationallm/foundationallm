namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Provides mappings from MIME content types to file type identifiers.
    /// </summary>
    public static class ContentTypeMappings
    {
        /// <summary>
        /// Provides a mapping between MIME types and their corresponding file format extensions.
        /// </summary>
        /// <remarks>This dictionary contains a predefined set of MIME types as keys and their associated
        /// file format  extensions as values. It is useful for converting MIME types to human-readable file format
        /// identifiers. The dictionary is read-only and cannot be modified at runtime.</remarks>
        public static readonly IReadOnlyDictionary<string, string> Map = new Dictionary<string, string>
        {
            { "application/pdf", "PDF" },
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "DOCX" },
            { "application/vnd.openxmlformats-officedocument.presentationml.presentation", "PPTX" },
            { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "XLSX" },
            { "application/rtf", "RTF" },
            { "application/javascript", "JS" },
            { "application/yaml", "YAML" },
            { "application/x-yaml", "YAML" },
            { "application/toml", "TOML" },
            { "application/zip", "ZIP" },
            { "application/json", "JSON" },
            { "application/x-ndjson", "JSONL" },
            { "application/jsonlines", "JSONL" },
            { "application/xml", "XML" },
            { "text/plain", "TXT" },
            { "text/markdown", "MD" },
            { "text/html", "HTML" },
            { "text/xml", "XML" },
            { "text/tab-separated-values", "TSV" },
            { "text/tsv", "TSV" },
            { "text/csv", "CSV" },
            { "text/css", "CSS" },
            { "text/x-python", "PY" },
            { "text/x-java-source", "JAVA" },
            { "text/x-shellscript", "SH" },
            { "text/yaml", "YAML" },
            { "text/x-ini", "INI" },
            { "image/jpeg", "JPEG" },
            { "image/png", "PNG" },
            { "image/gif", "GIF" },
            { "image/bmp", "BMP" },
            { "image/tiff", "TIFF" },
            { "audio/wav", "WAV" },
            { "audio/x-wav", "WAV" },
            { "audio/vnd.wave", "WAV" }
        };

        /// <summary>
        /// Provides a set of MIME types that are commonly associated with text-based content.
        /// </summary>
        /// <remarks>This collection includes MIME types for various text formats such as plain text,
        /// markup languages, programming languages, and data serialization formats. It can be used to identify or
        /// validate content types that are expected to contain textual data.</remarks>
        public static readonly HashSet<string> TextContentTypes =
        [
            "application/rtf",
            "application/javascript",
            "application/yaml",
            "application/x-yaml",
            "application/toml",
            "application/json",
            "application/x-ndjson",
            "application/jsonlines",
            "application/xml",
            "text/plain",
            "text/markdown",
            "text/html",
            "text/xml",
            "text/tab-separated-values",
            "text/tsv",
            "text/csv",
            "text/css",
            "text/x-python",
            "text/x-java-source",
            "text/x-shellscript",
            "text/yaml",
            "text/x-ini"
        ];

        /// <summary>
        /// Provides a set of MIME types that are commonly associated with media content, such as images and audio files.
        /// </summary>
        public static readonly HashSet<string> MediaContentTypes =
        [
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/tiff",
            "audio/wav",
            "audio/x-wav",
            "audio/vnd.wave"
        ];
    }
}
