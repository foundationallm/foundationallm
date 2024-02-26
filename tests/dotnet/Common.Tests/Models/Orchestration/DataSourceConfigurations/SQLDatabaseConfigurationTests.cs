using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;

namespace FoundationaLLM.Common.Tests.Models.Orchestration.DataSourceConfigurations
{
    public class SQLDatabaseConfigurationTests
    {
        [Fact]
        public void SQLDatabaseConfiguration_Properties_Initialized_Correctly()
        {
            // Arrange
            var configuration = new SQLDatabaseConfiguration();

            // Assert
            Assert.Equal("sql_database", configuration.ConfigurationType);
            Assert.Null(configuration.Dialect);
            Assert.Null(configuration.Host);
            Assert.Equal(0, configuration.Port);
            Assert.Null(configuration.DatabaseName);
            Assert.Null(configuration.Username);
            Assert.Null(configuration.PasswordSecretSettingKeyName);
            Assert.Empty(configuration.IncludeTables);
            Assert.Empty(configuration.ExcludeTables);
            Assert.Equal(0, configuration.FewShotExampleCount);
            Assert.False(configuration.RowLevelSecurityEnabled);
        }
    }
}
