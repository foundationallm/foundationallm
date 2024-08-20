using FoundationaLLM.Common.Authentication;

namespace FoundationaLLM.Common.Tests.Authentication
{
    public class APIKeyValidationSettingsTests
    {
        [Fact]
        public void APIKeyValidationSettings_APIKeySecretName_SetCorrectly()
        {
            // Arrange
            var settings = new APIKeyValidationSettings
            {
                APIKey = "API_KEY_SECRET"
            };

            // Assert
            Assert.Equal("API_KEY_SECRET", settings.APIKey);
        }
    }
}
