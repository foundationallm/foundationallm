namespace FoundationaLLM.Common.Constants.Quota
{
    /// <summary>
    /// Represents the type of partitioning applied to the metric based on which a quota is enforced.
    /// </summary>
    public enum QuotaMetricPartition
    {
        /// <summary>
        /// No partitioning.
        /// </summary>
        None,

        /// <summary>
        /// Partitioning based on the user identifier.
        /// </summary>
        UserIdentifier,

        /// <summary>
        /// Partitioning based on the user principal name.
        /// </summary>
        UserPrincipalName
    }
}
