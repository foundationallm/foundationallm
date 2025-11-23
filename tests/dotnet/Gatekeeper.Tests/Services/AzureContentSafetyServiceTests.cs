using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.ContentSafety;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ContentSafety;
using FoundationaLLM.Common.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Gatekeeper.Tests.Services
{
    public class AzureContentSafetyServiceTests
    {
        private readonly AzureContentSafetyService _testedService;

        private readonly ILogger<AzureContentSafetyService> _logger = Substitute.For<ILogger<AzureContentSafetyService>>();
        private readonly IOptions<AzureContentSafetySettings> _settings = Substitute.For<IOptions<AzureContentSafetySettings>>();
        private readonly IOptions<InstanceSettings> _instanceSettings = Substitute.For<IOptions<InstanceSettings>>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();

        public AzureContentSafetyServiceTests()
        {
            _testedService = new AzureContentSafetyService(_instanceSettings, _httpClientFactoryService, _settings, _logger);
        }

        [Fact]
        public async Task AnalyzeText_RequestFailedException_ReturnsExpectedResult()
        {
            // Arrange
            var content = "This is a content.";
            var expectedResult = new ContentSafetyAnalysisResult
            {
                SafeContent = false,
                Details = "The content safety service was unable to validate the prompt text due to an internal error."
            };

            // Act
            var result = await _testedService.AnalyzeText(content);

            // Assert
            Assert.Equal(expectedResult.SafeContent, result.SafeContent);
            Assert.Equal(expectedResult.Details, result.Details);
        }
    }
}
