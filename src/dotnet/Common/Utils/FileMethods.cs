using FoundationaLLM.Common.Constants.Orchestration;
using Microsoft.AspNetCore.StaticFiles;
using MimeDetective;

namespace FoundationaLLM.Common.Utils
{
    /// <summary>
    /// Contains methods for working with files.
    /// </summary>
    public class FileMethods
    {
        private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

        static FileMethods()
        {
            // Initialize any static resources or configurations if needed.

            ContentTypeProvider.Mappings.Add(".py", "text/x-python");
            ContentTypeProvider.Mappings.Add(".jsonl", "application/x-ndjson");
            ContentTypeProvider.Mappings.Add(".php", "application/x-httpd-php");
            ContentTypeProvider.Mappings.Add(".rb", "text/x-ruby");

            // Not really interested in the default mapping for this extension.
            ContentTypeProvider.Mappings[".ts"] = "application/typescript";
        }

        private static readonly Dictionary<string, string> FileTypeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", MessageContentItemTypes.ImageFile },
            { ".jpeg", MessageContentItemTypes.ImageFile },
            { ".png", MessageContentItemTypes.ImageFile },
            { ".gif", MessageContentItemTypes.ImageFile },
            { ".bmp", MessageContentItemTypes.ImageFile },
            { ".svg", MessageContentItemTypes.ImageFile },
            { ".webp", MessageContentItemTypes.ImageFile },
            { ".html", MessageContentItemTypes.HTML },
            { ".htm", MessageContentItemTypes.HTML }
        };

        /// <summary>
        /// Returns the type of the message content based on the file name.
        /// </summary>
        /// <param name="fileName">The file name to evaluate.</param>
        /// <param name="fallbackValue">If populated, defines the fallback type value
        /// if a mapping cannot be determined from the passed in file name. Otherwise,
        /// the default value is <see cref="MessageContentItemTypes.FilePath"/>.</param>
        /// <returns></returns>
        public static string GetMessageContentFileType(string? fileName, string? fallbackValue)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return fallbackValue ?? MessageContentItemTypes.FilePath;
            }
            var extension = Path.GetExtension(fileName);

            return FileTypeMappings.GetValueOrDefault(extension, fallbackValue ?? MessageContentItemTypes.FilePath);
        }

        /// <summary>
        /// Returns the mime type of the file data.
        /// </summary>
        /// <param name="fileData">The byte array for the file to inspect.</param>
        /// <returns></returns>
        public static string GetMimeType(byte[] fileData)
        {
            // Detect the mime type from the file data using Mime-Detective.
            var Inspector = new ContentInspectorBuilder()
            {
                Definitions = MimeDetective.Definitions.Default.All()
            }.Build();

            var results = Inspector.Inspect(fileData);
            var mimeType = results.FirstOrDefault()?.Definition.File.MimeType;

            return mimeType ?? "application/octet-stream";
        }

        /// <summary>
        /// Returns the mime type of the file based on its name.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The mime type.</returns>
        public static string GetMimeType(string fileName)
        {
            if (ContentTypeProvider.TryGetContentType(fileName, out var contentType))
            {
                return contentType;
            }

            return "application/octet-stream";
        }
    }
}
