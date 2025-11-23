using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Common.Models.Services;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Definest the interface for the FoundationaLLM Code Session service.
    /// </summary>
    public interface ICodeSessionService
    {
        /// <summary>
        /// Creates a new code session using one of the registered code session providers.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="codeSessionRequest">The <see cref="CreateCodeSessionRequest"/> providing the details of the request to create the code session.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity information.</param>
        /// <returns>A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="CreateCodeSessionResponse"/> on success; otherwise a failed <see cref="Result{T}"/> with a domain error.</returns>
        Task<Result<CreateCodeSessionResponse>> CreateCodeSession(
            string instanceId,
            CreateCodeSessionRequest codeSessionRequest,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Uploads files to a code session.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The identifier of the code session where the files must be uploaded.</param>
        /// <param name="request">The <see cref="CodeSessionFileUploadRequest"/> providing the details of the request.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity information.</param>
        /// <returns>A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="CodeSessionFileUploadResponse"/> containing per-file upload success data; otherwise a failed result with a domain error.</returns>
        Task<Result<CodeSessionFileUploadResponse>> UploadFilesToCodeSession(
            string instanceId,
            string sessionId,
            CodeSessionFileUploadRequest request,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Downloads newly created files from a code session.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The identifier of the code session from where the files must be downloaded.</param>
        /// <param name="operationId">The code session file upload operation identifier.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity information.</param>
        /// <returns>A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="CodeSessionFileDownloadResponse"/> containing downloaded file records and any errors; otherwise a failed result with a domain error.</returns>
        /// <remarks>
        /// By newly created files we mean files that were not uploaded to the code session but were created during the code execution.
        /// The <paramref name="operationId"/> is the identifier of the file upload operation that initially uploaded the files.
        /// </remarks>
        Task<Result<CodeSessionFileDownloadResponse>> DownloadFilesFromCodeSession(
            string instanceId,
            string sessionId,
            string operationId,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Executes code in a code session and returns the result of the execution.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The identifier of the code session where the code must be executed.</param>
        /// <param name="codeToExecute">The code to execute.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity information.</param>
        /// <returns>A <see cref="Task"/> producing a <see cref="Result{T}"/> whose value is a <see cref="CodeSessionCodeExecuteResponse"/> with execution status, result, standard output, and error output; otherwise a failed result with a domain error.</returns>
        Task<Result<CodeSessionCodeExecuteResponse>> ExecuteCodeInCodeSession(
            string instanceId,
            string sessionId,
            string codeToExecute,
            UnifiedUserIdentity userIdentity);
    }
}
