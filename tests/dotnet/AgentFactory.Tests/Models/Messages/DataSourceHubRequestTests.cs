using FoundationaLLM.AgentFactory.Core.Models.Messages;

namespace FoundationaLLM.AgentFactory.Tests.Models.Messages
{
    public class DataSourceHubRequestTests
    {
        [Fact]
        public void DataSourceHubRequest_WhenInitialized_ShouldSetDataSourcesProperty()
        {
            // Arrange
            var dataSources = new List<string> { "Source1", "Source2", "Source3" };

            // Act
            var request = new DataSourceHubRequest { DataSources = dataSources };

            // Assert
            Assert.Equal(dataSources, request.DataSources);
        }
    }
}
