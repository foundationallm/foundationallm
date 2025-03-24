using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the service interface for the FoundationaLLM File service.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Create a new file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="origin">The origin of the file.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="fileName">The original name of the file.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="content">The <see cref="Stream"/> providing the raw content of the file.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        /// <param name="metadata">Optional metadata dictionary associated with the file.</param>
        /// <returns>A <see cref="ContextFileRecord"/> instance with details about the newly created file.</returns>
        Task<ContextFileRecord> CreateFile(
            string instanceId,
            string origin,
            string conversationId,
            string fileName,
            string contentType,
            Stream content,
            UnifiedUserIdentity userIdentity,
            Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Get the binary content of a file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="fileName">The name of the file to retrieve.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        /// <returns>A stream with the binary content of the file.</returns>
        Task<Stream?>GetFileContent(
            string instanceId,
            string conversationId,
            string fileName,
            UnifiedUserIdentity userIdentity);
    }
}
