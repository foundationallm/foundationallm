using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Tasks
{
    /// <summary>
    /// Represents a pool of active tasks with a predefined capacity.
    /// </summary>
    /// <remarks>
    /// This class is thread safe.
    /// </remarks>
    public class ConcurrentTaskPool
    {
        private readonly TaskPool _taskPool;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// Constructs a task pool with a specified capacity.
        /// </summary>
        /// <param name="maxConcurrentTasks">Indicates the maximum number of tasks accepted by the task pool.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public ConcurrentTaskPool(int maxConcurrentTasks,
            ILogger<ConcurrentTaskPool> logger) =>
            _taskPool = new TaskPool(maxConcurrentTasks, logger);

        /// <summary>
        /// Indicates whether the task pool is at capacity or not.
        /// </summary>
        public int AvailableCapacity {
            get
            {
                _semaphore.Wait();

                try
                {
                    return _taskPool.AvailableCapacity;
                }
                finally
                {
                    _semaphore.Release();
                }
            }}

        /// <summary>
        /// Adds a new batch of tasks to the task pool.
        /// </summary>
        /// <param name="tasks">The list of <see cref="TaskInfo"/> items to be added to the pool.</param>
        /// <exception cref="TaskPoolException">The exception raised when a task cannot be added to the pool (e.g., the task pool is at capacity).</exception>
        public bool TryAdd(IEnumerable<TaskInfo> tasks)
        {
            _semaphore.Wait();

            try
            {
                if (_taskPool.AvailableCapacity < tasks.Count())
                    return false;

                _taskPool.Add(tasks);
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Determines whether the task pool already has a running task for a specified payload id.
        /// </summary>
        /// <param name="payloadId">The identifier of the payload.</param>
        /// <returns>True if the task pool already has a running task for the specified payload, false otherwise.</returns>
        public bool HasRunningTaskForPayload(string payloadId)
        {
            _semaphore.Wait();

            try
            {
                return _taskPool.HasRunningTaskForPayload(payloadId);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
