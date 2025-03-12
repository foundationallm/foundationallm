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
        /// Evaluates an HTTP API request to determine if it exceeds any quotas.
        /// </summary>
        /// <param name="apiName">The name of the API handling the request.</param>
        /// <param name="controllerName">The name of the ASP.NET controller handling the request.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity associated with the request.</param>
        /// <returns>An <see cref="QuotaEvaluationResult"/> object that indicates if any
        /// quotas where exceeded and provides details in case of a quota breach.</returns>
        QuotaEvaluationResult EvaluateRawRequestForQuota(
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
        QuotaEvaluationResult EvaluateCompletionRequestForQuota(
            string apiName,
            string controllerName,
            UnifiedUserIdentity? userIdentity,
            CompletionRequest completionRequest);
    }
}
