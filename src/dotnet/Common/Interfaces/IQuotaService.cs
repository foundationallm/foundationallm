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
    }
}
