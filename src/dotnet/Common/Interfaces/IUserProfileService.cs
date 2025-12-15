using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

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
        /// <param name="agents">The list of agents the signed in user has permission to use.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <remarks>The <paramref name="agents"/> parameter is provided when the caller seeks to avoid
        /// reloading the list of agents the user has permission to use. It is the responsibility
        /// of the caller to ensure the list is accurate.</remarks>
        Task<UserProfile?> GetUserProfileAsync(
            string instanceId,
            List<ResourceProviderGetResult<AgentBase>>? agents = null);

        /// <summary>
        /// Returns the user profile of the specified user.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="upn">The user principal name of the user for whom to return the user profile.</param>
        /// <returns></returns>
        Task<UserProfile?> GetUserProfileForUserAsync(
            string instanceId,
            string upn);

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
