using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models;
using FoundationaLLM.Core.Models.Configuration;
using FoundationaLLM.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
//using Microsoft.Graph.Models.CallRecords;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace FoundationaLLM.Core.Tests.Services
{
    public class CoreServiceTests
    {
        private readonly CoreService _testedService;

        private readonly ICosmosDbService _cosmosDbService = Substitute.For<ICosmosDbService>();
        private readonly IGatekeeperAPIService _gatekeeperAPIService = Substitute.For<IGatekeeperAPIService>();
        private readonly ICallContext _callContext = Substitute.For<ICallContext>();
        private readonly ILogger<CoreService> _logger = Substitute.For<ILogger<CoreService>>();
        private readonly IOptions<ClientBrandingConfiguration> _brandingConfig = Substitute.For<IOptions<ClientBrandingConfiguration>>();
        private readonly IEnumerable<IDownstreamAPIService> _downstreamAPIServices;
        private IOptions<CoreServiceSettings> _options;

        public CoreServiceTests()
        {
            var gatekeeperAPIDownstream = Substitute.For<IDownstreamAPIService>();
            gatekeeperAPIDownstream.APIName.Returns(HttpClients.GatekeeperAPI);

            var orchestrationAPIDownstream = Substitute.For<IDownstreamAPIService>();
            orchestrationAPIDownstream.APIName.Returns(HttpClients.OrchestrationAPI);

            _downstreamAPIServices = new List<IDownstreamAPIService>
            {
                gatekeeperAPIDownstream,
                orchestrationAPIDownstream
            };

            _options = Options.Create(new CoreServiceSettings {
                BypassGatekeeper =  true, 
                SessionSummarization = ChatSessionNameSummarizationType.LLM
            });

            _brandingConfig.Value.Returns(new ClientBrandingConfiguration());
            _callContext.CurrentUserIdentity.Returns(new UnifiedUserIdentity
            {
                Name = "Test User",
                UPN = "test@foundationallm.ai",
                Username = "test@foundationallm.ai"
            });
            _testedService = new CoreService(_cosmosDbService, _downstreamAPIServices, _logger, _brandingConfig, _options, _callContext);
        }

        #region GetAllChatSessionsAsync

        [Fact]
        public async Task GetAllChatSessionsAsync_ShouldReturnAllChatSessions()
        {
            // Arrange
            var expectedSessions = new List<Session>() { new Session() };
            _cosmosDbService.GetSessionsAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(expectedSessions);

            // Act
            var actualSessions = await _testedService.GetAllChatSessionsAsync();

            // Assert
            Assert.Equivalent(expectedSessions, actualSessions);
        }

        #endregion

        #region GetChatSessionMessagesAsync

        [Fact]
        public async Task GetChatSessionMessagesAsync_ShouldReturnAllChatSessionMessages()
        {
            // Arrange
            string sessionId = Guid.NewGuid().ToString();
            var message = new Message(sessionId, "sender", 0, "text", null, null, "test_upn");
            var expectedMessages = new List<Message>() { message };

            _cosmosDbService.GetSessionMessagesAsync(sessionId, Arg.Any<string>())
                .Returns(expectedMessages);

            // Act
            var messages = await _testedService.GetChatSessionMessagesAsync(sessionId);

            // Assert
            Assert.Equal(expectedMessages, messages);
        }

        [Fact]
        public async Task GetChatSessionMessagesAsync_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            string sessionId = null!;
            _cosmosDbService.GetSessionMessagesAsync(sessionId, "").ReturnsNull();

            // Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.GetChatSessionMessagesAsync(sessionId);
            });
        }

        #endregion

        #region CreateNewChatSessionAsync

        [Fact]
        public async Task CreateNewChatSessionAsync_ShouldReturnANewChatSession()
        {
            // Arrange
            var currentUserUPN = "testuser@example.com";
            var sessionType = "Test_type";
            var newSession = new Session { Type = sessionType, UPN = currentUserUPN };

            // Set up mock returns
            _callContext.CurrentUserIdentity.Returns(new UnifiedUserIdentity { UPN = currentUserUPN });

            _cosmosDbService.InsertSessionAsync(Arg.Any<Session>())
                .Returns(Task.FromResult(newSession));

            // Act
            var resultSession = await _testedService.CreateNewChatSessionAsync();

            // Assert
            Assert.NotNull(resultSession);
            Assert.Equal(sessionType, resultSession.Type);
            Assert.Equal(currentUserUPN, resultSession.UPN);
        }

        #endregion

        #region RenameChatSessionAsync

        [Fact]
        public async Task RenameChatSessionAsync_ShouldReturnTheRenamedChatSession()
        {
            // Arrange
            var session = new Session() { Name = "OldName" };
            var expectedName = "NewName";

            var expectedSession = new Session()
            {
                Id = session.Id,
                Messages = session.Messages,
                Name = expectedName,
                SessionId = session.SessionId,
                TokensUsed = session.TokensUsed,
                Type = session.Type,
            };
            _cosmosDbService.UpdateSessionNameAsync(session.Id, expectedName).Returns(expectedSession);

            // Act
            var actualSession = await _testedService.RenameChatSessionAsync(session.Id, expectedName);

            // Assert
            Assert.Equivalent(expectedSession, actualSession);
            Assert.Equal(expectedName, actualSession.Name);
        }

        [Fact]
        public async Task RenameChatSessionAsync_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var newChatSessionName = "NewName";

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.RenameChatSessionAsync(null!, newChatSessionName);
            });
        }

        [Fact]
        public async Task RenameChatSessionAsync_ShouldThrowExceptionWhenNewChatSessionNameIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.RenameChatSessionAsync(sessionId, null!);
            });

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _testedService.RenameChatSessionAsync(sessionId, string.Empty);
            });
        }

        #endregion

        #region DeleteChatSessionAsync

        [Fact]
        public async Task DeleteChatSessionAsync_ShouldSucceed()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var expected = Task.CompletedTask;
            _cosmosDbService.DeleteSessionAndMessagesAsync(sessionId).Returns(expected);

            // Act
            Task actual = _testedService.DeleteChatSessionAsync(sessionId);
            await actual;

            // Assert
            Assert.True(actual.IsCompletedSuccessfully);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task DeleteChatSessionAsync_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.DeleteChatSessionAsync(null!);
            });
        }

        #endregion

        #region GetChatCompletionAsync

        [Fact]
        public async Task GetChatCompletionAsync_ShouldReturnACompletion()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var userPrompt = "Prompt";
            var orchestrationRequest = new OrchestrationRequest { SessionId = sessionId, UserPrompt = userPrompt };
            var upn = "test@foundationallm.ai";
            var expectedCompletion = new Completion() { Text = "Completion" };

            var expectedMessages = new List<Message>();
            _cosmosDbService.GetSessionMessagesAsync(sessionId, upn).Returns(expectedMessages);

            var completionResponse = new CompletionResponse() { Completion = "Completion" };
            _downstreamAPIServices.Last().GetCompletion(Arg.Any<CompletionRequest>()).Returns(completionResponse);

            _cosmosDbService.GetSessionAsync(sessionId).Returns(new Session());
            _cosmosDbService.UpsertSessionBatchAsync().Returns(Task.CompletedTask);

            // Act
            var actualCompletion = await _testedService.GetChatCompletionAsync(orchestrationRequest);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualCompletion.Text);
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldReturnAnErrorMessageWhenSessionIdIsNull()
        {
            // Arrange
            var userPrompt = "Prompt";
            var orchestrationRequest = new OrchestrationRequest { UserPrompt = userPrompt };
            var expectedCompletion = new Completion { Text = "Could not generate a completion due to an internal error." };

            // Act
            var actualCompletion = await _testedService.GetChatCompletionAsync(orchestrationRequest);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualCompletion.Text);

            //_logger.Received(1).LogError($"Error getting completion in session {sessionId} for user prompt [{userPrompt}].");
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldNotThrowExceptionWhenUserPromptIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var orchestrationRequest = new OrchestrationRequest { SessionId = sessionId, UserPrompt = null! };

            // Act
            var exception = await Record.ExceptionAsync(async () => await _testedService.GetChatCompletionAsync(orchestrationRequest));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldNotThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var userPrompt = "Prompt";
            var orchestrationRequest = new OrchestrationRequest { UserPrompt = userPrompt };

            // Act
            var exception = await Record.ExceptionAsync(async () => await _testedService.GetChatCompletionAsync(orchestrationRequest));

            // Assert
            Assert.Null(exception);
        }

        #endregion

        #region SummarizeChatSessionNameAsync

        [Fact]
        public async Task SummarizeChatSessionNameAsync_ShouldReturnACompletion()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var prompt = "Prompt";
            var summary = "[No Summary]";
            var summaryRequest = new SummaryRequest
            {
                SessionId = sessionId,
                UserPrompt = prompt
            };
            var expectedCompletion = new Completion() { Text = summary };

            _gatekeeperAPIService.GetSummary(summaryRequest).Returns(summary);
            _cosmosDbService.UpdateSessionNameAsync(sessionId, summary).Returns(new Session());

            // Act
            var actualCompletion = await _testedService.GenerateChatSessionNameAsync(sessionId, prompt);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualCompletion.Text);
        }

        [Fact]
        public async Task SummarizeChatSessionNameAsync_ShouldReturnAnErrorMessageWhenSessionIdIsNull()
        {
            // Arrange
            var prompt = "Prompt";
            var expectedCompletion = new Completion { Text = "[No Summary]" };

            // Act
            var actualSummary = await _testedService.GenerateChatSessionNameAsync(null, prompt);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualSummary.Text);

            //_logger.Received(1).LogError($"Error getting a summary in session {sessionId} for user prompt [{prompt}].");
        }

        [Fact]
        public async Task SummarizeChatSessionNameAsync_ShouldNotThrowExceptionWhenPromptIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Act
            var exception = await Record.ExceptionAsync(async () => await _testedService.GenerateChatSessionNameAsync(sessionId, null!));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task SummarizeChatSessionNameAsync_ShouldNotThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var prompt = "Prompt";

            // Act
            var exception = await Record.ExceptionAsync(async () => await _testedService.GenerateChatSessionNameAsync(null, prompt));

            // Assert
            Assert.Null(exception);
        }

        #endregion

        #region RateMessageAsync

        [Fact]
        public async Task RateMessageAsync_ShouldReturnARatedMessage()
        {
            // Arrange
            var rating = true;
            var id = Guid.NewGuid().ToString();
            var sessionId = Guid.NewGuid().ToString();
            var upn = "";
            var expectedMessage = new Message(sessionId, string.Empty, default, "Text", null, rating, upn);
            _cosmosDbService.UpdateMessageRatingAsync(id, sessionId, rating).Returns(expectedMessage);

            // Act
            var actualMessage = await _testedService.RateMessageAsync(id, sessionId, rating);

            // Assert
            Assert.Equivalent(expectedMessage, actualMessage);
        }

        [Fact]
        public async Task RateMessageAsync_ShouldThrowExceptionWhenIdIsNull()
        {
            // Arrange
            var rating = true;
            var sessionId = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.RateMessageAsync(null!, sessionId, rating);
            });
        }

        [Fact]
        public async Task RateMessageAsync_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var rating = true;
            var id = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.RateMessageAsync(id, null!, rating);
            });
        }

        #endregion

        #region GetCompletionPrompt

        [Fact]
        public async Task GetCompletionPrompt_ShouldReturnACompletionPrompt()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var completionPromptId = Guid.NewGuid().ToString();
            var expectedPrompt = new CompletionPrompt(sessionId, messageId, "Text");
            _cosmosDbService.GetCompletionPrompt(sessionId, completionPromptId).Returns(expectedPrompt);

            // Act
            var actualPrompt = await _testedService.GetCompletionPrompt(sessionId, completionPromptId);

            // Assert
            Assert.Equivalent(actualPrompt, expectedPrompt);
        }

        [Fact]
        public async Task GetCompletionPrompt_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var completionPromptId = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.GetCompletionPrompt(null!, completionPromptId);
            });
        }

        [Fact]
        public async Task GetCompletionPrompt_ShouldThrowExceptionWhenCompletionPromptIdIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.GetCompletionPrompt(sessionId, null!);
            });
        }

        #endregion
    }
}
