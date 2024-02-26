using FoundationaLLM.Common.Models.Configuration.API;

namespace FoundationaLLM.Common.Tests.Models.Configuration.API
{
    public class SettingsBaseTests
    {
        [Fact]
        public void SettingsBase_APIUrl_SetCorrectly()
        {
            // Arrange
            var apiUrl = "https://example.com/api";
            var settingsBase = new SettingsBase { APIUrl = apiUrl };

            // Act
            var apiUrlSet = settingsBase.APIUrl;

            // Assert
            Assert.Equal(apiUrl, apiUrlSet);
        }

        [Fact]
        public void SettingsBase_APIKey_SetCorrectly()
        {
            // Arrange
            var apiKey = "ABC123";
            var settingsBase = new SettingsBase { APIKey = apiKey };

            // Act
            var apiKeySet = settingsBase.APIKey;

            // Assert
            Assert.Equal(apiKey, apiKeySet);
        }
    }
}
