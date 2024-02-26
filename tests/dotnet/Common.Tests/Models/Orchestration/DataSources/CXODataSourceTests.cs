using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.Common.Models.Orchestration.DataSources;

namespace FoundationaLLM.Common.Tests.Models.Orchestration.DataSources
{
    public class CXODataSourceTests
    {
        [Fact]
        public void CXODataSource_Configuration_Property_Test()
        {
            // Arrange
            var dataSource = new CXODataSource();
            var configuration = new CXOConfiguration() { Company = "Test_company", RetrieverMode = "Test_retriever"};

            // Act
            dataSource.Configuration = configuration;

            // Assert
            Assert.Equal(configuration, dataSource.Configuration);
        }
    }
}
