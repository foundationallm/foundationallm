namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Interface for the GraphService.
    /// </summary>
    public interface IGraphService
    {
        /// <summary>
        /// Retrieves the group memberships of a user identified by their user principal name (UPN).
        /// </summary>
        /// <param name="upn">The user principal name (UPN) of the user.</param>
        /// <returns>A list of membership IDs.</returns>
        Task<List<string>> GetMemberships(string upn);
    }
}
