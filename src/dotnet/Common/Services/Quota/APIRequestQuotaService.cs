using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Quota;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Provides services for evaluating API request quotas.
    /// </summary>
    public class APIRequestQuotaService : IAPIRequestQuotaService
    {
        /// <inheritdoc/>
        public bool Enabled => true;

        /// <inheritdoc/>
        public APIRequestQuotaEvaluationResult EvaluateRawRequestForQuota(
            string apiName,
            HttpContext httpContext,
            UnifiedUserIdentity? userIdentity)
        {
            var userIdentifier = userIdentity?.UserId ?? "__default__";
            var userPrincipalName = userIdentity?.UPN ?? "__default__";
            var controller = httpContext.Request.RouteValues["controller"]?.ToString() ?? "__default__";

            return new()
            {
                RateLimitExceeded = false
            };
        }

        /// <inheritdoc/>
        public APIRequestQuotaEvaluationResult EvaluateCompletionRequestForQuota(
            string apiName,
            CompletionRequest completionRequest)
        {
            var agentName = completionRequest.AgentName ?? "__default__";

            return new()
            {
                RateLimitExceeded = false
            };
        }
    }
}
