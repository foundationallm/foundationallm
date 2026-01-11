using FoundationaLLM.Common.Models.Quota;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Represents a partition of a quota metric.
    /// </summary>
    /// <param name="quotaServiceIdentifier">The identifier of the QuotaService instance managing this quota context.</param>
    /// <param name="quotaName">The name of the quota that this quota metric partition refers to.</param>
    /// <param name="quotaContext">The quota context that this quota metric partition refers to.</param>
    /// <param name="quotaMetricPartitionId">The identifier of the quota metric partion that this sequence refers to.</param>
    /// <param name="metricLimit">The limit of the metric for the time window specified by <paramref name="metricWindowSeconds"/>.</param>
    /// <param name="metricWindowSeconds">The time window for the metric limit.</param>
    /// <param name="lockoutDurationSeconds">The duration of the lockout period when the metric limit is exceeded.</param>
    /// <param name="logger">The logger instance to use for logging.</param>
    public class QuotaMetricPartition(
        string quotaServiceIdentifier,
        string quotaName,
        string quotaContext,
        string quotaMetricPartitionId,
        int metricLimit,
        int metricWindowSeconds,
        int lockoutDurationSeconds,
        ILogger logger)
    {
        private readonly string _quotaServiceIdentifier = quotaServiceIdentifier;
        private readonly string _quotaName = quotaName;
        private readonly string _quotaContext = quotaContext;
        private readonly string _quotaMetricPartitionId = quotaMetricPartitionId;
        private readonly int _lockoutDurationSeconds = lockoutDurationSeconds;
        private readonly ILogger _logger = logger;

        private const int METRIC_TIME_UNIT_SECONDS = 20;
        private readonly int _actualMetricWindowSeconds = metricWindowSeconds;
        private readonly int _actualMetricLimit = metricLimit;

        private readonly object _syncRoot = new();
        private int _localMetricUnitsCount = 0;
        private int _remoteMetricUnitsCount = 0;
        /// <summary>
        /// The last time the metric units arrays were shifted.
        /// </summary>
        private DateTimeOffset _metricUnitsLastShiftTime = DateTimeOffset.MinValue;
        /// <summary>
        /// Holds the number of local metric units for each one-second interval that passed before <see cref="_metricUnitsLastShiftTime"/>.
        /// </summary>
        private readonly int[] _localMetricUnits = new int[metricWindowSeconds];
        /// <summary>
        /// Holds the number of remote metric units for each one-second interval that passed before <see cref="_metricUnitsLastShiftTime"/>.
        /// </summary>
        private readonly int[] _remoteMetricUnits = new int[metricWindowSeconds];
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
        /// Gets the current state of the metric partition for reporting purposes.
        /// </summary>
        /// <returns>A <see cref="QuotaMetricPartitionDisplayState"/> with the current state.</returns>
        public QuotaMetricPartitionDisplayState GetCurrentState()
        {
            lock (_syncRoot)
            {
                var localReferenceTime = DateTimeOffset.UtcNow;
                var lockoutRemainingSeconds = 0;
                var isLockedOut = _lockedOut;

                if (_lockedOut)
                {
                    var elapsedSinceLockout = (int)(localReferenceTime - _lockoutStartTime).TotalSeconds;
                    if (elapsedSinceLockout >= _lockoutDurationSeconds)
                    {
                        isLockedOut = false;
                    }
                    else
                    {
                        lockoutRemainingSeconds = _lockoutDurationSeconds - elapsedSinceLockout;
                    }
                }

                return new QuotaMetricPartitionDisplayState
                {
                    QuotaMetricPartitionId = _quotaMetricPartitionId,
                    MetricValue = _localMetricUnitsCount + _remoteMetricUnitsCount,
                    IsLockedOut = isLockedOut,
                    LockoutRemainingSeconds = lockoutRemainingSeconds
                };
            }
        }

        /// <summary>
        /// Adds a local metric unit to the metric partition.
        /// </summary>
        /// <param name="localReferenceTime">The time at which the local metric unit was recorded.</param>
        /// <returns>A <see cref="QuotaMetricPartitionState"/> instance with the current state of the metric partition.</returns>
        public QuotaMetricPartitionState AddLocalMetricUnit(
            DateTimeOffset localReferenceTime)
        {
            int metricCount = 0;
            int localMetricCount = 0;
            int remoteMetricCount = 0;

            lock (_syncRoot)
            {
                if (_lockedOut)
                {
                    if (localReferenceTime - _lockoutStartTime > TimeSpan.FromSeconds(_lockoutDurationSeconds))
                    {
                        // Lockout period has expired.
                        _lockedOut = false;
                        ShiftAndAddLocalUnit(localReferenceTime);
                    }

                    localMetricCount = _localMetricUnitsCount;
                    remoteMetricCount = _remoteMetricUnitsCount;
                    metricCount = localMetricCount + remoteMetricCount;
                }
                else
                {
                    ShiftAndAddLocalUnit(localReferenceTime);

                    localMetricCount = _localMetricUnitsCount;
                    remoteMetricCount = _remoteMetricUnitsCount;
                    metricCount = localMetricCount + remoteMetricCount;

                    if (metricCount > _actualMetricLimit)
                    {
                        _lockedOut = true;
                        _lockoutStartTime = localReferenceTime;
                        Array.Clear(_localMetricUnits, 0, _actualMetricWindowSeconds);
                        Array.Clear(_remoteMetricUnits, 0, _actualMetricWindowSeconds);
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
                TimeUntilRetrySeconds = _lockedOut
                    ? _lockoutDurationSeconds - (int)(DateTimeOffset.UtcNow - _lockoutStartTime).TotalSeconds
                    : 0,
                LocalMetricCount = localMetricCount,
                RemoteMetricCount = remoteMetricCount,
                TotalMetricCount = metricCount
            };
        }

        /// <summary>
        /// Adds remote metric units to the metric partition.
        /// </summary>
        /// <param name="remoteReferenceTimes">The times at which the remote metric units were recorded.</param>
        /// <returns>A <see cref="QuotaMetricPartitionState"/> instance with the current state of the metric partition.</returns>
        public QuotaMetricPartitionState? AddRemoteMetricUnits(
            List<DateTimeOffset> remoteReferenceTimes)
        {
            int metricCount = 0;
            int localMetricCount = 0;
            int remoteMetricCount = 0;
            var localReferenceTime = DateTimeOffset.UtcNow;

            #region Validation

            // Adjust for possible clock skew between the local and remote instances.
            // Only consider remote metric units that were recorded in the past (from the perspective of the local instance)
            // but not earlier than METRIC_TIME_UNIT_SECONDS seconds.
            var validRemoteReferenceTimes = remoteReferenceTimes
                .Where(t =>
                {
                    var timeDifference = localReferenceTime - t;
                    return
                        timeDifference.TotalSeconds >= 0 // Remote time is in the past relative to local time
                        && timeDifference.TotalSeconds < _actualMetricWindowSeconds; // Remote time is not too distant in the past.
                })
                .ToList();
            if (validRemoteReferenceTimes.Count == 0)
            {
                _logger.LogWarning("[QuotaService {ServiceIdentifier}] All remote metric units where discarded because their timestamps were not valid on the local system.",
                    _quotaServiceIdentifier);
                return null;
            }

            var invalidRemoteReferenceTimesCount = remoteReferenceTimes.Count - validRemoteReferenceTimes.Count;
            if (invalidRemoteReferenceTimesCount > 0)
                _logger.LogWarning(
                    "[QuotaService {ServiceIdentifier}] {DiscardedUnitsCount} remote metric units were discarded because their timestamps were not valid on the local system.",
                    _quotaServiceIdentifier,
                    invalidRemoteReferenceTimesCount);

            #endregion

            lock (_syncRoot)
            {
                if (_lockedOut)
                {
                    if (localReferenceTime - _lockoutStartTime > TimeSpan.FromSeconds(_lockoutDurationSeconds))
                    {
                        // Lockout period has expired.
                        _lockedOut = false;
                        ShiftAndAddRemoteUnits(localReferenceTime, validRemoteReferenceTimes);
                    }

                    localMetricCount = _localMetricUnitsCount;
                    remoteMetricCount = _remoteMetricUnitsCount;
                    metricCount = localMetricCount + remoteMetricCount;
                }
                else
                {
                    ShiftAndAddRemoteUnits(localReferenceTime, validRemoteReferenceTimes);

                    localMetricCount = _localMetricUnitsCount;
                    remoteMetricCount = _remoteMetricUnitsCount;
                    metricCount = localMetricCount + remoteMetricCount;

                    if (metricCount > _actualMetricLimit)
                    {
                        _lockedOut = true;
                        _lockoutStartTime = localReferenceTime;
                        Array.Clear(_localMetricUnits, 0, _actualMetricWindowSeconds);
                        Array.Clear(_remoteMetricUnits, 0, _actualMetricWindowSeconds);
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
                // Not relevant for remote units updates.
                TimeUntilRetrySeconds = 0,
                LocalMetricCount = localMetricCount,
                RemoteMetricCount = remoteMetricCount,
                TotalMetricCount = metricCount
            };
        }

        private void ShiftTimeslots(DateTimeOffset refTime)
        {
            // Shift the array to align with the new reference time.

            var fractionalSeconds = (refTime - _metricUnitsLastShiftTime).TotalSeconds;
            if (fractionalSeconds >= 1)
            {
                // More than one second passed since the last unit was added, so we need to shift the one-second buckets.

                if (fractionalSeconds >= _actualMetricWindowSeconds)
                {
                    // No need to shift anything, too much time has passed since the last unit was added.
                    Array.Clear(_localMetricUnits, 0, _actualMetricWindowSeconds);
                    Array.Clear(_remoteMetricUnits, 0, _actualMetricWindowSeconds);
                    _localMetricUnitsCount = 0;
                    _remoteMetricUnitsCount = 0;
                }
                else
                {
                    _localMetricUnitsCount = 0;
                    _remoteMetricUnitsCount = 0;

                    var roundedSeconds = (int)Math.Ceiling(fractionalSeconds);

                    for (int i = _actualMetricWindowSeconds - 1; i >= roundedSeconds; i--)
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
        }

        private void ShiftAndAddLocalUnit(
            DateTimeOffset refTime)
        {
            ShiftTimeslots(refTime);

            // Add the local unit to the metric units.
            _localMetricUnits[0]++;
            _localMetricUnitsCount++;
        }

        private void ShiftAndAddRemoteUnits(
            DateTimeOffset refTime,
            List<DateTimeOffset> remoteReferenceTimes)
        {
            ShiftTimeslots(refTime);

            // Add the remote units to the metric units.
            foreach (var remoteReferenceTime in remoteReferenceTimes)
            {
                var roundedSeconds = (int)Math.Ceiling((refTime - remoteReferenceTime).TotalSeconds);
                if (roundedSeconds >= 0
                    && roundedSeconds < _actualMetricWindowSeconds)
                {
                    _remoteMetricUnits[roundedSeconds]++;
                    _remoteMetricUnitsCount++;
                }
            }
        }

    }
}
