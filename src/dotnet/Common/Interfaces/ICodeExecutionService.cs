using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines the capabilities for code execution services.
    /// </summary>
    public interface ICodeExecutionService
    {
        /// <summary>
        /// Creates a new code execution session.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="context">The context in which the code execution session is created. This is usually the name of the agent tool, but it is not limited to that.</param>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity information.</param>
        /// <returns>A <see cref="CodeExecutionSession"/> object with the properties of the code execution session.</returns>
        Task<CodeExecutionSession> CreateCodeExecutionSession(
            string instanceId,
            string context,
            string conversationId,
            UnifiedUserIdentity userIdentity);
    }
}
