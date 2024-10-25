using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;

namespace FoundationaLLM.Client.Management.Clients.Resources
{
    internal class ConfigurationManagementClient(IManagementRESTClient managementRestClient) : IConfigurationManagementClient
    {
        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<AppConfigurationKeyBase>>> GetAppConfigurationsAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<AppConfigurationKeyBase>>>(
                ResourceProviderNames.FoundationaLLM_Configuration,
                ConfigurationResourceTypeNames.AppConfigurations
            );

        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<AppConfigurationKeyBase>>> GetAppConfigurationsByFilterAsync(string key) =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<AppConfigurationKeyBase>>>(
                ResourceProviderNames.FoundationaLLM_Configuration,
                $"{ConfigurationResourceTypeNames.AppConfigurations}/{key}"
            );

        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<APIEndpointConfiguration>>> GetExternalOrchestrationServicesAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<APIEndpointConfiguration>>>(
                ResourceProviderNames.FoundationaLLM_Configuration,
                ConfigurationResourceTypeNames.APIEndpointConfigurations
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<APIEndpointConfiguration>> GetExternalOrchestrationServiceAsync(string externalOrchestrationServiceName)
        {
            var result = await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<APIEndpointConfiguration>>>(
                ResourceProviderNames.FoundationaLLM_Configuration,
                $"{ConfigurationResourceTypeNames.APIEndpointConfigurations}/{externalOrchestrationServiceName}"
            );

            if (result == null || result.Count == 0)
            {
                throw new Exception($"ExternalOrchestrationService '{externalOrchestrationServiceName}' not found.");
            }

            var resource = result[0];

            return resource;
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertAppConfigurationAsync(AppConfigurationKeyBase appConfiguration) =>
            await managementRestClient.Resources.UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Configuration,
                $"{ConfigurationResourceTypeNames.AppConfigurations}/{appConfiguration.Name}",
                appConfiguration
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertAPIEndpointConfiguration(APIEndpointConfiguration apiEndpointConfiguration) =>
            await managementRestClient.Resources.UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Configuration,
                $"{ConfigurationResourceTypeNames.APIEndpointConfigurations}/{apiEndpointConfiguration.Name}",
                apiEndpointConfiguration
            );
            
        /// <inheritdoc/>
        public async Task DeleteAppConfigurationAsync(string key) =>
            await managementRestClient.Resources.DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Configuration,
                $"{ConfigurationResourceTypeNames.AppConfigurations}/{key}"
            );
    }
}
