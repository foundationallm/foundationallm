using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Common.Middleware
{
    /// <summary>
    /// Middleware that enforces API request quotas.
    /// </summary>
    public class QuotaMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallContextMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        public QuotaMiddleware(RequestDelegate next) =>
            _next = next;

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The current HTTP request context.</param>
        /// <param name="callContext">Stores context information extracted from the current HTTP request. This information
        /// is primarily used to inject HTTP headers into downstream HTTP calls.</param>
        /// <param name="quotaService">Provides services for managing quotas.</param>
        /// <param name="instanceSettings">Contains the FoundationaLLM instance configuration settings.</param>
        /// <returns></returns>
        public async Task InvokeAsync(
            HttpContext context,
            ICallContext callContext,
            IQuotaService quotaService,
            IOptions<InstanceSettings> instanceSettings)
        {
            if (quotaService.Enabled)
            {
                // Evaluate quotas for API requests
                var quotaEvaluationResult = quotaService.EvaluateRawRequestForQuota(
                    ServiceNames.CoreAPI,
                    context.Request.RouteValues["controller"]?.ToString(),
                    callContext.CurrentUserIdentity);
                if (quotaEvaluationResult.QuotaExceeded)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(quotaEvaluationResult));
                    return; // Short-circuit the request pipeline.
                }
            }

            // Call the next delegate/middleware in the pipeline:
            await _next(context);
        }
    }
}
