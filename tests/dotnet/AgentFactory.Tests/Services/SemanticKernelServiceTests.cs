using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Services;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Tests.Services
{
    public class SemanticKernelServiceTests
    {
        private readonly IOptions<SemanticKernelServiceSettings> options = Substitute.For<IOptions<SemanticKernelServiceSettings>>();
        private readonly ILogger<SemanticKernelService> logger = Substitute.For<ILogger<SemanticKernelService>>();
        private readonly IHttpClientFactoryService httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
        private readonly SemanticKernelService semanticKernelService;

        public SemanticKernelServiceTests()
        {
            semanticKernelService = new SemanticKernelService(options, logger, httpClientFactoryService);
        }

        [Fact]
        public async Task GetCompletion_Success_ReturnsCompletionResponse()
        {
            // Arrange
            var userPrompt = "TestUserPrompt";
            var messageHistory = new List<MessageHistoryItem>();

            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new CompletionResponse
                {
                    Completion = "TestCompletion",
                    UserPrompt = userPrompt,
                    PromptTokens = 40,
                    CompletionTokens = 100,
                    UserPromptEmbedding = new float[] { 0 }
                }), Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.OK
            };

            var httpClient = new HttpClient(new FakeMessageHandler(response))
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI).Returns(httpClient);

            // Act
            var result = await semanticKernelService.GetCompletion(userPrompt, messageHistory);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestCompletion", result.Completion);
            Assert.Equal(userPrompt, result.UserPrompt);
        }

        [Fact]
        public async Task GetSummary_Success_ReturnsSummary()
        {
            // Arrange
            var content = "TestContent";

            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new SummaryResponse
                {
                    Summary = "TestSummary"
                }), Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.OK
            };

            var httpClient = new HttpClient(new FakeMessageHandler(response))
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI).Returns(httpClient);

            // Act
            var result = await semanticKernelService.GetSummary(content);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestSummary", result);
        }
    }
}
