namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Implements a quota context partitioned by user principal name.
    /// </summary>
    public class UserPrincipalNameQuotaContext : QuotaContextBase
    {
        private readonly Dictionary<string, QuotaMetricSequence> _metrics = [];

        /// <inheritdoc/>
        protected override void AddMetricUnit(string userIdentifier, string userPrincipalName)
        {
            if (!_metrics.ContainsKey(userPrincipalName))
            {
                lock (_syncRoot)
                {
                    // Ensure that the key is still not present after acquiring the lock.
                    if (!_metrics.ContainsKey(userPrincipalName))
                    {
                        _metrics[userPrincipalName] = new();
                    }
                }
            }

            _metrics[userPrincipalName].AddUnit();
        }
    }
}
