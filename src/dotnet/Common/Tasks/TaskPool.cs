using Microsoft.Extensions.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tasks
{
    /// <summary>
    /// Represents a pool of active tasks with a predefined capacity.
    /// </summary>
    public class TaskPool
    {
        private readonly int _maxConcurrentTasks;
        private readonly List<TaskStatus> _runningStates = new List<TaskStatus>
        {
            TaskStatus.Running,
            TaskStatus.WaitingForActivation,
            TaskStatus.WaitingToRun
        };

        private Task[] _tasks;

        /// <summary>
        /// Indicates whether the task pool is at capacity or not.
        /// </summary>
        public int AvailableCapacity => _maxConcurrentTasks - _tasks.Count(t => (t != null) && _runningStates.Contains(t.Status));

        /// <summary>
        /// Constructs a task pool with a specified capacity.
        /// </summary>
        /// <param name="maxConcurrentTasks">Indicates the maximum number of tasks accepted by the task pool.</param>
        public TaskPool(int maxConcurrentTasks)
        {
            _maxConcurrentTasks = maxConcurrentTasks;
            _tasks = new Task[maxConcurrentTasks];
        }

        /// <summary>
        /// Adds a new batch of tasks to the task pool.
        /// </summary>
        /// <param name="tasks">The list of tasks to be added to the pool.</param>
        /// <exception cref="TaskPoolException">The exception raised when a task cannot be added to the pool (e.g., the task pool is at capacity).</exception>
        public void Add(IEnumerable<Task> tasks)
        {
            foreach (var t in tasks)
            {
                var indexOfFirstEmptySlot = _tasks.TakeWhile(t => (t != null) && _runningStates.Contains(t.Status)).Count();

                if (indexOfFirstEmptySlot == _tasks.Length)
                    throw new TaskPoolException("The task pool is at capacity and cannot accept additional tasks");

                _tasks[indexOfFirstEmptySlot] = t;
            }
        }
    }
}
