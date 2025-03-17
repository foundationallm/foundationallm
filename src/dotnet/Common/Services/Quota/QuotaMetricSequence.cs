using FoundationaLLM.Common.Models.Quota;
using Microsoft.Extensions.Azure;
using OpenTelemetry.Metrics;

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
        private int _metricUnitsSum = 0;
        /// <summary>
        /// The last time the <see cref="_metricUnits"/> array was shifted.
        /// </summary>
        private DateTimeOffset _metricUnitsLastShiftTime = DateTimeOffset.MinValue;
        /// <summary>
        /// Holds the number of metric units for each one-second interval that passed before <see cref="_metricUnitsLastShiftTime"/>.
        /// </summary>
        private readonly int[] _metricUnits = new int[METRIC_TIME_UNIT_SECONDS];
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
                        ShiftAndAddLocalUnit(refTime);
                    }

                    metricCount = _metricUnitsSum;
                }
                else
                {
                    ShiftAndAddLocalUnit(refTime);
                    metricCount = _metricUnitsSum;

                    if (metricCount > _actualMetricLimit)
                    {
                        _lockedOut = true;
                        _lockoutStartTime = refTime;
                        Array.Clear(_metricUnits, 0, METRIC_TIME_UNIT_SECONDS);
                        _metricUnitsSum = 0;
                    }
                }
            }

            return new QuotaMetricEvaluationResult
            {
                TotalMetricCount = metricCount,
                LockedOut = _lockedOut,
                RemainingLockoutSeconds = _lockedOut
                    ? _lockoutDurationSeconds - (int)(DateTimeOffset.UtcNow - _lockoutStartTime).TotalSeconds
                    : 0
            };
        }

        private void ShiftAndAddLocalUnit(DateTimeOffset refTime)
        {
            // Shift the array to align with the new reference time.

            var fractionalSeconds = (refTime - _metricUnitsLastShiftTime).TotalSeconds;
            if (fractionalSeconds >= 1)
            {
                // More than one second passed since the last unit was added, so we need to shift the one-second buckets.
                var roundedSeconds = (int) Math.Ceiling(fractionalSeconds);

                if (roundedSeconds >= METRIC_TIME_UNIT_SECONDS)
                {
                    // No need to shift anything, too much time has passed since the last unit was added.
                    Array.Clear(_metricUnits, 0, METRIC_TIME_UNIT_SECONDS);
                    _metricUnitsSum = 0;
                }
                else
                {
                    _metricUnitsSum = 0;
                    for (int i = METRIC_TIME_UNIT_SECONDS - 1; i >= roundedSeconds; i--)
                    {
                        _metricUnits[i] = _metricUnits[i - roundedSeconds];
                        _metricUnitsSum += _metricUnits[i];
                    }
                    Array.Clear(_metricUnits, 0, roundedSeconds);
                }

                _metricUnitsLastShiftTime = refTime;
            }

            // Add the local unit to the metric units.
            _metricUnits[0]++;
            _metricUnitsSum++;
        }
    }
}
