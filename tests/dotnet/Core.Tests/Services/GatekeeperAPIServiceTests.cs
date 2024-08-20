using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Core.Services;
using FoundationaLLM.TestUtils.Helpers;
using NSubstitute;
using System.Net;

namespace FoundationaLLM.Core.Tests.Services
{
    public class GatekeeperAPIServiceTests
    {
        private readonly string _instanceId = "00000000-0000-0000-0000-000000000000";
        private readonly GatekeeperAPIService _testedService;

        private readonly ICallContext _callContext = Substitute.For<ICallContext>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();

        public GatekeeperAPIServiceTests()
        {
            _testedService = new GatekeeperAPIService(_callContext, _httpClientFactoryService);
        }

        #region GetCompletion

        [Fact]
        public async Task GetCompletion_SuccessfulCompletionResponse()
        {
            // Arrange
            var expected = new CompletionResponse { Completion = "Test Completion" };
            var completionRequest = new CompletionRequest { UserPrompt = "Test Prompt", MessageHistory = new List<MessageHistoryItem>() };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, expected);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>(), _callContext.CurrentUserIdentity).Returns(httpClient);

            // Act
            var actual = await _testedService.GetCompletion(_instanceId, completionRequest);

            // Assert
            Assert.NotNull(actual);
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public async Task GetCompletion_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var expected = new CompletionResponse { Completion = "A problem on my side prevented me from responding." };
            var completionRequest = new CompletionRequest { UserPrompt = "Test Prompt", MessageHistory = new List<MessageHistoryItem>() };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>(), _callContext.CurrentUserIdentity).Returns(httpClient);

            // Act
            var actual = await _testedService.GetCompletion(_instanceId, completionRequest);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.Completion, actual.Completion);
        }

        #endregion
    }
}
