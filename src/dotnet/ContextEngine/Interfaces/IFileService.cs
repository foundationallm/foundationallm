using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Services;

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
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="fileName">The original name of the file.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="content">The <see cref="Stream"/> providing the raw content of the file.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        /// <param name="metadata">Optional metadata dictionary associated with the file.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ContextFileRecord"/> describing the newly created file when successful; otherwise a failed result containing a domain error.
        /// </returns>
        Task<Result<ContextFileRecord>> CreateFileForConversation(
            string instanceId,
            string origin,
            string? agentName,
            string conversationId,
            string fileName,
            string contentType,
            Stream content,
            UnifiedUserIdentity userIdentity,
            Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Create a new file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="origin">The origin of the file.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="fileName">The original name of the file.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="content">The <see cref="Stream"/> providing the raw content of the file.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        /// <param name="metadata">Optional metadata dictionary associated with the file.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ContextFileRecord"/> describing the newly created file when successful; otherwise a failed result containing a domain error.
        /// </returns>
        Task<Result<ContextFileRecord>> CreateFileForAgent(
            string instanceId,
            string origin,
            string agentName,
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
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ContextFileContent"/> containing the file's binary content when successful; otherwise a failed result containing a domain error.
        /// </returns>
        Task<Result<ContextFileContent>> GetFileContent(
            string instanceId,
            string conversationId,
            string fileName,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Get the binary content of a file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The identifier of the file.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ContextFileContent"/> containing the file's binary content when successful; otherwise a failed result containing a domain error.
        /// </returns>
        Task<Result<ContextFileContent>> GetFileContent(
            string instanceId,
            string fileId,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Get the file record associated with a file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The identifier of the file.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="ContextFileRecord"/> representing the file record when successful; otherwise a failed result containing a domain error.
        /// </returns>
        Task<Result<ContextFileRecord>> GetFileRecord(
            string instanceId,
            string fileId,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Deletes the file record associated with a file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The identifier of the file.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        /// <returns>
        /// A <see cref="Task"/> producing a <see cref="Result"/> indicating success when the record is deleted; otherwise a failed result containing a domain error.
        /// </returns>
        Task<Result> DeleteFileRecord(
            string instanceId,
            string fileId,
            UnifiedUserIdentity userIdentity);
    }
}
