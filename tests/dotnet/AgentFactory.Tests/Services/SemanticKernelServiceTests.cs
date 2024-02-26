﻿using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Services;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
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
            var request = new KnowledgeManagementCompletionRequest
            {
                Agent = new KnowledgeManagementAgent() { Name = "Test_name", ObjectId = "Test_id", Type = "Test_type"}
            };
            var responseContent = System.Text.Json.JsonSerializer.Serialize(new LLMCompletionResponse { Completion = "Completion response" });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(responseContent) };


            var httpClient = new HttpClient(new FakeMessageHandler(responseMessage))
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI).Returns(httpClient);

            // Act
            var result = await semanticKernelService.GetCompletion(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Completion response", result.Completion);
        }
    }
}
