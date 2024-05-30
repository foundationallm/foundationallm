using System.Collections.Immutable;

namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Name constants used to identify file extension.
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Allowed file extensions for vectorization.
        /// </summary>
        public static readonly ImmutableArray<string> AllowedFileExtensions = [
            Text,
            JSON,
            Markdown,
            Word,
            PowerPoint,
            Excel,
            PDF,
            Wav,
            Mp3
        ];

        /// <summary>
        /// File extension for text files.
        /// </summary>
        public const string Text = ".txt";
        /// <summary>
        /// File extension for JSON files.
        /// </summary>
        public const string JSON = ".json";
        /// <summary>
        /// File extension for Markdown files.
        /// </summary>
        public const string Markdown = ".md";
        /// <summary>
        /// File extension for Microsoft Office Word files.
        /// </summary>
        public const string Word = ".docx";
        /// <summary>
        /// File extension for Microsoft Office PowerPoint files.
        /// </summary>
        public const string PowerPoint = ".pptx";
        /// <summary>
        /// File extension for Microsoft Office Excel files.
        /// </summary>
        public const string Excel = ".xlsx";
        /// <summary>
        /// File extension for PDF files.
        /// </summary>
        public const string PDF = ".pdf";
        /// <summary>
        /// File extension for wav files.
        /// </summary>
        public const string Wav = ".wav";
        /// <summary>
        /// File extension for mp3 files.
        /// </summary>
        public const string Mp3 = ".mp3";
    }
}
