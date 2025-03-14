namespace FoundationaLLM.Common.Constants.Quota
{
    /// <summary>
    /// Represents the type of quota.
    /// </summary>
    public enum QuotaType
    {
        /// <summary>
        /// Represents a quota that enforces a rate limit on raw API requests.
        /// </summary>
        RawRequestRateLimit,

        /// <summary>
        /// Represents a quota that enforces a rate limit on agent requests.
        /// </summary>
        AgentRequestRateLimit
    }
}
