using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.DataSourceConfigurations
{
    public class BlobStorageConfigurationTests
    {
        private readonly string _connectionSecret = "testConnectionSecretName";
        private readonly string _containerName = "testContainerName";
        private readonly List<string> _files = new List<string> { "file1", "file2" };
        private readonly BlobStorageConfiguration _configuration;

        public BlobStorageConfigurationTests()
        {
            _configuration = new BlobStorageConfiguration
            {
                ConnectionStringSecretName = _connectionSecret,
                ContainerName = _containerName,
                Files = _files
            };
        }
        [Fact]
        public void BlobStorageConfiguration_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_connectionSecret, _configuration.ConnectionStringSecretName);
            Assert.Equal(_containerName, _configuration.ContainerName);
            Assert.Equal(_files, _configuration.Files);
        }

        [Fact]
        public void BlobStorageConfiguration_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_configuration);
            var deserializedConfig = JsonConvert.DeserializeObject<BlobStorageConfiguration>(serializedJson);

            // Assert
            Assert.Equal(_configuration.ConnectionStringSecretName, deserializedConfig?.ConnectionStringSecretName);
            Assert.Equal(_configuration.ContainerName, deserializedConfig?.ContainerName);
            Assert.Equal(_configuration.Files, deserializedConfig?.Files);
        }
    }
}
