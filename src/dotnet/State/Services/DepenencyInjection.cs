using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.State.Interfaces;
using FoundationaLLM.State.Models.Configuration;
using FoundationaLLM.State.Services;
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
        /// Adds the core Gateway service the the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddStateService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<StateServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_StateAPI_Configuration_CosmosDB));

            builder.Services.AddScoped<IStateService, StateService>();
        }
    }
}
