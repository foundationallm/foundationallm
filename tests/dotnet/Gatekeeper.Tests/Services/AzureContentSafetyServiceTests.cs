using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Models.ContentSafety;
using FoundationaLLM.Gatekeeper.Core.Services;
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
        private readonly ICallContext _callContext = Substitute.For<ICallContext>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();

        public AzureContentSafetyServiceTests()
        {
            _testedService = new AzureContentSafetyService(_callContext, _httpClientFactoryService, _settings, _logger);
        }

        [Fact]
        public async Task AnalyzeText_RequestFailedException_ReturnsExpectedResult()
        {
            // Arrange
            var content = "This is a content.";
            var expectedResult = new AnalyzeTextFilterResult
            {
                Safe = false,
                Reason = "The content safety service was unable to validate the prompt text due to an internal error."
            };

            // Act
            var result = await _testedService.AnalyzeText(content);

            // Assert
            Assert.Equal(expectedResult.Safe, result.Safe);
            Assert.Equal(expectedResult.Reason, result.Reason);
        }
    }
}
