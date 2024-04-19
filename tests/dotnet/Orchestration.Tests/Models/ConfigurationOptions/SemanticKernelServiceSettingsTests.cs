using FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions;
using Xunit;

namespace FoundationaLLM.Orchestration.Tests.Models.ConfigurationOptions
{
    public class SemanticKernelServiceSettingsTests
    {
        [Fact]
        public void SemanticKernelServiceSettings_WhenInitialized_ShouldInheritAPIUrlAndAPIKeyProperty()
        {
            // Arrange
            string apiUrl = "http://nsubstitute.io";
            string apiKey = "testApiKey";

            // Act
            var settings = new SemanticKernelServiceSettings { APIUrl = apiUrl, APIKey = apiKey };

            // Assert
            Assert.Equal(apiUrl, settings.APIUrl);
            Assert.Equal(apiKey, settings.APIKey);
        }
    }
}
