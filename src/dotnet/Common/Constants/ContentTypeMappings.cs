namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Provides mappings from MIME content types to file type identifiers.
    /// </summary>
    public static class ContentTypeMappings
    {
        /// <summary>
        /// Provides a read-only mapping of MIME types to their corresponding plain text or data file format
        /// identifiers.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> PlainTextMap = new Dictionary<string, string>
        {
            // Plain text files
            //---------------------------------------------------------------------------------------------------------------------------
            { "text/x-c", "C" },                                                                            // .C file
            { "text/x-c++src", "CPP" },                                                                     // .CPP file
            { "text/x-csharp", "CS" },                                                                      // .CS file
            { "text/x-csharp-src", "CS" },                                                                  // .CS file
            { "text/css", "CSS" },                                                                          // .CSS file
            { "text/html", "HTML" },                                                                        // .HTML file
            { "text/x-ini", "INI" },                                                                        // .INI file
            { "text/x-java-source", "JAVA" },                                                               // .JAVA file
            { "application/javascript", "JS" },                                                             // .JS file
            { "application/json", "JSON" },                                                                 // .JSON file
            { "application/x-ndjson", "JSONL" },                                                            // .JSONL file
            { "application/jsonlines", "JSONL" },                                                           // .JSONL file
            { "text/markdown", "MD" },                                                                      // .MD file
            { "application/x-https-php", "PHP" },                                                           // .PHP file
            { "text/php", "PHP" },                                                                          // .PHP file
            { "application/php", "PHP" },                                                                   // .PHP file 
            { "text/x-python", "PY" },                                                                      // .PY file
            { "text/x-ruby", "RB" },                                                                        // .RB file
            { "application/x-ruby", "RB" },                                                                 // .RB file
            { "application/rtf", "RTF" },                                                                   // .RTF file
            { "text/x-shellscript", "SH" },                                                                 // .SH file
            { "application/x-tex", "TEX" },                                                                 // .TEX file
            { "application/toml", "TOML" },                                                                 // .TOML file
            { "application/typescript", "TS" },                                                             // .TS file
            { "text/typescript", "TS" },                                                                    // .TS file
            { "text/plain", "TXT" },                                                                        // .TXT file
            { "application/xml", "XML" },                                                                   // .XML file
            { "text/xml", "XML" },                                                                          // .XML file
            { "application/yaml", "YAML" },                                                                 // .YAML/.YML file
            { "application/x-yaml", "YAML" },                                                               // .YAML/.YML file
            { "text/yaml", "YAML" },                                                                        // .YAML/.YML file
            // Data files
            { "text/csv", "CSV" },                                                                          // .CSV file
            { "text/tab-separated-values", "TSV" },                                                         // .TSV file
            { "text/tsv", "TSV" },                                                                          // .TSV file
        };

        /// <summary>
        /// Provides a read-only mapping of media MIME types to their corresponding file format names.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> MediaMap = new Dictionary<string, string>
        {
            // Media files
            //---------------------------------------------------------------------------------------------------------------------------
            { "image/bmp", "BMP" },                                                                         // .BMP file
            { "image/gif", "GIF" },                                                                         // .GIF file
            { "image/jpeg", "JPEG" },                                                                       // .JPEG/.JPG file
            { "image/png", "PNG" },                                                                         // .PNG file
            { "image/tiff", "TIFF" },                                                                       // .TIFF file
            { "audio/wav", "WAV" },                                                                         // .WAV file
            { "audio/x-wav", "WAV" },                                                                       // .WAV file
            { "audio/vnd.wave", "WAV" },                                                                    // .WAV file
        };

        /// <summary>
        /// Provides a mapping of legacy Microsoft Office MIME types to their corresponding file format abbreviations.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> MicrosoftOfficeLegacyMap = new Dictionary<string, string>
        {
            // Legacy Microsoft Office formats
            //---------------------------------------------------------------------------------------------------------------------------
            { "application/msword", "DOC"  },                                                               // .DOC file
            { "application/vnd.ms-powerpoint", "PPT" },                                                     // .PPT file
            { "application/vnd.ms-excel", "XLS" },                                                          // .XLS file
        };

        /// <summary>
        /// Provides a read-only mapping of MIME types for common binary document formats to their corresponding file
        /// type identifiers.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> BinaryDocumentMap = new Dictionary<string, string>
        {
            // Binary document files
            //---------------------------------------------------------------------------------------------------------------------------
            { "application/pdf", "PDF" },                                                                   // .PDF file
            // Current Microsoft Office formats
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "DOCX" },          // .DOCX file
            { "application/vnd.openxmlformats-officedocument.presentationml.presentation", "PPTX" },        // .PPTX file
            { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "XLSX" },                // .XLSX file
        };

        /// <summary>
        /// Provides a read-only mapping of archive MIME types to their corresponding file format names.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> ArchiveMap = new Dictionary<string, string>
        {
            // Archive files
            //---------------------------------------------------------------------------------------------------------------------------
            { "application/zip", "ZIP" },                                                                   // .ZIP file
            { "application/x-tar", "TAR" }                                                                  // .TAR file
        };

        /// <summary>
        /// Provides a mapping between MIME types and their corresponding file format extensions.
        /// </summary>
        /// <remarks>This dictionary contains a predefined set of MIME types as keys and their associated
        /// file format  extensions as values. It is useful for converting MIME types to human-readable file format
        /// identifiers. The dictionary is read-only and cannot be modified at runtime.</remarks>
        public static readonly IReadOnlyDictionary<string, string> Map = new[]
            {
                PlainTextMap,
                MediaMap,
                MicrosoftOfficeLegacyMap,
                BinaryDocumentMap,
                ArchiveMap
            }
            .SelectMany(d => d)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        /// <summary>
        /// Provides a set of MIME types that are commonly associated with text-based content.
        /// </summary>
        public static readonly HashSet<string> TextContentTypes =
            [.. PlainTextMap.Keys];

        /// <summary>
        /// Provides a set of file extensions commonly associated with plain text files.
        /// </summary>
        public static readonly HashSet<string> TextFileExtensions =
            [.. PlainTextMap.Values.Distinct()];

        /// <summary>
        /// Provides a set of MIME types that are commonly associated with media content, such as images and audio files.
        /// </summary>
        public static readonly HashSet<string> MediaContentTypes =
            [.. MediaMap.Keys];

        /// <summary>
        /// Provides a set of file extensions recognized as media files by the application.
        /// </summary>
        public static readonly HashSet<string> MediaFileExtensions =
            [.. MediaMap.Values.Distinct()];

        /// <summary>
        /// Provides a set of MIME content types associated with legacy Microsoft Office file formats, including Word,
        /// PowerPoint, and Excel documents.
        /// </summary>
        public static readonly HashSet<string> MicrosoftOfficeLegacyContentTypes =
            [.. MicrosoftOfficeLegacyMap.Keys];

        /// <summary>
        /// Provides a set of file extensions associated with legacy Microsoft Office document formats.
        /// </summary>
        public static readonly HashSet<string> MicrosoftOfficeLegacyFileExtensions =
            [.. MicrosoftOfficeLegacyMap.Values.Distinct()];

        /// <summary>
        /// Provides a set of MIME content types that are recognized as binary document formats.
        /// </summary>
        public static readonly HashSet<string> BinaryDocumentContentTypes =
            [.. BinaryDocumentMap.Keys];

        /// <summary>
        /// Provides a set of file extensions that are recognized as binary document formats.
        /// </summary>
        public static readonly HashSet<string> BinaryDocumentFileExtensions =
            [.. BinaryDocumentMap.Values.Distinct()];

        /// <summary>
        /// Provides a set of MIME content types that are commonly used for archive files, such as ZIP and TAR formats.
        /// </summary>
        public static readonly HashSet<string> ArchiveContentTypes =
            [.. ArchiveMap.Keys];

        /// <summary>
        /// Provides a set of file extensions commonly associated with archive files.
        /// </summary>
        public static readonly HashSet<string> ArchiveFileExtensions =
            [.. ArchiveMap.Values.Distinct()];
    }
}
