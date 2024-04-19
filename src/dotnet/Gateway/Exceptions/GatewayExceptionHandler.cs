using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Gateway.Exceptions
{
    /// <summary>
    /// Implements a global exception handler for the Gateway API.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class GatewayExceptionHandler(
        ILogger<GatewayExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GatewayExceptionHandler> _logger = logger;

        /// <inheritdoc/>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "ERROR: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Title = "FoundationaLLM Gateway error"
            };

            if (exception is GatewayException gatewayException)
            {
                problemDetails.Status = gatewayException.StatusCode;
                problemDetails.Detail = gatewayException.Message;
            }
            else
            {
                problemDetails.Status = StatusCodes.Status500InternalServerError;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
