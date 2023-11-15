using FoundationaLLM.AgentFactory.Core.Models.ConfigurationOptions;

namespace FoundationaLLM.AgentFactory.Tests.Models.ConfigurationOptions
{
    public class AgentFactorySettingsTests
    {
        [Fact]
        public void AgentFactorySettings_WhenInitialized_ShouldSetDefaultOrchestrationService()
        {
            // Arrange
            string defaultOrchestrationService = "TestOrchestrationService";

            // Act
            var settings = new AgentFactorySettings { DefaultOrchestrationService = defaultOrchestrationService };

            // Assert
            Assert.Equal(defaultOrchestrationService, settings.DefaultOrchestrationService);
        }

        [Fact]
        public void AgentFactorySettings_DefaultOrchestrationService_ShouldBeNullWhenNotSet()
        {
            // Arrange
            var settings = new AgentFactorySettings();

            // Assert
            Assert.Null(settings.DefaultOrchestrationService);
        }
    }
}
