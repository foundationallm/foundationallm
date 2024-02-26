using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;

namespace FoundationaLLM.Common.Tests.Models.Orchestration.DataSourceConfigurations
{
    public class BlobStorageConfigurationTests
    {
        [Fact]
        public void BlobStorageConfiguration_Properties_Initialized_Correctly()
        {
            // Arrange
            var configuration = new BlobStorageConfiguration();

            // Assert
            Assert.Equal("blob_storage", configuration.ConfigurationType);
            Assert.Null(configuration.ConnectionStringSecretName);
            Assert.Null(configuration.ContainerName);
            Assert.Null(configuration.Files);
        }
    }
}
