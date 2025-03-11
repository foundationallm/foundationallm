namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Implements the base class for managing the in-memory state of a quota context.
    /// </summary>
    public abstract class QuotaContextBase
    {
        /// <summary>
        /// The object used to synchronize access to the quota context.
        /// </summary>
        protected readonly object _syncRoot = new();

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        public required QuotaDefinition Quota { get; set; }

        /// <summary>
        /// Adds a new unit of the quota metric to the quota context.
        /// </summary>
        /// <param name="userIdentifier">The user identifier associated with the unit of the quota metric.</param>
        /// <param name="userPrincipalName">The user principal name associated with the unit of the quota metric.</param>
        /// <returns>The result of the quota evaluation.</returns>
        protected abstract QuotaEvaluationResult AddMetricUnit(
            string userIdentifier,
            string userPrincipalName);
    }
}
