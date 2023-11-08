using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;

namespace FoundationaLLM.AgentFactory.Tests.Models.ConfigurationOptions
{
    public class PromptHubSettingsTests
    {
        [Fact]
        public void PromptHubSettings_WhenInitialized_ShouldInheritAPIUrlAndAPIKeyProperty()
        {
            // Arrange
            string apiUrl = "http://nsubstitute.io";
            string apiKey = "testApiKey";

            // Act
            var settings = new PromptHubSettings { APIUrl = apiUrl, APIKey = apiKey };

            // Assert
            Assert.Equal(apiUrl, settings.APIUrl);
            Assert.Equal(apiKey, settings.APIKey);
        }
    }
}
