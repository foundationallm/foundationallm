using FoundationaLLM.AgentFactory.Core.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Hubs;
using FoundationaLLM.Common.Models.Orchestration;
using System.Reflection;

namespace FoundationaLLM.AgentFactory.Tests.Orchestration
{
    public class LegacyOrchestrationTests
    {
        private LegacyOrchestration _legacyOrchestration;
        private AgentMetadata _agentMetadata = new AgentMetadata();
        private ICacheService _cacheService = Substitute.For<ICacheService>();
        private ICallContext _callContext = Substitute.For<ICallContext>();
        private ILLMOrchestrationService _orchestrationService = Substitute.For<ILLMOrchestrationService>();
        private IPromptHubAPIService _promptHubService = Substitute.For<IPromptHubAPIService>();
        private IDataSourceHubAPIService _dataSourceHubService = Substitute.For<IDataSourceHubAPIService>();
        private ILogger<LegacyOrchestration> _logger = Substitute.For<ILogger<LegacyOrchestration>>();

        public LegacyOrchestrationTests()
        {
            _legacyOrchestration = new LegacyOrchestration(
                _agentMetadata,
                _cacheService,
                _callContext,
                _orchestrationService,
                _promptHubService,
                _dataSourceHubService,
                _logger);
        }

        [Fact]
        public async Task GetCompletion_ReturnsCompletionResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest() { SessionId = "Session_1", MessageHistory = new List<MessageHistoryItem>(), UserPrompt = "Test_Userprompt" };
            var orchestrationResult = new LLMCompletionResponse { Completion = "Completion" };
            _orchestrationService.GetCompletion(Arg.Any<LegacyCompletionRequest>())
                .Returns(Task.FromResult(orchestrationResult));

            // Initialize _completionRequestTemplate
            var completionRequestTemplateField = typeof(LegacyOrchestration).GetField("_completionRequestTemplate", BindingFlags.NonPublic | BindingFlags.Instance);
            completionRequestTemplateField!.SetValue(_legacyOrchestration, new LegacyCompletionRequest());

            // Act
            var completionResponse = await _legacyOrchestration.GetCompletion(completionRequest);

            // Assert
            Assert.Equal(orchestrationResult.Completion, completionResponse.Completion);
            Assert.Equal(completionRequest.UserPrompt, completionResponse.UserPrompt);
            Assert.Equal(orchestrationResult.FullPrompt, completionResponse.FullPrompt);
            Assert.Equal(orchestrationResult.PromptTemplate, completionResponse.PromptTemplate);
            Assert.Equal(orchestrationResult.AgentName, completionResponse.AgentName);
            Assert.Equal(orchestrationResult.PromptTokens, completionResponse.PromptTokens);
            Assert.Equal(orchestrationResult.CompletionTokens, completionResponse.CompletionTokens);
        }
    }
}
