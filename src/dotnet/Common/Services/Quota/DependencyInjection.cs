using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Services.Quota;
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
        /// Adds CORS policies the the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddAPIRequestQuotaService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IAPIRequestQuotaService, APIRequestQuotaService>();
            builder.Services.ActivateSingleton<IAPIRequestQuotaService>();
        }
    }
}
