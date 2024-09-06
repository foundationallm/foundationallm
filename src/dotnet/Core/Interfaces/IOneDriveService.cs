using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Core.Interfaces
{
    /// <summary>
    /// Interface for OneDrive integration.
    /// </summary>
    public interface IOneDriveService
    {
        /// <summary>
        /// Connects to user's OneDrive work or school account.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns></returns>
        Task Connect(string instanceId);

        /// <summary>
        /// Disconnect from user's OneDrive work or school account.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns></returns>
        Task Disconnect(string instanceId);

        /// <summary>
        /// Downloads a file from the user's connected OneDrive work or school account.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The session ID from which the file is uploaded.</param>
        /// <param name="agentName">The agent name.</param>
        /// <param name="itemId">The OneDrive work or school item identifier.</param>
        /// <param name="userIdentity">The user's identity.</param>
        /// <returns></returns>
        Task<ResourceProviderUpsertResult> Download(
            string instanceId, string sessionId, string agentName, string itemId, UnifiedUserIdentity userIdentity);
    }
}
