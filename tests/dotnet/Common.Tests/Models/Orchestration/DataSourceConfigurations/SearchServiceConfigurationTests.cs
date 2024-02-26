using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;

namespace FoundationaLLM.Common.Tests.Models.Orchestration.DataSourceConfigurations
{
    public class SearchServiceConfigurationTests
    {
        [Fact]
        public void SearchServiceConfiguration_Properties_Initialized_Correctly()
        {
            // Arrange
            var configuration = new SearchServiceConfiguration();

            // Assert
            Assert.Equal("search_service", configuration.ConfigurationType);
            Assert.Null(configuration.Endpoint);
            Assert.Null(configuration.KeySecret);
            Assert.Null(configuration.IndexName);
            Assert.Null(configuration.TopN);
            Assert.Null(configuration.EmbeddingFieldName);
            Assert.Null(configuration.TextFieldName);
        }
    }
}
