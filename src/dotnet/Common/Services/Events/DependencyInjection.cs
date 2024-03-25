using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Events;
using FoundationaLLM.Common.Services.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoundationaLLM
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
        /// <param name="configuration">The <see cref="IConfigurationManager"/> providing access to configuration.</param>
        /// <param name="eventGridProfileSection">The name of the configuration section that contains the Event Grid profile to load.</param>
        public static void AddAzureEventGridEvents(this IServiceCollection services,
            IConfigurationManager configuration,
            string eventGridProfileSection)
        {
            services.AddOptions<AzureEventGridEventServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Events_AzureEventGridEventService));

            services.AddOptions<AzureEventGridEventServiceProfile>()
                .Bind(configuration.GetSection(eventGridProfileSection));

            services.AddSingleton<IEventService, AzureEventGridEventService>();

            services.AddHostedService<EventsWorker>();
        }
    }
}
