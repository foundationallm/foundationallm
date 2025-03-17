using FoundationaLLM.Common.Models.Quota;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Represents a sequence of units of a quota metric.
    /// </summary>
    /// <param name="metricLimit">The limit of the metric for the time window specified by <paramref name="metricWindowSeconds"/>.</param>
    /// <param name="metricWindowSeconds">The time window for the metric limit.</param>
    /// <param name="lockoutDurationSeconds">The duration of the lockout period when the metric limit is exceeded.</param>
    public class QuotaMetricSequence(
        int metricLimit,
        int metricWindowSeconds,
        int lockoutDurationSeconds)
    {
        private readonly int _metricLimit = metricLimit;
        private readonly int _metricWindowSeconds = metricWindowSeconds;
        private readonly int _lockoutDurationSeconds = lockoutDurationSeconds;

        private const int METRIC_TIME_UNIT_SECONDS = 20;
        private readonly int _actualMetricWindowSeconds = METRIC_TIME_UNIT_SECONDS;
        private readonly int _actualMetricLimit = metricLimit / (metricWindowSeconds / METRIC_TIME_UNIT_SECONDS);

        private object _syncRoot = new();
        private readonly List<DateTimeOffset> _metricUnits = [];
        private bool _lockedOut = false;
        private DateTimeOffset _lockoutStartTime = DateTimeOffset.MinValue;

        /// <summary>
        /// Tries to add a unit to the metric sequence.
        /// </summary>
        /// <returns>False is the metric limit is exceeded.</returns>
        public QuotaMetricEvaluationResult AddUnitAndEvaluateMetric()
        {
            int metricCount = 0;

            lock (_syncRoot)
            {
                var refTime = DateTimeOffset.UtcNow;

                if (_lockedOut)
                {
                    if (refTime - _lockoutStartTime > TimeSpan.FromSeconds(_lockoutDurationSeconds))
                    {
                        // Lockout period has expired.
                        _lockedOut = false;
                        AddUnitAndTrim(refTime);
                    }

                    metricCount = _metricUnits.Count;
                }
                else
                {
                    AddUnitAndTrim(refTime);
                    metricCount = _metricUnits.Count;

                    if (metricCount > _actualMetricLimit)
                    {
                        _lockedOut = true;
                        _lockoutStartTime = refTime;
                        _metricUnits.Clear();
                    }
                }
            }

            return new QuotaMetricEvaluationResult
            {
                MetricCount = metricCount,
                LockedOut = _lockedOut,
                RemainingLockoutSeconds = _lockedOut
                    ? _lockoutDurationSeconds - (int)(DateTimeOffset.UtcNow - _lockoutStartTime).TotalSeconds
                    : 0
            };
        }

        private void AddUnitAndTrim(DateTimeOffset refTime)
        {
            _metricUnits.Add(refTime);

            for (int i = 0; i < _metricUnits.Count; i++)
            {
                if (_metricUnits[i] < refTime.AddSeconds(-_actualMetricWindowSeconds))
                {
                    _metricUnits.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
