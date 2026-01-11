using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Quota;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines methods exposed by the FoundationaLLM Quota service.
    /// </summary>
    public interface IQuotaService
    {
        /// <summary>
        /// Indicates whether the quota service is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Gets the <see cref="TaskCompletionSource{T}"/> (<c>TResult</c> of type <see cref="bool"/>)
        /// that signals the completion of the initialization task.
        /// </summary>
        /// <remarks>
        /// The result of the task indicates whether initialization completed successfully or not.
        /// </remarks>
        Task<bool> InitializationTask { get; }

        /// <summary>
        /// Evaluates an HTTP API request to determine if it exceeds any quotas.
        /// </summary>
        /// <param name="apiName">The name of the API handling the request.</param>
        /// <param name="controllerName">The name of the ASP.NET controller handling the request.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity associated with the request.</param>
        /// <returns>An <see cref="QuotaMetricPartitionState"/> object that indicates if any
        /// quotas where exceeded and provides details in case of a quota breach.</returns>
        QuotaMetricPartitionState EvaluateRawRequestForQuota(
            string apiName,
            string? controllerName,
            UnifiedUserIdentity? userIdentity);

        /// <summary>
        /// Evaluates a completion request to determine if it exceeds any quotas.
        /// </summary>
        /// <param name="apiName">The name of the API handling the request.</param>
        /// <param name="controllerName">The name of the ASP.NET controller handling the request.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity associated with the request.</param>
        /// <param name="completionRequest">The <see cref="CompletionRequest"/> providing the completion request details.</param>
        /// <returns></returns>
        QuotaMetricPartitionState EvaluateCompletionRequestForQuota(
            string apiName,
            string controllerName,
            UnifiedUserIdentity? userIdentity,
            CompletionRequest completionRequest);

        /// <summary>
        /// Gets all quota definitions.
        /// </summary>
        /// <returns>A list of all quota definitions.</returns>
        Task<List<QuotaDefinition>> GetQuotaDefinitionsAsync();

        /// <summary>
        /// Gets a quota definition by name.
        /// </summary>
        /// <param name="name">The name of the quota definition to retrieve.</param>
        /// <returns>The quota definition if found, null otherwise.</returns>
        Task<QuotaDefinition?> GetQuotaDefinitionAsync(string name);

        /// <summary>
        /// Creates or updates a quota definition.
        /// </summary>
        /// <param name="quotaDefinition">The quota definition to create or update.</param>
        /// <returns>The created or updated quota definition.</returns>
        Task<QuotaDefinition> UpsertQuotaDefinitionAsync(QuotaDefinition quotaDefinition);

        /// <summary>
        /// Deletes a quota definition.
        /// </summary>
        /// <param name="name">The name of the quota definition to delete.</param>
        Task DeleteQuotaDefinitionAsync(string name);

        /// <summary>
        /// Gets current usage metrics for all quotas.
        /// </summary>
        /// <returns>A list of current quota usage metrics.</returns>
        Task<List<QuotaUsageMetrics>> GetQuotaUsageMetricsAsync();

        /// <summary>
        /// Gets usage metrics for quotas matching the filter criteria.
        /// </summary>
        /// <param name="filter">The filter criteria.</param>
        /// <returns>A list of quota usage metrics matching the filter.</returns>
        Task<List<QuotaUsageMetrics>> GetQuotaUsageMetricsAsync(QuotaMetricsFilter filter);

        /// <summary>
        /// Gets usage history for a specific quota.
        /// </summary>
        /// <param name="quotaName">The name of the quota.</param>
        /// <param name="startTime">The start time for the history query.</param>
        /// <param name="endTime">The end time for the history query.</param>
        /// <returns>A list of historical usage data.</returns>
        Task<List<QuotaUsageHistory>> GetQuotaUsageHistoryAsync(
            string quotaName,
            DateTimeOffset startTime,
            DateTimeOffset endTime);
    }
}
