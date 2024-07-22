using FoundationaLLM.Client.Management.Clients.Resources;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using NSubstitute;
using System.Collections.Generic;

namespace Management.Client.Tests.Clients.Resources
{
    public class ConfigurationManagementClientTests
    {
        private readonly IManagementRESTClient _mockRestClient;
        private readonly ConfigurationManagementClient _configurationClient;

        public ConfigurationManagementClientTests()
        {
            _mockRestClient = Substitute.For<IManagementRESTClient>();
            _configurationClient = new ConfigurationManagementClient(_mockRestClient);
        }

        [Fact]
        public async Task GetAppConfigurationsAsync_ShouldReturnConfigurations()
        {
            // Arrange
            var expectedConfigurations = new List<ResourceProviderGetResult<AppConfigurationKeyBase>>
            {
                new ResourceProviderGetResult<AppConfigurationKeyBase>
                {
                    Resource = new AppConfigurationKeyBase
                    {
                        Name = "test-configuration",
                        Key = "FoundationaLLM:TestConfiguration",
                        Value = "TestValue",
                        ContentType = "text/plain",
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AppConfigurationKeyBase>>>(
                    ResourceProviderNames.FoundationaLLM_Configuration,
                    ConfigurationResourceTypeNames.AppConfigurations
                )
                .Returns(Task.FromResult(expectedConfigurations));

            // Act
            var result = await _configurationClient.GetAppConfigurationsAsync();

            // Assert
            Assert.Equal(expectedConfigurations, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<AppConfigurationKeyBase>>>(
                ResourceProviderNames.FoundationaLLM_Configuration,
                ConfigurationResourceTypeNames.AppConfigurations
            );
        }

        [Fact]
        public async Task GetAppConfigurationsByFilterAsync_ShouldReturnFilteredConfigurations()
        {
            // Arrange
            var key = "FoundationaLLM:TestConfiguration";
            var expectedConfigurations = new List<ResourceProviderGetResult<AppConfigurationKeyBase>>
            {
                new ResourceProviderGetResult<AppConfigurationKeyBase>
                {
                    Resource = new AppConfigurationKeyBase
                    {
                        Name = key,
                        Key = key,
                        Value = "TestValue",
                        ContentType = "text/plain",
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AppConfigurationKeyBase>>>(
                    ResourceProviderNames.FoundationaLLM_Configuration,
                    $"{ConfigurationResourceTypeNames.AppConfigurations}/{key}"
                )
                .Returns(Task.FromResult(expectedConfigurations));

            // Act
            var result = await _configurationClient.GetAppConfigurationsByFilterAsync(key);

            // Assert
            Assert.Equal(expectedConfigurations, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<AppConfigurationKeyBase>>>(
                ResourceProviderNames.FoundationaLLM_Configuration,
                $"{ConfigurationResourceTypeNames.AppConfigurations}/{key}"
            );
        }

        [Fact]
        public async Task GetAPIEndpointsAsync_ShouldReturnServices()
        {
            // Arrange
            var expectedServices = new List<ResourceProviderGetResult<APIEndpoint>>
            {
                new ResourceProviderGetResult<APIEndpoint>
                {
                    Resource = new APIEndpoint
                    {
                        Name = "test-service",
                        Url = "FoundationaLLM:TestAPIUrlConfiguration",
                        APIKeyConfigurationName = "FoundationaLLM:TestAPIKeyConfiguration",
                        Category = APIEndpointCategory.General,
                        AuthenticationType = "APIKey",
                        APIKeyHeaderName = "FoundationaLLM:TestAPIKeyHeaderName",
                        TimeoutSeconds = 60,
                        RetryStrategyName = "ExponentialBackoff"
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<APIEndpoint>>>(
                    ResourceProviderNames.FoundationaLLM_Configuration,
                    ConfigurationResourceTypeNames.APIEndpoints
                )
                .Returns(Task.FromResult(expectedServices));

            // Act
            var result = await _configurationClient.GetExternalOrchestrationServicesAsync();

            // Assert
            Assert.Equal(expectedServices, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<APIEndpoint>>>(
                ResourceProviderNames.FoundationaLLM_Configuration,
                ConfigurationResourceTypeNames.APIEndpoints
            );
        }

        [Fact]
        public async Task GetAPIEndpointAsync_ShouldReturnService()
        {
            // Arrange
            var serviceName = "test-service";
            var expectedService = new ResourceProviderGetResult<APIEndpoint>
            {
                Resource = new APIEndpoint
                {
                    Name = serviceName,
                    Url = "FoundationaLLM:TestAPIUrlConfiguration",
                    APIKeyConfigurationName = "FoundationaLLM:TestAPIKeyConfiguration",
                    Category = APIEndpointCategory.General,
                    AuthenticationType = "APIKey",
                    APIKeyHeaderName = "FoundationaLLM:TestAPIKeyHeaderName",
                    TimeoutSeconds = 60,
                    RetryStrategyName = "ExponentialBackoff"
                },
                Actions = [],
                Roles = []
            };
            var expectedServices = new List<ResourceProviderGetResult<APIEndpoint>> { expectedService };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<APIEndpoint>>>(
                    ResourceProviderNames.FoundationaLLM_Configuration,
                    $"{ConfigurationResourceTypeNames.APIEndpoints}/{serviceName}"
                )
                .Returns(Task.FromResult(expectedServices));

            // Act
            var result = await _configurationClient.GetExternalOrchestrationServiceAsync(serviceName);

            // Assert
            Assert.Equal(expectedService, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<APIEndpoint>>>(
                ResourceProviderNames.FoundationaLLM_Configuration,
                $"{ConfigurationResourceTypeNames.APIEndpoints}/{serviceName}"
            );
        }

        [Fact]
        public async Task GetAPIEndpointAsync_ShouldThrowException_WhenServiceNotFound()
        {
            // Arrange
            var serviceName = "test-service";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<APIEndpoint>>>(
                    ResourceProviderNames.FoundationaLLM_Configuration,
                    $"{ConfigurationResourceTypeNames.APIEndpoints}/{serviceName}"
                )
                .Returns(Task.FromResult<List<ResourceProviderGetResult<APIEndpoint>>>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _configurationClient.GetExternalOrchestrationServiceAsync(serviceName));
            Assert.Equal($"APIEndpoint '{serviceName}' not found.", exception.Message);
        }

        [Fact]
        public async Task UpsertAppConfigurationAsync_ShouldReturnUpsertResult()
        {
            // Arrange
            var appConfiguration = new AppConfigurationKeyBase { Name = "test-configuration" };
            var expectedUpsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = "test-object-id"
            };

            _mockRestClient.Resources
                .UpsertResourceAsync(
                    ResourceProviderNames.FoundationaLLM_Configuration,
                    $"{ConfigurationResourceTypeNames.AppConfigurations}/{appConfiguration.Name}",
                    appConfiguration
                )
                .Returns(Task.FromResult(expectedUpsertResult));

            // Act
            var result = await _configurationClient.UpsertAppConfigurationAsync(appConfiguration);

            // Assert
            Assert.Equal(expectedUpsertResult, result);
            await _mockRestClient.Resources.Received(1).UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Configuration,
                $"{ConfigurationResourceTypeNames.AppConfigurations}/{appConfiguration.Name}",
                appConfiguration
            );
        }
    }
}
