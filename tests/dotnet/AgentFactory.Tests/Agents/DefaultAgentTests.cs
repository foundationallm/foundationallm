using FoundationaLLM.AgentFactory.Core.Agents;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using System.Reflection;

namespace FoundationaLLM.AgentFactory.Tests.Agents
{
    public class DefaultAgentTests
    {
        private readonly ILLMOrchestrationService _orchestrationService = Substitute.For<ILLMOrchestrationService>();
        private readonly IPromptHubAPIService _promptHubService = Substitute.For<IPromptHubAPIService>();
        private readonly IDataSourceHubAPIService _dataSourceHubService = Substitute.For<IDataSourceHubAPIService>();
        private readonly AgentMetadata _agentMetadata = new AgentMetadata
        {
            Name = "TestAgent",
            AllowedDataSourceNames = ["TestDataSource"],
            LanguageModel = new LanguageModel
            {
                Type = "TestModelType",
                Provider = "TestProvider",
                Temperature = 0.5f,
                UseChat = true
            }
        };
        private readonly PromptHubResponse _promptResponse = new()
        {
            Prompt = new PromptMetadata
            {
                PromptPrefix = "TestPrefix",
                PromptSuffix = "TestSuffix"
            }
        };
        private readonly DataSourceHubResponse _dataSourceResponse = new DataSourceHubResponse
        {
            DataSources = new List<DataSourceMetadata>
            {
                new DataSourceMetadata
                {
                    Name = "TestDataSource",
                    FileType = "csv",
                    Description = "TestDescription",
                    Authentication = new Dictionary<string, string>
                    {
                        { "connection_string_secret", "TestConnectionStringSecret" },
                        { "host", "TestHost" },
                        { "port", "123" },
                        { "database", "TestDatabase" },
                        { "username", "TestUsername" },
                        { "password_secret", "TestPasswordSecret" }
                    },
                }
            }
        };

        public DefaultAgentTests()
        {
            var sessionId = "TestSessionId";
            _promptHubService.ResolveRequest(_agentMetadata.Name!, sessionId).Returns(_promptResponse);
            _dataSourceHubService.ResolveRequest(Arg.Any<List<string>>(), sessionId).Returns(_dataSourceResponse);
        }

        [Fact]
        public async Task TestSearchService_CorrectlyConfiguresAgent()
        {
            // Arrange
            _agentMetadata.Type = "search-service";
            var agent = new DefaultAgent(_agentMetadata, _orchestrationService, _promptHubService, _dataSourceHubService);
            var sessionId = "TestSessionId";

            // Act
            await agent.Configure("TestSearchService", sessionId);

            // Assert
            Assert.NotNull(agent);
        }

        [Fact]
        public async Task TestBlobStorage_CorrectlyConfiguresAgent()
        {
            // Arrange
            _agentMetadata.Type = "blob-storage";
            var agent = new DefaultAgent(_agentMetadata, _orchestrationService, _promptHubService, _dataSourceHubService);
            var sessionId = "TestSessionId";

            // Act
            await agent.Configure("TestBlobStorage", sessionId);

            // Assert
            Assert.NotNull(agent);
        }

        [Fact]
        public async Task TestSQL_CorrectlyConfiguresAgent()
        {
            // Arrange
            _agentMetadata.Type = "sql";
            var agent = new DefaultAgent(_agentMetadata, _orchestrationService, _promptHubService, _dataSourceHubService);
            var sessionId = "TestSessionId";

            // Act
            await agent.Configure("TestSQL", sessionId);

            // Assert
            Assert.NotNull(agent);
        }

        [Fact]
        public async Task TestUnsupportedType_ThrowsArgumentException()
        {
            // Arrange
            _agentMetadata.Type = "UnsupportedType";
            var agent = new DefaultAgent(_agentMetadata, _orchestrationService, _promptHubService, _dataSourceHubService);
            var sessionId = "TestSessionId";

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await agent.Configure("TestUnsupportedType", sessionId));
        }

        [Fact]
        public async Task GetSummary_ValidSummaryRequest_CallsOrchestrationService()
        {
            // Arrange
            var agent = new DefaultAgent(_agentMetadata, _orchestrationService, _promptHubService, _dataSourceHubService);

            var summaryRequest = new SummaryRequest
            {
                SessionId = "TestSessionId",
                UserPrompt = "TestUserPrompt"
            };

            var summaryResult = "TestSummary";

            _orchestrationService.GetSummary(Arg.Any<LLMOrchestrationRequest>()).Returns(summaryResult);

            // Act
            var summaryResponse = await agent.GetSummary(summaryRequest);

            // Assert
            Assert.NotNull(summaryResponse);
            Assert.Equal(summaryResponse.Summary, summaryResult);
        }

        [Fact]
        public async Task GetCompletion_ReturnsExpectedResult()
        {
            // Arrange
            var completionRequest = new CompletionRequest
            {
                SessionId = "TestSessionId",
                UserPrompt = "TestPrompt",
                MessageHistory = new List<MessageHistoryItem> { }
            };

            var orchestrationService = Substitute.For<ILLMOrchestrationService>();
            var completionResponseTemplate = new LLMOrchestrationCompletionResponse
            {
                Completion = "TestCompletion",
                UserPrompt = "TestPrompt",
                PromptTemplate = "TestPromptTemplate",
                AgentName = "TestAgentName",
                PromptTokens = 200,
                CompletionTokens = 400
            };

            orchestrationService.GetCompletion(Arg.Any<LLMOrchestrationCompletionRequest>()).Returns(completionResponseTemplate);

            var completionRequestTemplateMock = Substitute.For<LLMOrchestrationCompletionRequest>();

            var agent = new DefaultAgent(_agentMetadata, orchestrationService, _promptHubService, _dataSourceHubService);
            var fieldInfo = agent.GetType().GetField("_completionRequestTemplate", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo?.SetValue(agent, completionRequestTemplateMock);

            // Act
            var result = await agent.GetCompletion(completionRequest);

            // Assert
            Assert.Equal("TestCompletion", result.Completion);
            Assert.Equal("TestPrompt", result.UserPrompt);
            Assert.Equal("TestPromptTemplate", result.PromptTemplate);
            Assert.Equal("TestAgentName", result.AgentName);
            Assert.Equal(200, result.PromptTokens);
            Assert.Equal(400, result.CompletionTokens);
        }
    }

}
