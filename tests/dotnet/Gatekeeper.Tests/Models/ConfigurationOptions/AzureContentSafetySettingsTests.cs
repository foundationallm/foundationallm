using FoundationaLLM.Common.Models.Configuration.ContentSafety;

namespace Gatekeeper.Tests.Models.ConfigurationOptions
{
    public class AzureContentSafetySettingsTests
    {
        [Fact]
        public void AzureContentSafetySettings_Properties_SetCorrectly()
        {
            // Arrange
            var azureContentSafetySettings = new AzureContentSafetySettings
            {
                HateSeverity = 1,
                ViolenceSeverity = 2,
                SelfHarmSeverity = 3,
                SexualSeverity = 4
            };

            // Assert
            Assert.Equal(1, azureContentSafetySettings.HateSeverity);
            Assert.Equal(2, azureContentSafetySettings.ViolenceSeverity);
            Assert.Equal(3, azureContentSafetySettings.SelfHarmSeverity);
            Assert.Equal(4, azureContentSafetySettings.SexualSeverity);
        }
    }
}
