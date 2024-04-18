namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides group membership services.
    /// </summary>
    public interface IGroupMembershipService
    {
        /// <summary>
        /// Retrieves the group identifiers list of the groups where the specified user principal is a member.
        /// </summary>
        /// <param name="userIdentifier">The user identifier for which group membership is retrieved. Can be either an object id or a user principal name (UPN).</param>
        /// <returns></returns>
        Task<List<string>> GetGroupsForPrincipal(string userIdentifier);
    }
}
