using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.Metadata
{
    public class SQLDatabaseDataSourceTests
    {
        private readonly SQLDatabaseConfiguration _configuration;
        private readonly SQLDatabaseDataSource _sqlDatabaseDataSource;

        public SQLDatabaseDataSourceTests()
        {
            _configuration = new SQLDatabaseConfiguration
            {
                Dialect = "TestDialect",
                Host = "TestHost",
                Port = 1234,
                DatabaseName = "TestDatabaseName",
                Username = "TestUsername",
                IncludeTables = new List<string> { "table1", "table2" },
                FewShotExampleCount = 5
            };

            _sqlDatabaseDataSource = new SQLDatabaseDataSource { Configuration = _configuration };
        }

        [Fact]
        public void SQLDatabaseDataSource_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_configuration, _sqlDatabaseDataSource.Configuration);
        }

        [Fact]
        public void SQLDatabaseDataSource_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_sqlDatabaseDataSource);
            var deserializedSQLDatabaseDataSource = JsonConvert.DeserializeObject<SQLDatabaseDataSource>(serializedJson);

            // Assert
            Assert.Equal(_configuration.Dialect, deserializedSQLDatabaseDataSource?.Configuration?.Dialect);
            Assert.Equal(_configuration.Host, deserializedSQLDatabaseDataSource?.Configuration?.Host);
            Assert.Equal(_configuration.Port, deserializedSQLDatabaseDataSource?.Configuration?.Port);
            Assert.Equal(_configuration.DatabaseName, deserializedSQLDatabaseDataSource?.Configuration?.DatabaseName);
            Assert.Equal(_configuration.Username, deserializedSQLDatabaseDataSource?.Configuration?.Username);
            Assert.Equal(_configuration.IncludeTables, deserializedSQLDatabaseDataSource?.Configuration?.IncludeTables);
            Assert.Equal(_configuration.FewShotExampleCount, deserializedSQLDatabaseDataSource?.Configuration?.FewShotExampleCount);
        }
    }
}
