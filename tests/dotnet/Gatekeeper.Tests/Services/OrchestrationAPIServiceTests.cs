using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.TestUtils.Helpers;
using NSubstitute;
using System.Net;

namespace Gatekeeper.Tests.Services
{
    public class OrchestrationAPIServiceTests
    {
        private readonly string _instanceId = "00000000-0000-0000-0000-000000000000";
        private readonly DownstreamAPIService _testedService;

        private readonly ICallContext _callContext = Substitute.For<ICallContext>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
        
        public OrchestrationAPIServiceTests()
        {
            _testedService = new DownstreamAPIService(
                HttpClientNames.AgentHubAPI,
                _callContext,
                _httpClientFactoryService,
                null);
        }

        [Fact]
        public async Task GetCompletion_SuccessfulCompletionResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest {OperationId = Guid.NewGuid().ToString(), UserPrompt = "Prompt_1", MessageHistory = new List<MessageHistoryItem>() };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new CompletionResponse {OperationId = completionRequest.OperationId, Completion = "Test Completion" });

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>(), _callContext.CurrentUserIdentity).Returns(httpClient);

            // Act
            var completionResponse = await _testedService.GetCompletion(_instanceId, completionRequest);

            // Assert
            Assert.NotNull(completionResponse);
            Assert.Equal("Test Completion", completionResponse.Completion);
        }

        [Fact]
        public async Task GetCompletion_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest { OperationId = Guid.NewGuid().ToString(), UserPrompt = "Prompt_1", MessageHistory = new List<MessageHistoryItem>() };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>(), _callContext.CurrentUserIdentity).Returns(httpClient);

            // Act
            var completionResponse = await _testedService.GetCompletion(_instanceId, completionRequest);

            // Assert
            Assert.NotNull(completionResponse);
            Assert.Equal("A problem on my side prevented me from responding.", completionResponse.Completion);
        }
    }
}
