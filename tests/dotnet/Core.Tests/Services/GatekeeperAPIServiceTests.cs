﻿using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Core.Services;
using FoundationaLLM.TestUtils.Helpers;
using NSubstitute;
using System.Net;

namespace FoundationaLLM.Core.Tests.Services
{
    public class GatekeeperAPIServiceTests
    {
        private readonly GatekeeperAPIService _testedService;

        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();

        public GatekeeperAPIServiceTests()
        {
            _testedService = new GatekeeperAPIService(_httpClientFactoryService);
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
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var actual = await _testedService.GetCompletion(completionRequest);

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
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var actual = await _testedService.GetCompletion(completionRequest);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.Completion, actual.Completion);
        }

        #endregion

        #region GetSummary

        [Fact]
        public async Task GetSummary_SuccessfulCompletionResponse()
        {
            // Arrange
            var expected = "Test Response";
            var response = new SummaryResponse { Summary = expected };
            var summaryRequest = new SummaryRequest
            {
                SessionId = "TestSessionId",
                UserPrompt = "Test Prompt"
            };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, response);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var actual = await _testedService.GetSummary(summaryRequest);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetSummary_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var expected = "A problem on my side prevented me from responding.";
            var summaryRequest = new SummaryRequest
            {
                SessionId = "TestSessionId",
                UserPrompt = "Test Prompt"
            };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var actual = await _testedService.GetSummary(summaryRequest);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
