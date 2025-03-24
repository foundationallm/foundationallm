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
    }
}
