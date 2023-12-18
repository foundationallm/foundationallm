using System;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Provides access to a content source.
    /// </summary>
    public interface IContentSourceService
    {
        /// <summary>
        /// Reads the binary content of a specified document from the content source.
        /// </summary>
        /// <param name="filePath">The path of the document to read.</param>
        /// <returns>The binary content of the document.</returns>
        Task<BinaryData> ReadFileAsync(string filePath);
    }
}
