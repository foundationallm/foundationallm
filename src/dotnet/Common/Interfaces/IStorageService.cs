using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides services to interact with a storage.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Reads the binary content of a specified file from the storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to read.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns>The binary content of the file.</returns>
        Task<BinaryData> ReadFileAsync(string containerName, string filePath, CancellationToken cancellationToken);

        /// <summary>
        /// Writes the binary content to a specified file from the storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to read.</param>
        /// <param name="fileContent">The binary content written to the file.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns></returns>
        Task WriteFileAsync(string containerName, string filePath, Stream fileContent, CancellationToken cancellationToken);

        /// <summary>
        /// Writes the string content to a specified file from the storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to read.</param>
        /// <param name="fileContent">The string content written to the file.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns></returns>
        Task WriteFileAsync(string containerName, string filePath, string fileContent, CancellationToken cancellationToken);

        /// <summary>
        /// Checks if a file exists on the storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to read.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns></returns>
        Task<bool> FileExistsAsync(string containerName, string filePath, CancellationToken cancellationToken);
    }
}
