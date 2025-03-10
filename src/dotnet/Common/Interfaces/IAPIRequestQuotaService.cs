using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Quota;
using Microsoft.AspNetCore.Http;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines methods exposed by the API Request Quota service.
    /// </summary>
    public interface IAPIRequestQuotaService
    {
        /// <summary>
        /// Indicates whether the API Request Quota service is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Evaluates an HTTP API request to determine if it exceeds any quotas.
        /// </summary>
        /// <param name="apiName">The name of the API handling the request.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> providing the current HTTP request context.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity associated with the request.</param>
        /// <returns>An <see cref="APIRequestQuotaEvaluationResult"/> object that indicates if any
        /// quotas where exceeded and provides details in case of a quota breach.</returns>
        APIRequestQuotaEvaluationResult EvaluateRawRequestForQuota(
            string apiName,
            HttpContext httpContext,
            UnifiedUserIdentity? userIdentity);

        /// <summary>
        /// Evaluates a completion request to determine if it exceeds any quotas.
        /// </summary>
        /// <param name="apiName">The name of the API handling the request.</param>
        /// <param name="completionRequest">The <see cref="CompletionRequest"/> providing the completion request details.</param>
        /// <returns></returns>
        APIRequestQuotaEvaluationResult EvaluateCompletionRequestForQuota(
            string apiName,
            CompletionRequest completionRequest);
    }
}
