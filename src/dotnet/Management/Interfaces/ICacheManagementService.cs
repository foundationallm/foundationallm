namespace FoundationaLLM.Management.Interfaces
{
    /// <summary>
    /// Provides cache management functionality.
    /// </summary>
    public interface ICacheManagementService
    {
        /// <summary>
        /// Clears the agent cache from the OrchestrationService and AgentHubService.
        /// </summary>
        /// <returns></returns>
        Task<bool> ClearAgentCache();

        /// <summary>
        /// Clears the agent cache from the OrchestrationService and DataSourceHubService.
        /// </summary>
        /// <returns></returns>
        Task<bool> ClearDataSourceCache();

        /// <summary>
        /// Clears the agent cache from the OrchestrationService and DataSourceHubService.
        /// </summary>
        /// <returns></returns>
        Task<bool> ClearPromptCache();
    }
}
