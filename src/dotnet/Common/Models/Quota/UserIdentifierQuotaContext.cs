namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Implements a quota context partitioned by user identifier.
    /// </summary>
    public class UserIdentifierQuotaContext : QuotaContextBase
    {
        private readonly Dictionary<string, QuotaMetricSequence> _metrics = [];

        /// <inheritdoc/>
        protected override void AddMetricUnit(string userIdentifier, string userPrincipalName)
        {
            if (!_metrics.ContainsKey(userIdentifier))
            {
                lock(_syncRoot)
                {
                    // Ensure that the key is still not present after acquiring the lock.
                    if (!_metrics.ContainsKey(userIdentifier))
                    {
                        _metrics[userIdentifier] = new();
                    }
                }
            }

            _metrics[userIdentifier].AddUnit();
        }
    }
}
