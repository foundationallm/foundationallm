using FoundationaLLM.Common.Models.Configuration.Users;

namespace FoundationaLLM.Core.Interfaces
{
    /// <summary>
    /// Provides methods for working with user profiles.
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Returns the user profile of the signed in user.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task<UserProfile?> GetUserProfileAsync(string instanceId);

        /// <summary>
        /// Returns the user profile of the specified user.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="upn">The user principal name of the user for whom to return the user profile.</param>
        /// <returns></returns>
        Task<UserProfile?> GetUserProfileForUserAsync(string instanceId, string upn);

        /// <summary>
        /// Inserts or updates a user profile.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="userProfile">The user profile to upsert.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task UpsertUserProfileAsync(string instanceId, UserProfile userProfile);
    }
}
