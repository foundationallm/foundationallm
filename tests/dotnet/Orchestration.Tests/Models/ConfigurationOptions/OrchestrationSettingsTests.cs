using FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions;
using Xunit;

namespace FoundationaLLM.Orchestration.Tests.Models.ConfigurationOptions
{
    public class OrchestrationSettingsTests
    {
        [Fact]
        public void OrchestrationSettings_WhenInitialized_ShouldSetDefaultOrchestrationService()
        {
            // Arrange
            string defaultOrchestrationService = "TestOrchestrationService";

            // Act
            var settings = new OrchestrationSettings { DefaultOrchestrationService = defaultOrchestrationService };

            // Assert
            Assert.Equal(defaultOrchestrationService, settings.DefaultOrchestrationService);
        }

        [Fact]
        public void OrchestrationSettings_DefaultOrchestrationService_ShouldBeNullWhenNotSet()
        {
            // Arrange
            var settings = new OrchestrationSettings();

            // Assert
            Assert.Null(settings.DefaultOrchestrationService);
        }
    }
}
