using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.Common.Models.Orchestration.DataSources;

namespace FoundationaLLM.Common.Tests.Models.Orchestration.DataSources
{
    public class SQLDatabaseDataSourceTests
    {
        [Fact]
        public void SQLDatabaseDataSource_Configuration_Property_Test()
        {
            // Arrange
            var dataSource = new SQLDatabaseDataSource();
            var configuration = new SQLDatabaseConfiguration();

            // Act
            dataSource.Configuration = configuration;

            // Assert
            Assert.Equal(configuration, dataSource.Configuration);
        }
    }
}
