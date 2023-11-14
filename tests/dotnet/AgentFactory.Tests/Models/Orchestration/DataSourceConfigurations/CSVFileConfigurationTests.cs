using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.DataSourceConfigurations
{
    public class CSVFileConfigurationTests
    {
        private readonly string _sourceFilePath = "TestSourceFilePath";
        private readonly bool _pathValueIsSecret = false;
        private readonly CSVFileConfiguration _configuration;

        public CSVFileConfigurationTests()
        {
            _configuration = new CSVFileConfiguration
            {
                SourceFilePath = _sourceFilePath,
                PathValueIsSecret = _pathValueIsSecret
            };
        }

        [Fact]
        public void CSVFileConfiguration_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_sourceFilePath, _configuration.SourceFilePath);
            Assert.Equal(_pathValueIsSecret, _configuration.PathValueIsSecret);
        }

        [Fact]
        public void CSVFileConfiguration_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_configuration);
            var deserializedConfig = JsonConvert.DeserializeObject<CSVFileConfiguration>(serializedJson);

            // Assert
            Assert.Equal(_configuration.SourceFilePath, deserializedConfig?.SourceFilePath);
            Assert.Equal(_configuration.PathValueIsSecret, deserializedConfig?.PathValueIsSecret);
        }
    }
}
