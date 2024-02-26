using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.Common.Models.Orchestration.DataSources;

namespace FoundationaLLM.Common.Tests.Models.Orchestration.DataSources
{
    public class BlobStorageDataSourceTests
    {
        [Fact]
        public void BlobStorageDataSource_Configuration_Property_Test()
        {
            // Arrange
            var dataSource = new BlobStorageDataSource();
            var configuration = new BlobStorageConfiguration();

            // Act
            dataSource.Configuration = configuration;

            // Assert
            Assert.Equal(configuration, dataSource.Configuration);
        }
    }
}
