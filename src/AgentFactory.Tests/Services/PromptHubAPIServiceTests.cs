using FoundationaLLM.AgentFactory.Core.Services;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Tests.Services
{
    public class PromptHubAPIServiceTests
    {
        private readonly IOptions<PromptHubSettings> _options = Substitute.For<IOptions<PromptHubSettings>>();
        private readonly ILogger<PromptHubAPIService> _logger = Substitute.For<ILogger<PromptHubAPIService>>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
        private readonly PromptHubAPIService _promptHubAPIService;

        public PromptHubAPIServiceTests()
        {
            _promptHubAPIService = new PromptHubAPIService(_options, _logger, _httpClientFactoryService);
        }

        [Fact]
        public async Task ResolveRequest_Success_ReturnsPromptHubResponse()
        {
            // Arrange
            var agentName = "TestAgentName";

            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://example.com")
            };

            _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.PromptHubAPI).Returns(httpClient);

            // Act
            var result = await _promptHubAPIService.ResolveRequest(agentName);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Status_Success_ReturnsStatus()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Status content")
            };

            var client = new HttpClient(new FakeMessageHandler(responseMessage))
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };

            _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.PromptHubAPI).Returns(client);

            // Act
            var result = await _promptHubAPIService.Status();

            // Assert
            Assert.Equal("Status content", result);
        }
    }
}
