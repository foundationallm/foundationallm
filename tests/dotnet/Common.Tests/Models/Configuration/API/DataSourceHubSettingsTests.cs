using FoundationaLLM.Common.Models.Configuration.API;

namespace FoundationaLLM.Common.Tests.Models.Configuration.API
{
    public class DataSourceHubSettingsTests
    {
        [Fact]
        public void DataSourceHubSettings_InheritsFrom_SettingsBase()
        {
            // Arrange
            var dataSourceHubSettings = new DataSourceHubSettings();

            // Act
            bool inherits = dataSourceHubSettings is SettingsBase;

            // Assert
            Assert.True(inherits);
        }
    }
}
