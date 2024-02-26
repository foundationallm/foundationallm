using FoundationaLLM.Common.Models.Configuration.API;

namespace FoundationaLLM.Common.Tests.Models.Configuration.API
{
    public class PromptHubSettingsTests
    {
        [Fact]
        public void PromptHubSettings_InheritsFrom_SettingsBase()
        {
            // Arrange
            var promptHubSettings = new PromptHubSettings();

            // Act
            bool inherits = promptHubSettings is SettingsBase;

            // Assert
            Assert.True(inherits);
        }

    }
}
