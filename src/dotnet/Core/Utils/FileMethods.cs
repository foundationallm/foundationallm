using FoundationaLLM.Common.Constants.Orchestration;

namespace FoundationaLLM.Core.Utils
{
    /// <summary>
    /// Contains methods for working with files.
    /// </summary>
    public class FileMethods
    {
        private static readonly Dictionary<string, string> FileTypeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            //{ ".jpg", MessageContentItemTypes.ImageFile },
            //{ ".jpeg", MessageContentItemTypes.ImageFile },
            //{ ".png", MessageContentItemTypes.ImageFile },
            //{ ".gif", MessageContentItemTypes.ImageFile },
            //{ ".bmp", MessageContentItemTypes.ImageFile },
            //{ ".svg", MessageContentItemTypes.ImageFile },
            //{ ".webp", MessageContentItemTypes.ImageFile },
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
    }
}
