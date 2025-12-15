using FoundationaLLM.Common.Models.Configuration.Users;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides methods for working with user profiles.
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Returns the user profile of the signed in user.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task<UserProfile?> GetUserProfileAsync(string instanceId);

        /// <summary>
        /// Returns the user profile of the specified user.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="upn">The user principal name of the user for whom to return the user profile.</param>
        /// <returns></returns>
        Task<UserProfile?> GetUserProfileForUserAsync(string instanceId, string upn);

        /// <summary>
        /// Inserts or updates a user profile.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="userProfile">The user profile to upsert.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task UpsertUserProfileAsync(string instanceId, UserProfile userProfile);

        /// <summary>
        /// Adds an agent to the user's profile.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="agentObjectId">The object identifier of the agent to add.</param>
        /// <returns></returns>
        Task AddAgentToUserProfileAsync(
            string instanceId,
            string agentObjectId);

        /// <summary>
        /// Removes an agent to the user's profile.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="agentObjectId">The object identifier of the agent to remove.</param>
        /// <returns></returns>
        Task RemoveAgentFromUserProfileAsync(
            string instanceId,
            string agentObjectId);

        /// <summary>
        /// Returns the user data of the signed in user.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task<UserData?> GetUserDataAsync(string instanceId);

        /// <summary>
        /// Inserts or updates a user data object.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="userData">The user data object to upsert.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task UpsertUserDataAsync(string instanceId, UserData userData);
    }
}
