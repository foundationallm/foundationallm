using FoundationaLLM.AgentFactory.Core.Models.ConfigurationOptions;

namespace FoundationaLLM.AgentFactory.Tests.Models.ConfigurationOptions
{
    public class SettingsBaseTests
    {
        [Fact]
        public void SettingsBase_WhenInitialized_ShouldSetAPIUrlAndAPIKey()
        {
            // Arrange
            string apiUrl = "http://nsubstitute.io";
            string apiKey = "testApiKey";

            // Act
            var settings = new SettingsBase { APIUrl = apiUrl, APIKey = apiKey };

            // Assert
            Assert.Equal(apiUrl, settings.APIUrl);
            Assert.Equal(apiKey, settings.APIKey);
        }

        [Fact]
        public void SettingsBase_APIUrl_ShouldBeNullWhenNotSet()
        {
            // Arrange
            var settings = new SettingsBase();

            // Assert
            Assert.Null(settings.APIUrl);
        }

        [Fact]
        public void SettingsBase_APIKey_ShouldBeNullWhenNotSet()
        {
            // Arrange
            var settings = new SettingsBase();

            // Assert
            Assert.Null(settings.APIKey);
        }
    }
}
