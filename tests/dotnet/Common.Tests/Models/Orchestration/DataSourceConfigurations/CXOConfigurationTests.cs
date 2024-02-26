using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;

namespace FoundationaLLM.Common.Tests.Models.Orchestration.DataSourceConfigurations
{
    public class CXOConfigurationTests
    {
        [Fact]
        public void CXOConfiguration_Properties_Initialized_Correctly()
        {
            // Arrange
            var configuration = new CXOConfiguration() { Company = "Test_company", RetrieverMode = "Test_mode"};

            // Assert
            Assert.Equal("cxo", configuration.ConfigurationType);
            Assert.Null(configuration.Sources);
            Assert.NotNull(configuration.RetrieverMode);
            Assert.NotNull(configuration.Company);
        }
    }
}
