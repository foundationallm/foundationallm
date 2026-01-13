namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Defines the interface for the Data Pipeline Scheduler service.
    /// </summary>
    /// <remarks>
    /// The scheduler service evaluates scheduled data pipeline triggers and initiates pipeline runs
    /// when the schedule conditions are met.
    /// </remarks>
    public interface IDataPipelineSchedulerService
    {
        // This interface is intentionally empty.
        // It is used to identify the scheduler service in the dependency injection container.
        // The actual scheduling logic is implemented in the DataPipelineSchedulerService class
        // which inherits from DataPipelineBackgroundService and executes on a periodic cycle.
    }
}
