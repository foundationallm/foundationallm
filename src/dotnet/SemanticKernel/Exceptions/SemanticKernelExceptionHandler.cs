using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.SemanticKernel.Core.Exceptions
{
    /// <summary>
    /// Implements a global exception handler for the Semantic Kernel API.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class SemanticKernelExceptionHandler(
        ILogger<SemanticKernelExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<SemanticKernelExceptionHandler> _logger = logger;

        /// <inheritdoc/>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "ERROR: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Title = "FoundationaLLM Gateway error"
            };

            if (exception is SemanticKernelException skException)
            {
                problemDetails.Status = skException.StatusCode;
                problemDetails.Detail = skException.Message;
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
