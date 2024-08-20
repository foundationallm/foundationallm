using FoundationaLLM.Client.Management.Clients.Resources;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using NSubstitute;

namespace Management.Client.Tests.Clients.Resources
{
    public class AIModelManagementClientTests
    {
        private readonly IManagementRESTClient _mockRestClient;
        private readonly AIModelManagementClient _aiModelClient;

        public AIModelManagementClientTests()
        {
            _mockRestClient = Substitute.For<IManagementRESTClient>();
            _aiModelClient = new AIModelManagementClient(_mockRestClient);
        }

        [Fact]
        public async Task GetAIModelsAsync_ShouldReturnAIModels()
        {
            // Arrange
            var expectedAIModels = new List<ResourceProviderGetResult<AIModelBase>>
            {
                new() {
                    Resource = new AIModelBase
                    {
                        Name = "test-ai-model",
                        Description = "A test AI model",
                        Type = AIModelTypes.Completion,
                        EndpointObjectId = "endpoint-object-id"
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AIModelBase>>>(
                    ResourceProviderNames.FoundationaLLM_AIModel,
                    AIModelResourceTypeNames.AIModels
                )
                .Returns(Task.FromResult(expectedAIModels));

            // Act
            var result = await _aiModelClient.GetAIModelsAsync();

            // Assert
            Assert.Equal(expectedAIModels, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<AIModelBase>>>(
                ResourceProviderNames.FoundationaLLM_AIModel,
                AIModelResourceTypeNames.AIModels
            );
        }

        [Fact]
        public async Task GetAIModelAsync_ShouldReturnAIModel()
        {
            // Arrange
            var aiModelName = "test-ai-model";
            var expectedAIModel = new ResourceProviderGetResult<AIModelBase>
            {
                Resource = new AIModelBase
                {
                    Name = aiModelName,
                    Description = "A test AI model",
                    Type = AIModelTypes.Completion,
                    EndpointObjectId = "endpoint-object-id"
                },
                Actions = [],
                Roles = []
            };
            var expectedAIModels = new List<ResourceProviderGetResult<AIModelBase>> { expectedAIModel };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AIModelBase>>>(
                    ResourceProviderNames.FoundationaLLM_AIModel,
                    $"{AIModelResourceTypeNames.AIModels}/{aiModelName}"
                )
                .Returns(Task.FromResult(expectedAIModels));

            // Act
            var result = await _aiModelClient.GetAIModelAsync(aiModelName);

            // Assert
            Assert.Equal(expectedAIModel, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<AIModelBase>>>(
                ResourceProviderNames.FoundationaLLM_AIModel,
                $"{AIModelResourceTypeNames.AIModels}/{aiModelName}"
            );
        }

        [Fact]
        public async Task GetAIModelAsync_ShouldThrowException_WhenAIModelNotFound()
        {
            // Arrange
            var aiModelName = "test-ai-model";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AIModelBase>>>(
                    ResourceProviderNames.FoundationaLLM_AIModel,
                    $"{AIModelResourceTypeNames.AIModels}/{aiModelName}"
                )
                .Returns(Task.FromResult<List<ResourceProviderGetResult<AIModelBase>>>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _aiModelClient.GetAIModelAsync(aiModelName));
            Assert.Equal($"AI Model '{aiModelName}' not found.", exception.Message);
        }

        [Fact]
        public async Task UpsertAIModel_ShouldReturnUpsertResult()
        {
            // Arrange
            var aiModel = new AIModelBase
            {
                Name = "test-configuration",
                EndpointObjectId = ""
            };
            var expectedUpsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = "test-object-id"
            };

            _mockRestClient.Resources
                .UpsertResourceAsync(
                    ResourceProviderNames.FoundationaLLM_AIModel,
                    $"{AIModelResourceTypeNames.AIModels}/{aiModel.Name}",
                    aiModel
                )
                .Returns(Task.FromResult(expectedUpsertResult));

            // Act
            var result = await _aiModelClient.UpsertAIModel(aiModel);

            // Assert
            Assert.Equal(expectedUpsertResult, result);
            await _mockRestClient.Resources.Received(1).UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_AIModel,
                $"{AIModelResourceTypeNames.AIModels}/{aiModel.Name}",
                aiModel
            );
        }
    }
}
