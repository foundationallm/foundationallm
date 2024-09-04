using FoundationaLLM.Common.Models.Authentication;

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
        /// <param name="itemId">The OneDrive work or school item identifier.</param>
        /// <param name="userIdentity">The user's identity.</param>
        /// <returns>The file contents.</returns>
        Task<string> Download(string instanceId, string itemId, UnifiedUserIdentity userIdentity);
    }
}
