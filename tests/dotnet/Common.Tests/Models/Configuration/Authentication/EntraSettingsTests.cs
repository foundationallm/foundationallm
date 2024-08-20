using FoundationaLLM.Common.Models.Configuration.Authentication;

namespace FoundationaLLM.Common.Tests.Models.Configuration.Authentication
{
    public class EntraSettingsTests
    {
        [Fact]
        public void EntraSettings_Scopes_SetCorrectly()
        {
            // Arrange
            var entraSettings = new EntraSettings();
            var testScopes = "Scope_1";

            // Act
            entraSettings.Scopes = testScopes;

            // Assert
            Assert.Equal(testScopes, entraSettings.Scopes);
        }
    }
}
