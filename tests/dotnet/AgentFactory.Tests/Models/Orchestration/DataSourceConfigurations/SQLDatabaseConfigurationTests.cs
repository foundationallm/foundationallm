using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.DataSourceConfigurations
{
    public class SQLDatabaseConfigurationTests
    {
        private readonly string _dialect = "TestDialect";
        private readonly string _host = "TestHost";
        private readonly int _port = 1234;
        private readonly string _databaseName = "TestDatabaseName";
        private readonly string _username = "TestUsername";
        private readonly List<string> _includeTables = new List<string> { "table1", "table2" };
        private readonly List<string> _excludeTables = new List<string> { "table3", "table4" };
        private readonly int _fewShotExampleCount = 5;
        private readonly SQLDatabaseConfiguration _configuration;

        public SQLDatabaseConfigurationTests()
        {
            _configuration = new SQLDatabaseConfiguration
            {
                Dialect = _dialect,
                Host = _host,
                Port = _port,
                DatabaseName = _databaseName,
                Username = _username,
                IncludeTables = _includeTables,
                ExcludeTables = _excludeTables,
                FewShotExampleCount = _fewShotExampleCount
            };
        }

        [Fact]
        public void SQLDatabaseConfiguration_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_dialect, _configuration.Dialect);
            Assert.Equal(_host, _configuration.Host);
            Assert.Equal(_port, _configuration.Port);
            Assert.Equal(_databaseName, _configuration.DatabaseName);
            Assert.Equal(_username, _configuration.Username);
            Assert.Equal(_includeTables, _configuration.IncludeTables);
            Assert.Equal(_excludeTables, _configuration.ExcludeTables);
            Assert.Equal(_fewShotExampleCount, _configuration.FewShotExampleCount);
        }

        [Fact]
        public void SQLDatabaseConfiguration_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_configuration);
            var deserializedConfig = JsonConvert.DeserializeObject<SQLDatabaseConfiguration>(serializedJson);

            // Assert
            Assert.Equal(_configuration.Dialect, deserializedConfig?.Dialect);
            Assert.Equal(_configuration.Host, deserializedConfig?.Host);
            Assert.Equal(_configuration.Port, deserializedConfig?.Port);
            Assert.Equal(_configuration.DatabaseName, deserializedConfig?.DatabaseName);
            Assert.Equal(_configuration.Username, deserializedConfig?.Username);
            Assert.Equal(_configuration.IncludeTables, deserializedConfig?.IncludeTables);
            Assert.Equal(_configuration.ExcludeTables, deserializedConfig?.ExcludeTables);
            Assert.Equal(_configuration.FewShotExampleCount, deserializedConfig?.FewShotExampleCount);
        }
    }
}
