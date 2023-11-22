using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Services;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Tests.Services
{
    public class LangChainServiceTests
    {
        private readonly IOptions<LangChainServiceSettings> _options = Substitute.For<IOptions<LangChainServiceSettings>>();
        private readonly ILogger<LangChainService> _logger = Substitute.For<ILogger<LangChainService>>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
        private readonly LangChainService _langChainService;

        public LangChainServiceTests()
        {
            _langChainService = new LangChainService(_options, _logger, _httpClientFactoryService);
        }

        [Fact]
        public async Task GetCompletion_Success_ReturnsCompletionResponse()
        {
            // Arrange
            var request = new LLMOrchestrationCompletionRequest
            {
                UserPrompt = "TestUserPrompt",
                Agent = new Agent
                {
                    Name = "summarizer",
                    Type = "summary",
                    Description = "Test Description",
                    PromptPrefix = "Prefix_1", 
                    PromptSuffix = "Sufix_1"
                },
                LanguageModel = new LanguageModel
                {
                    Type = "TestType",
                    Provider = "TestProvider",
                    Temperature = 0.5f,
                    UseChat = true
                }
            };

            var serializedRequest = JsonConvert.SerializeObject(request);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(serializedRequest, Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.OK
            };

            var httpClient = new HttpClient(new FakeMessageHandler(response))
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.LangChainAPI).Returns(httpClient);

            // Act
            var result = await _langChainService.GetCompletion(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestUserPrompt", result.UserPrompt);
        }

        [Fact]
        public async Task GetSummary_Success_ReturnsSummary()
        {
            // Arrange
            var userPrompt = new LLMOrchestrationCompletionResponse
            {
                Completion = "TestCompletion",
                CompletionTokens = 100,
                PromptTokens = 40,
                TotalCost = 10,
                TotalTokens = 10,
                UserPrompt = "TestUserPrompt"
            };

            var request = new LLMOrchestrationRequest
            {
                SessionId = "TestSessionId",
                UserPrompt = "TestUserPrompt"
            };

            var userPromptSerialized = JsonConvert.SerializeObject(userPrompt);

            var response = new HttpResponseMessage
            {
                Content = new StringContent(userPromptSerialized, Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.OK
            };

            var httpClient = new HttpClient(new FakeMessageHandler(response))
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.LangChainAPI).Returns(httpClient);

            // Act
            var result = await _langChainService.GetSummary(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userPrompt.Completion, result);
        }
    }
}
