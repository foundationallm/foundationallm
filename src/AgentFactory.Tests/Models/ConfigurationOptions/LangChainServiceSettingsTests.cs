using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;

namespace FoundationaLLM.AgentFactory.Tests.Models.ConfigurationOptions
{
    public class LangChainServiceSettingsTests
    {
        [Fact]
        public void LangChainServiceSettings_WhenInitialized_ShouldInheritAPIUrlAndAPIKeyProperty()
        {
            // Arrange
            string apiUrl = "http://nsubstitute.io";
            string apiKey = "testApiKey";

            // Act
            var settings = new LangChainServiceSettings { APIUrl = apiUrl, APIKey = apiKey };

            // Assert
            Assert.Equal(apiUrl, settings.APIUrl);
            Assert.Equal(apiKey, settings.APIKey);
        }
    }
}
