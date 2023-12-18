using FoundationaLLM.Common.Models.Configuration.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task<UserProfile?> GetUserProfileAsync();

        /// <summary>
        /// Returns the user profile of the specified user.
        /// </summary>
        /// <param name="upn">The user principal name of the user for whom to return the user profile.</param>
        /// <returns></returns>
        Task<UserProfile?> GetUserProfileForUserAsync(string upn);

        /// <summary>
        /// Inserts or updates a user profile.
        /// </summary>
        /// <param name="userProfile">The user profile to upsert.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task UpsertUserProfileAsync(UserProfile userProfile);
    }
}
