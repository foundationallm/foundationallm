using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.Metadata
{
    public class CSVFileDataSourceTests
    {
        private readonly CSVFileConfiguration _configuration;
        private readonly CSVFileDataSource _csvFileDataSource;

        public CSVFileDataSourceTests()
        {
            _configuration = new CSVFileConfiguration
            {
                SourceFilePath = "TestSourceFilePath",
                PathValueIsSecret = true
            };

            _csvFileDataSource = new CSVFileDataSource { Configuration = _configuration };
        }

        [Fact]
        public void CSVFileDataSource_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_configuration, _csvFileDataSource.Configuration);
        }

        [Fact]
        public void CSVFileDataSource_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_csvFileDataSource);
            var deserializedCSVFileDataSource = JsonConvert.DeserializeObject<CSVFileDataSource>(serializedJson);

            // Assert
            Assert.Equal(_configuration.SourceFilePath, deserializedCSVFileDataSource?.Configuration?.SourceFilePath);
            Assert.Equal(_configuration.PathValueIsSecret, deserializedCSVFileDataSource?.Configuration?.PathValueIsSecret);
        }
    }
}
