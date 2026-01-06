namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Enumeration of ways to sort users.
    /// </summary>
    public enum UserSortBy
    {
        /// <summary>
        /// Sort by total requests.
        /// </summary>
        Requests,

        /// <summary>
        /// Sort by total tokens.
        /// </summary>
        Tokens,

        /// <summary>
        /// Sort by active sessions.
        /// </summary>
        Sessions,

        /// <summary>
        /// Sort by abuse risk score.
        /// </summary>
        RiskScore
    }
}
