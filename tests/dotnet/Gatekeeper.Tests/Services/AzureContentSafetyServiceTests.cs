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

        private readonly IOptions<AzureContentSafetySettings> _options = Substitute.For<IOptions<AzureContentSafetySettings>>();
        private readonly ILogger<AzureContentSafetyService> _logger = Substitute.For<ILogger<AzureContentSafetyService>>();

        public AzureContentSafetyServiceTests()
        {
            _testedService = new AzureContentSafetyService(_options, _logger);
        }

        [Fact]
        public async Task AnalyzeText_ShouldReturnTheTextAnalysisResult()
        {
            // Arrange
            var text = "Safe content.";

            var expectedResult = new AnalyzeTextFilterResult { Safe = true, Reason = string.Empty };

            // Act
            var actualResult = await _testedService.AnalyzeText(text);

            // Assert
            Assert.Equivalent(expectedResult, actualResult);
        }
    }
}
