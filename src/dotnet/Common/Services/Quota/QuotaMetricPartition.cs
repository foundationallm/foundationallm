using FoundationaLLM.Common.Models.Quota;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Represents a partition of a quota metric.
    /// </summary>
    /// <param name="quotaName">The name of the quota that this quota metric partition refers to.</param>
    /// <param name="quotaContext">The quota context that this quota metric partition refers to.</param>
    /// <param name="quotaMetricPartitionId">The identifier of the quota metric partion that this sequence refers to.</param>
    /// <param name="metricLimit">The limit of the metric for the time window specified by <paramref name="metricWindowSeconds"/>.</param>
    /// <param name="metricWindowSeconds">The time window for the metric limit.</param>
    /// <param name="lockoutDurationSeconds">The duration of the lockout period when the metric limit is exceeded.</param>
    public class QuotaMetricPartition(
        string quotaName,
        string quotaContext,
        string quotaMetricPartitionId,
        int metricLimit,
        int metricWindowSeconds,
        int lockoutDurationSeconds)
    {
        private readonly string _quotaName = quotaName;
        private readonly string _quotaContext = quotaContext;
        private readonly string _quotaMetricPartitionId = quotaMetricPartitionId;
        private readonly int _metricLimit = metricLimit;
        private readonly int _metricWindowSeconds = metricWindowSeconds;
        private readonly int _lockoutDurationSeconds = lockoutDurationSeconds;

        private const int METRIC_TIME_UNIT_SECONDS = 20;
        private readonly int _actualMetricWindowSeconds = METRIC_TIME_UNIT_SECONDS;
        private readonly int _actualMetricLimit = metricLimit / (metricWindowSeconds / METRIC_TIME_UNIT_SECONDS);

        private object _syncRoot = new();
        private int _localMetricUnitsCount = 0;
        private int _remoteMetricUnitsCount = 0;
        /// <summary>
        /// The last time the metric units arrays were shifted.
        /// </summary>
        private DateTimeOffset _metricUnitsLastShiftTime = DateTimeOffset.MinValue;
        /// <summary>
        /// Holds the number of local metric units for each one-second interval that passed before <see cref="_metricUnitsLastShiftTime"/>.
        /// </summary>
        private readonly int[] _localMetricUnits = new int[METRIC_TIME_UNIT_SECONDS];
        /// <summary>
        /// Holds the number of remote metric units for each one-second interval that passed before <see cref="_metricUnitsLastShiftTime"/>.
        /// </summary>
        private readonly int[] _remoteMetricUnits = new int[METRIC_TIME_UNIT_SECONDS];
        private bool _lockedOut = false;
        private DateTimeOffset _lockoutStartTime = DateTimeOffset.MinValue;

        /// <summary>
        /// The identifier of the quota metric partition that this sequence refers to.
        /// </summary>
        /// <remarks>
        /// The value of the identifier is only relevant within the parent quota context (where it is unique).
        /// </remarks>
        public string QuotaMetricPartitionId => _quotaMetricPartitionId;

        /// <summary>
        /// Adds a local metric unit to the metric partition.
        /// </summary>
        /// <param name="referenceTime">The time at which the local metric unit was recorded.</param>
        /// <returns>A <see cref="QuotaMetricPartitionState"/> instance with the current state of the metric partition.</returns>
        public QuotaMetricPartitionState AddLocalMetricUnit(
            DateTimeOffset referenceTime)
        {
            int metricCount = 0;
            int localMetricCount = 0;
            int remoteMetricCount = 0;

            lock (_syncRoot)
            {
                if (_lockedOut)
                {
                    if (referenceTime - _lockoutStartTime > TimeSpan.FromSeconds(_lockoutDurationSeconds))
                    {
                        // Lockout period has expired.
                        _lockedOut = false;
                        ShiftAndAddLocalUnit(referenceTime);
                    }

                    localMetricCount = _localMetricUnitsCount;
                    remoteMetricCount = _remoteMetricUnitsCount;
                    metricCount = localMetricCount + remoteMetricCount;
                }
                else
                {
                    ShiftAndAddLocalUnit(referenceTime);

                    localMetricCount = _localMetricUnitsCount;
                    remoteMetricCount = _remoteMetricUnitsCount;
                    metricCount = localMetricCount + remoteMetricCount;

                    if (metricCount > _actualMetricLimit)
                    {
                        _lockedOut = true;
                        _lockoutStartTime = referenceTime;
                        Array.Clear(_localMetricUnits, 0, METRIC_TIME_UNIT_SECONDS);
                        _localMetricUnitsCount = 0;
                        _remoteMetricUnitsCount = 0;
                    }
                }
            }

            return new QuotaMetricPartitionState
            {
                QuotaName = _quotaName,
                QuotaContext = _quotaContext,
                QuotaMetricPartitionId = _quotaMetricPartitionId,
                QuotaExceeded = _lockedOut,
                // Add a small buffer of 5 seconds to the lockout duration to avoid race conditions at the limit.
                TimeUntilRetrySeconds = _lockoutDurationSeconds - (int)(DateTimeOffset.UtcNow - _lockoutStartTime).TotalSeconds + 5,
                LocalMetricCount = localMetricCount,
                RemoteMetricCount = remoteMetricCount,
                TotalMetricCount = metricCount
            };
        }

        /// <summary>
        /// Adds remote metric units to the metric partition.
        /// </summary>
        /// <param name="referenceTimes">The times at which the remote metric units were recorded.</param>
        /// <returns>A <see cref="QuotaMetricPartitionState"/> instance with the current state of the metric partition.</returns>
        public QuotaMetricPartitionState AddRemoteMetricUnits(
            List<DateTimeOffset> referenceTimes)
        {
            int metricCount = 0;
            int localMetricCount = 0;
            int remoteMetricCount = 0;

            lock (_syncRoot)
            {
                if (!_lockedOut)
                {
                    foreach (var referenceTime in referenceTimes)
                    {
                        // TODO: Implement the logic to add remote metric units.
                    }
                }

                localMetricCount = _localMetricUnitsCount;
                remoteMetricCount = _remoteMetricUnitsCount;
                metricCount = localMetricCount + remoteMetricCount;
            }

            return new QuotaMetricPartitionState
            {
                QuotaName = _quotaName,
                QuotaContext = _quotaContext,
                QuotaMetricPartitionId = _quotaMetricPartitionId,
                QuotaExceeded = _lockedOut,
                // Not relevant for remote units updates.
                TimeUntilRetrySeconds = 0,
                LocalMetricCount = localMetricCount,
                RemoteMetricCount = remoteMetricCount,
                TotalMetricCount = metricCount
            };
        }

        private void ShiftAndAddLocalUnit(DateTimeOffset refTime)
        {
            // Shift the array to align with the new reference time.

            var fractionalSeconds = (refTime - _metricUnitsLastShiftTime).TotalSeconds;
            if (fractionalSeconds >= 1)
            {
                // More than one second passed since the last unit was added, so we need to shift the one-second buckets.

                if (fractionalSeconds >= METRIC_TIME_UNIT_SECONDS)
                {
                    // No need to shift anything, too much time has passed since the last unit was added.
                    Array.Clear(_localMetricUnits, 0, METRIC_TIME_UNIT_SECONDS);
                    _localMetricUnitsCount = 0;
                    _remoteMetricUnitsCount = 0;
                }
                else
                {
                    _localMetricUnitsCount = 0;
                    _remoteMetricUnitsCount = 0;

                    var roundedSeconds = (int)Math.Ceiling(fractionalSeconds);

                    for (int i = METRIC_TIME_UNIT_SECONDS - 1; i >= roundedSeconds; i--)
                    {
                        _localMetricUnits[i] = _localMetricUnits[i - roundedSeconds];
                        _localMetricUnitsCount += _localMetricUnits[i];

                        _remoteMetricUnits[i] = _remoteMetricUnits[i - roundedSeconds];
                        _remoteMetricUnitsCount += _remoteMetricUnits[i];
                    }
                    Array.Clear(_localMetricUnits, 0, roundedSeconds);
                    Array.Clear(_remoteMetricUnits, 0, roundedSeconds);
                }

                _metricUnitsLastShiftTime = refTime;
            }

            // Add the local unit to the metric units.
            _localMetricUnits[0]++;
            _localMetricUnitsCount++;
        }
    }
}
