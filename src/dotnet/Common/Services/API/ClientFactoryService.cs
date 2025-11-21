using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoundationaLLM.Common.Services.API
{
    /// <summary>
    /// Provides methods for creating client pipelines for communication with API endpoints.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> for the main DI container.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
    /// <remarks>This service abstracts the creation of client pipelines, allowing clients to establish
    /// connections using either an instance identifier and client name or a specific API endpoint configuration.
    /// Implementations may enforce authentication or configuration requirements depending on the provided
    /// parameters.</remarks>
    public class ClientFactoryService(
        IServiceProvider serviceProvider,
        IConfiguration configuration) : IClientFactoryService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IConfiguration _configuration = configuration;

        private IResourceProviderService? _configurationResourceProvider;

        /// <inheritdoc/>
        public async Task<T> CreateClient<T>(
            string instanceId,
            string clientName,
            UnifiedUserIdentity userIdentity,
            Func<Dictionary<string, object>, T> clientBuilder,
            Dictionary<string, object>? clientBuilderParameters = null)
        {
            var endpointConfiguration = await GetEndpoint(instanceId, clientName, userIdentity);

            clientBuilderParameters ??= [];

            clientBuilderParameters[HttpClientFactoryServiceKeyNames.Endpoint] =
                GetUrlExceptionForUserIdentity(endpointConfiguration, userIdentity)?.Url
                ?? endpointConfiguration.Url;
            clientBuilderParameters[HttpClientFactoryServiceKeyNames.TimeoutSeconds] = endpointConfiguration.TimeoutSeconds;
            clientBuilderParameters[HttpClientFactoryServiceKeyNames.AuthenticationType] = endpointConfiguration.AuthenticationType;
            if (endpointConfiguration.AuthenticationType == AuthenticationTypes.APIKey)
            {
                if (endpointConfiguration.AuthenticationParameters.TryGetValue(
                    AuthenticationParametersKeys.APIKeyHeaderName, out var apiKeyHeaderNameObj))
                    clientBuilderParameters[HttpClientFactoryServiceKeyNames.APIKeyHeaderName] =
                        apiKeyHeaderNameObj!.ToString()!;

                if (!endpointConfiguration.AuthenticationParameters.TryGetValue(
                        AuthenticationParametersKeys.APIKeyConfigurationName, out var apiKeyConfigurationNameObj))
                    throw new Exception($"The {AuthenticationParametersKeys.APIKeyConfigurationName} key is missing from the endpoint's authentication parameters dictionary.");

                var apiKey = _configuration.GetValue<string>(apiKeyConfigurationNameObj.ToString()!);
                clientBuilderParameters[HttpClientFactoryServiceKeyNames.APIKey] = apiKey!;

                if (endpointConfiguration.AuthenticationParameters.TryGetValue(
                    AuthenticationParametersKeys.APIKeyPrefix, out var apiKeyPrefixObj))
                    clientBuilderParameters[HttpClientFactoryServiceKeyNames.APIKeyHeaderName] =
                        apiKeyPrefixObj.ToString()!;
            }
            if (!string.IsNullOrWhiteSpace(endpointConfiguration.APIVersion))
                clientBuilderParameters[HttpClientFactoryServiceKeyNames.APIVersion] = endpointConfiguration.APIVersion;

            var client = clientBuilder(clientBuilderParameters);
            return client;
        }

        private async Task EnsureConfigurationResourceProvider()
        {
            if (_configurationResourceProvider != null)
                return;

            var resourceProviderServices = _serviceProvider.GetServices<IResourceProviderService>();
            _configurationResourceProvider = resourceProviderServices
                .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Configuration)
                ?? throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} was not loaded.");


            await _configurationResourceProvider.WaitForInitialization();
        }

        private async Task<APIEndpointConfiguration> GetEndpoint(
            string instanceId,
            string name,
            UnifiedUserIdentity userIdentity)
        {
            await EnsureConfigurationResourceProvider();

            var endpointConfiguration = await _configurationResourceProvider!.GetResourceAsync<APIEndpointConfiguration>(
                $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Configuration}/{ConfigurationResourceTypeNames.APIEndpointConfigurations}/{name}",
                userIdentity)
                ?? throw new Exception($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} did not load the {name} endpoint configuration.");

            return endpointConfiguration;
        }

        /// <summary>
        /// Retrieve an enabled URL Exception for the provided user identity if one exists. If not NULL is returned.
        /// </summary>
        /// <param name="endpointConfiguration">The endpoint configuration from which to retrieve the URL Exception.</param>
        /// <param name="userIdentity">The Identity of the user.</param>
        /// <returns>URLException or NULL</returns>
        private UrlException? GetUrlExceptionForUserIdentity(APIEndpointConfiguration endpointConfiguration, UnifiedUserIdentity userIdentity)
            => endpointConfiguration.UrlExceptions.SingleOrDefault(x => x.UserPrincipalName.ToLower() == userIdentity.UPN!.ToLower() && x.Enabled);
    }
}
