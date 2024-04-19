using FoundationaLLM.Gateway.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM
{
    /// <summary>
    /// General purpose dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Adds the Gateway general exception handler to the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddGatewayGenericExceptionHandling(this IHostApplicationBuilder builder)
        {
            builder.Services.AddExceptionHandler<GatewayExceptionHandler>();
            builder.Services.AddProblemDetails();
        }
    }
}
