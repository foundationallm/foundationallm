using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Orchestration;
using FoundationaLLM.Common.Models.Files;
using Microsoft.AspNetCore.StaticFiles;
using MimeDetective;
using System.Collections.Immutable;

namespace FoundationaLLM.Common.Utils
{
    /// <summary>
    /// Contains methods for working with files.
    /// </summary>
    public class FileUtils
    {
        private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

        static FileUtils()
        {
            // Initialize any static resources or configurations if needed.

            ContentTypeProvider.Mappings.Add(".py", "text/x-python");
            ContentTypeProvider.Mappings.Add(".jsonl", "application/x-ndjson");
            ContentTypeProvider.Mappings.Add(".php", "application/x-httpd-php");
            ContentTypeProvider.Mappings.Add(".rb", "text/x-ruby");

            // Replace mapping values with values relevant to our application.
            ContentTypeProvider.Mappings[".c"] = "text/x-c";
            ContentTypeProvider.Mappings[".cpp"] = "text/x-c++src";
            ContentTypeProvider.Mappings[".cs"] = "text/x-csharp";
            ContentTypeProvider.Mappings[".ini"] = "text/x-ini";
            ContentTypeProvider.Mappings[".java"] = "text/x-java-source";
            ContentTypeProvider.Mappings[".toml"] = "application/toml";
            ContentTypeProvider.Mappings[".yaml"] = "application/yaml";
            ContentTypeProvider.Mappings[".yml"] = "application/yaml";
            ContentTypeProvider.Mappings[".ts"] = "application/typescript";
        }

        private static readonly Dictionary<string, string> FileTypeMappings = new(StringComparer.OrdinalIgnoreCase)
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
        /// Determines the MIME content type of a file based on its name and binary content.
        /// </summary>
        /// <remarks>If the content type cannot be determined from the file's content, the method falls
        /// back to using the file extension. If neither can be determined, 'application/octet-stream' is returned as a
        /// default. The method attempts to resolve discrepancies between the file's content and its extension, favoring
        /// the extension for plain text files when appropriate.</remarks>
        /// <param name="fileName">The name of the file, including its extension. Used to infer the expected content type.</param>
        /// <param name="fileContent">The binary data representing the contents of the file. Used to inspect and detect the actual content type.</param>
        /// <returns>A FileContentTypeResult containing the detected MIME type, a value indicating whether the type is supported,
        /// and whether the detected type matches the file extension.</returns>
        public static FileContentTypeResult GetFileContentType(
            string fileName,
            BinaryData fileContent)
        {
            var fileExtension = Path.GetExtension(fileName).Replace(".", string.Empty).ToUpper();
            if (ContentTypeMappings.FileExtensionMap.TryGetValue(fileExtension, out var canonicalFileExtension))
                fileExtension = canonicalFileExtension;

            // Detect the mime type from the file data using Mime-Detective.
            var inspector = GetContentInspector();

            var results = inspector.Inspect(fileContent.ToArray().ToImmutableArray());
            var contentMimeType = (results.FirstOrDefault()?.Definition.File.MimeType)
                ?? (IsProbablyText(
                    // Use the first 64kb for textual heuristics.
                    fileContent.ToMemory()[.. Math.Min(64 * 1024, fileContent.ToMemory().Length)])
                    ? "text/plain"
                    : "application/octet-stream");
            var extensionMimeType =
                ContentTypeProvider.TryGetContentType(fileName, out var contentType)
                ? contentType
                : "application/octet-stream";

            if (ContentTypeMappings.Map.TryGetValue(contentMimeType, out var detectedFileExtension))
            {
                if (detectedFileExtension == "TXT"
                    && ContentTypeMappings.TextFileExtensions.Contains(fileExtension)
                    && extensionMimeType != "application/octet-stream")
                {
                    // The content is detected as plain text and the file extension is a known text type.
                    // Attempt to identify a more specific text content type using the file's extension.

                    if (ContentTypeMappings.Map.TryGetValue(extensionMimeType, out var detectedFileExtension2))
                    {
                        return new FileContentTypeResult
                        {
                            ContentType = extensionMimeType,
                            IsSupported = true,
                            MatchesExtension = string.Equals(
                                fileExtension,
                                detectedFileExtension2)
                        };
                    }
                    else
                        return new FileContentTypeResult
                        {
                            ContentType = extensionMimeType,
                            IsSupported = false,
                            MatchesExtension = false
                        };
                }
                else
                    return new FileContentTypeResult
                    {
                        ContentType = contentMimeType,
                        IsSupported = true,
                        MatchesExtension = string.Equals(
                            fileExtension,
                            detectedFileExtension)
                    };
            }
            else
                return new FileContentTypeResult
                {
                    ContentType = contentMimeType,
                    IsSupported = false,
                    MatchesExtension = false
                };
        }

        /// <summary>
        /// Lightweight heuristic to determine if data is probably text.
        /// Considers UTF-8 BOM, absence of NUL bytes, and low ratio of control characters.
        /// </summary>
        private static bool IsProbablyText(ReadOnlyMemory<byte> data)
        {
            if (data.Length == 0)
                return true;
            var span = data.Span;

            // UTF-8 BOM
            if (data.Length >= 3 && span[0] == 0xEF && span[1] == 0xBB && span[2] == 0xBF)
                return true;

            // Reject obvious binary (NUL bytes)
            for (int i = 0; i < span.Length && i < 64 * 1024; i++)
            {
                if (span[i] == 0x00)
                    return false;
            }

            // Count control characters excluding common whitespace: \t, \r, \n
            int controlCount = 0;
            int sampleCount = Math.Min(data.Length, 64 * 1024);
            for (int i = 0; i < sampleCount; i++)
            {
                var b = span[i];
                // ASCII control range 0x00-0x1F and 0x7F
                bool isControl = (b < 0x20 && b != (byte)'\t' && b != (byte)'\r' && b != (byte)'\n') || b == 0x7F;
                if (isControl)
                {
                    controlCount++;
                }
            }

            // If less than 1% of sampled bytes are control chars, treat as text.
            var ratio = (double)controlCount / sampleCount;
            return ratio < 0.01;
        }

        private static ContentInspector GetContentInspector() =>
            new ContentInspectorBuilder()
            {
                Definitions = [.. MimeDetective.Definitions.Default
                    .All()
                    // Remove EML because it misidentifies PY files as EML.
                    .Where(d =>
                        !d.File.Extensions.Any(ext => string.Equals(ext, "eml", StringComparison.OrdinalIgnoreCase)))]
            }
            .Build();
    }
}
