using FoundationaLLM.Common.Models.Plugins;

namespace FoundationaLLM.Common.Interfaces.Plugins
{
    /// <summary>
    /// Defines the interface for a content text extraction plugin.
    /// </summary>
    public interface IContentTextExtractionPlugin
    {
        /// <summary>
        /// Extracts text from the provided raw content.
        /// </summary>
        /// <param name="rawContent">The binary content to extract text from.</param>
        /// <returns>A <see cref="PluginResult{T}"/> object with the extracted text.</returns>
        Task<PluginResult<string>> ExtractText(
            BinaryData rawContent);
    }
}
