using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;

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
        /// <returns>The properties of the code execution session.</returns>
        Task<CreateCodeSessionResponse> CreateCodeSession(
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
        /// <returns>The result of uploading the files to the code session in the form of a dictionary where the keys are the file names and the values are true/false.</returns>
        Task<CodeSessionFileUploadResponse> UploadFilesToCodeSession(
            string instanceId,
            string sessionId,
            CodeSessionFileUploadRequest request,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Downloads newly created files from a code session.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// /// <param name="sessionId">The identifier of the code session from where the files must be downloaded.</param>
        /// <param name="operationId">The code session file upload operation identifier.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity information.</param>
        /// <returns>The result of downloading the newly created in the form of a dictionary with file names and file identifiers.</returns>
        /// <remarks>
        /// By newly created files we mean files that were not uploaded to the code session but were created during the code execution.
        /// The <paramref name="operationId"/> is the identifier of the file upload operation that initially uploaded the files.
        /// </remarks>
        Task<CodeSessionFileDownloadResponse> DownloadFilesFromCodeSession(
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
        /// <returns>The result of the code execution including standard and error outputs.</returns>
        Task<CodeSessionCodeExecuteResponse> ExecuteCodeInCodeSession(
            string instanceId,
            string sessionId,
            string codeToExecute,
            UnifiedUserIdentity userIdentity);
    }
}
