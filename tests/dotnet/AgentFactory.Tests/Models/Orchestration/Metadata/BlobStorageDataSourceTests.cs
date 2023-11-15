using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.Metadata
{
    public class BlobStorageDataSourceTests
    {
        private readonly BlobStorageConfiguration _configuration;
        private readonly BlobStorageDataSource _blobStorageDataSource;

        public BlobStorageDataSourceTests()
        {
            _configuration = new BlobStorageConfiguration
            {
                ConnectionStringSecretName = "TestConnectionStringSecretName",
                ContainerName = "TestContainerName",
                Files = new List<string> { "file1", "file2" }
            };

            _blobStorageDataSource = new BlobStorageDataSource { Configuration = _configuration };
        }

        [Fact]
        public void BlobStorageDataSource_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_configuration, _blobStorageDataSource.Configuration);
        }

        [Fact]
        public void BlobStorageDataSource_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_blobStorageDataSource);
            var deserializedBlobStorageDataSource = JsonConvert.DeserializeObject<BlobStorageDataSource>(serializedJson);

            // Assert
            Assert.Equal(_configuration.ConnectionStringSecretName, deserializedBlobStorageDataSource?.Configuration?.ConnectionStringSecretName);
            Assert.Equal(_configuration.ContainerName, deserializedBlobStorageDataSource?.Configuration?.ContainerName);
            Assert.Equal(_configuration.Files, deserializedBlobStorageDataSource?.Configuration?.Files);
        }
    }
}
