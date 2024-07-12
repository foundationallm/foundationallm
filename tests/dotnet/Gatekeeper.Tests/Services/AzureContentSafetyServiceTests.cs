using Azure;
using Azure.AI.ContentSafety;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Models.ContentSafety;
using FoundationaLLM.Gatekeeper.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Gatekeeper.Tests.Services
{
    public class AzureContentSafetyServiceTests
    {
        private readonly AzureContentSafetyService _testedService;
        private readonly IOptions<AzureContentSafetySettings> _settings = Options.Create(new AzureContentSafetySettings
        {
            APIUrl = "https://example.com",
            APIKey = "test-api-key"
        });

        private readonly ILogger<AzureContentSafetyService> _logger = Substitute.For<ILogger<AzureContentSafetyService>>();
        private ContentSafetyClient _client;
        private AzureContentSafetyService _service;

        private readonly ICallContext _callContext = Substitute.For<ICallContext>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();

        public AzureContentSafetyServiceTests()
        {
            _testedService = new AzureContentSafetyService(_callContext, _httpClientFactoryService, _settings, _logger);
            _client = Substitute.ForPartsOf<ContentSafetyClient>(new Uri(_settings.Value.APIUrl), new AzureKeyCredential(_settings.Value.APIKey));
            _service = new AzureContentSafetyService(_callContext, _httpClientFactoryService, _settings, _logger);
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

            _client.AnalyzeTextAsync(Arg.Any<AnalyzeTextOptions>())
                .Throws(new RequestFailedException("Test error"));

            // Act
            var result = await _testedService.AnalyzeText(content);

            // Assert
            Assert.Equal(expectedResult.Safe, result.Safe);
            Assert.Equal(expectedResult.Reason, result.Reason);
        }
    }
}
