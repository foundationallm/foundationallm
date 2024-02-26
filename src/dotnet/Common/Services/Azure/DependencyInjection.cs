using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM.Common.Services.Azure
{
    /// <summary>
    /// Provides extension methods used to configure dependency injection.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Register the dependencies required to support Azure Event Grid events.
        /// </summary>
        /// <param name="services">Application builder service collection.</param>
        public static void AddAzureResourceManager(
            this IServiceCollection services) =>
            services.AddSingleton<IAzureResourceManagerService, AzureResourceManagerService>();
    }
}
