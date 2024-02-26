using Azure.Messaging;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Configuration.AppConfiguration;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoundationaLLM.Configuration.Services
{
    /// <summary>
    /// Implements the FoundationaLLM.Configuration resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="appConfigurationService">The <see cref="IAzureAppConfigurationService"/> provding access to the app configuration service.</param>
    /// <param name="keyVaultService">The <see cref="IAzureKeyVaultService"/> providing access to the key vault service.</param>
    /// <param name="configurationManager">The <see cref="IConfigurationManager"/> providing configuration services.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class ConfigurationResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Configuration)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IAzureAppConfigurationService appConfigurationService,
        IAzureKeyVaultService keyVaultService,
        IConfigurationManager configurationManager,
        ILogger<ConfigurationResourceProviderService> logger)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            storageService,
            eventService,
            resourceValidatorFactory,
            logger,
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Configuration
            ])
    {
        private readonly IAzureAppConfigurationService _appConfigurationService = appConfigurationService;
        private readonly IAzureKeyVaultService _keyVaultService = keyVaultService;
        private readonly IConfigurationManager _configurationManager = configurationManager;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Configuration;

        /// <inheritdoc/>
        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventSetEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.Namespace);

            switch (e.Namespace)
            {
                case EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Configuration:
                    foreach (var @event in e.Events)
                        await HandleConfigurationResourceProviderEvent(@event);
                    break;
                default:
                    // Ignore sliently any event namespace that's of no interest.
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task HandleConfigurationResourceProviderEvent(CloudEvent e)
        {
            if (string.IsNullOrWhiteSpace(e.Subject))
                return;

            try
            {
                var eventData = JsonSerializer.Deserialize<AppConfigurationEventData>(e.Data);
                if (eventData == null)
                    throw new ResourceProviderException("Invalid app configuration event data.");

                _logger.LogInformation("The value [{AppConfigurationKey}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
                    eventData.Key, _name);

                var keyValue = await _appConfigurationService.GetConfigurationSettingAsync(eventData.Key);

                try
                {
                    var keyVaultSecret = JsonSerializer.Deserialize<AppConfigurationKeyVaultUri>(keyValue!);
                    if (keyVaultSecret != null
                        & !string.IsNullOrWhiteSpace(keyVaultSecret!.Uri))
                        keyValue = await _keyVaultService.GetSecretValueAsync(
                            keyVaultSecret.Uri!.Split('/').Last());
                }
                catch { }

                _configurationManager[eventData.Key] = keyValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the app configuration event.");
            }
        }

        #endregion
    }
}
