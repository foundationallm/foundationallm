using FoundationaLLM.AgentFactory.Core.Models.Messages;

namespace FoundationaLLM.AgentFactory.Tests.Models.Messages
{
    public class DataSourceHubResponseTests
    {
        [Fact]
        public void DataSourceHubResponse_WhenInitialized_ShouldSetDataSourcesProperty()
        {
            // Arrange
            var dataSources = new List<DataSourceMetadata>();

            // Act
            var response = new DataSourceHubResponse { DataSources = dataSources };

            // Assert
            Assert.Equal(dataSources, response.DataSources);
        }
    }

    public class DataSourceMetadataTests
    {
        [Fact]
        public void DataSourceMetadata_WhenInitialized_ShouldSetProperties()
        {
            // Arrange
            var authentication = new Dictionary<string, string>
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            };

            var files = new List<string> { "File1", "File2", "File3" };
            var includeTables = new List<string> { "Table1", "Table2" };
            var excludeTables = new List<string> { "Table3", "Table4" };

            // Act
            var metadata = new DataSourceMetadata
            {
                Name = "TestName",
                Description = "TestDescription",
                UnderlyingImplementation = "TestImplementation",
                FileType = "TestFileType",
                Authentication = authentication,
                Container = "TestContainer",
                Files = files,
                Dialect = "TestDialect",
                IncludeTables = includeTables,
                ExcludeTables = excludeTables,
                FewShotExampleCount = 5
            };

            // Assert
            Assert.Equal("TestName", metadata.Name);
            Assert.Equal("TestDescription", metadata.Description);
            Assert.Equal("TestImplementation", metadata.UnderlyingImplementation);
            Assert.Equal("TestFileType", metadata.FileType);
            Assert.Equal(authentication, metadata.Authentication);
            Assert.Equal("TestContainer", metadata.Container);
            Assert.Equal(files, metadata.Files);
            Assert.Equal("TestDialect", metadata.Dialect);
            Assert.Equal(includeTables, metadata.IncludeTables);
            Assert.Equal(excludeTables, metadata.ExcludeTables);
            Assert.Equal(5, metadata.FewShotExampleCount);
        }
    }
}
