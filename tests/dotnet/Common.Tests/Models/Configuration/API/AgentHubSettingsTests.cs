using FoundationaLLM.Common.Models.Configuration.API;

namespace FoundationaLLM.Common.Tests.Models.Configuration.API
{
    public class AgentHubSettingsTests
    {
        [Fact]
        public void AgentHubSettings_InheritsFrom_SettingsBase()
        {
            // Arrange
            var agentHubSettings = new AgentHubSettings();

            // Act
            bool inherits = agentHubSettings is SettingsBase;

            // Assert
            Assert.True(inherits);
        }
    }
}
