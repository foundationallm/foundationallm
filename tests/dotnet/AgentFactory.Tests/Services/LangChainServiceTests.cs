using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Services;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Metadata;
using FoundationaLLM.Common.Models.Orchestration;

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
            var request = new LegacyCompletionRequest
            {
                UserPrompt = "TestUserPrompt",
                Agent = new LegacyAgentMetadata
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
        }
    }
}
