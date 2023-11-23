using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Core.Services;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Tests.Services
{
    public class AgentHubAPIServiceTests
    {
        private readonly IOptions<AgentHubSettings> _options = Substitute.For<IOptions<AgentHubSettings>>();
        private readonly ILogger<AgentHubAPIService> _logger = Substitute.For<ILogger<AgentHubAPIService>>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
        private readonly AgentHubAPIService _agentHubAPIService;

        public AgentHubAPIServiceTests()
        {
            _agentHubAPIService = new AgentHubAPIService(_options, _logger, _httpClientFactoryService);
        }

        [Fact]
        public async Task Status_Success_ReturnsStatus()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("TestStatus", Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.OK
            };

            var httpClient = new HttpClient(new FakeMessageHandler(response))
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(HttpClients.AgentHubAPI).Returns(httpClient);

            // Act
            var result = await _agentHubAPIService.Status();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestStatus", result);
        }

        [Fact]
        public async Task ResolveRequest_Success_ReturnsAgentHubResponse()
        {
            // Arrange
            var userPrompt = "TestUserPrompt";
            var sessionId = "TestSessionId";

            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new AgentHubResponse
                { 
                    Agent = new AgentMetadata { Name = "Agent1"}

                }), Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.OK
            };

            var httpClient = new HttpClient(new FakeMessageHandler(response))
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentHubAPI).Returns(httpClient);

            // Act
            var result = await _agentHubAPIService.ResolveRequest(userPrompt, sessionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Agent1", result?.Agent?.Name);
        }
    }
}
