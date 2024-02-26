using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.Common.Models.Orchestration.DataSources;

namespace FoundationaLLM.Common.Tests.Models.Orchestration.DataSources
{
    public class SearchServiceDataSourceTests
    {
        [Fact]
        public void SearchServiceDataSource_Configuration_Property_Test()
        {
            // Arrange
            var dataSource = new SearchServiceDataSource();
            var configuration = new SearchServiceConfiguration();

            // Act
            dataSource.Configuration = configuration;

            // Assert
            Assert.Equal(configuration, dataSource.Configuration);
        }
    }
}
