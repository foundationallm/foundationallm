using FoundationaLLM.AgentFactory.Core.Agents;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.Orchestration;
using System.Reflection;

namespace FoundationaLLM.AgentFactory.Tests.Agents
{
    public class AgentBuilderTests
    {
        private string userPrompt = "TestPrompt";
        private readonly IAgentHubAPIService _agentHubAPIService = Substitute.For<IAgentHubAPIService>();
        private readonly ILangChainService _langChainService = Substitute.For<ILangChainService>();
        private readonly ISemanticKernelService _semanticKernelService = Substitute.For<ISemanticKernelService>();
        private readonly IEnumerable<ILLMOrchestrationService> _orchestrationServices;
        private readonly IPromptHubAPIService _promptHubAPIService = Substitute.For<IPromptHubAPIService>();
        private readonly IDataSourceHubAPIService _dataSourceHubAPIService = Substitute.For<IDataSourceHubAPIService>();

        public AgentBuilderTests()
        {
            _orchestrationServices = new List<ILLMOrchestrationService>
            {
               _langChainService,
               _semanticKernelService
            };
        }

        [Fact]
        public async Task Build_WithValidParameters_ReturnsAgent()
        {
            // Arrange
            var agentResponse = new AgentHubResponse
            {
                Agent = new AgentMetadata { 
                    Orchestrator = "LangChain", 
                    Type = "search-service", 
                    LanguageModel = new LanguageModelMetadata { }, 
                    AllowedDataSourceNames = new List<string> { "DataSource_Test" } 
                }
            };
            var sessionId = "TestSessionId";

            _agentHubAPIService.ResolveRequest(userPrompt, sessionId).Returns(agentResponse);

            // Configure DataSourceHubAPIService
            _dataSourceHubAPIService.ResolveRequest(Arg.Any<List<string>>(), sessionId).Returns(new DataSourceHubResponse
            {
                DataSources = new List<DataSourceMetadata>
                    {
                        new DataSourceMetadata
                        {
                            Name = userPrompt
                        }
                    }
            });

            // Configure PromptHubAPIService
            var promptResponse = new PromptHubResponse
            {
                Prompt = new PromptMetadata
                {
                    PromptPrefix = "TestPrefix",
                    PromptSuffix = "TestSuffix"
                },
            };
            _promptHubAPIService.ResolveRequest(Arg.Any<string>(), sessionId, Arg.Any<string>()).Returns(promptResponse);

            // Act
            var agent = await AgentBuilder.Build(
                userPrompt,
                sessionId,
                _agentHubAPIService,
                _orchestrationServices,
                _promptHubAPIService,
                _dataSourceHubAPIService);

            // Assert
            Assert.NotNull(agent);
        }

        [Fact]
        public void Build_WithInvalidOrchestrationType_ThrowsArgumentException()
        {
            // Arrange
            var agentResponse = new AgentHubResponse
            {
                Agent = new AgentMetadata { Orchestrator = "InvalidOrchestrator" }
            };
            var sessionId = "TestSessionId";

            _agentHubAPIService.ResolveRequest(userPrompt, sessionId).Returns(agentResponse);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await AgentBuilder.Build(
                    userPrompt,
                    sessionId,
                    _agentHubAPIService,
                    _orchestrationServices,
                    _promptHubAPIService,
                    _dataSourceHubAPIService));
        }

        [Fact]
        public void SelectLangChainOrchestrationService_ValidOrchestrationType_ReturnsService()
        {
            // Act
            var result = InvokeSelectOrchestrationService(LLMOrchestrationService.LangChain, _orchestrationServices);

            // Assert
            Assert.Equal(_langChainService, result);
        }

        [Fact]
        public void SelectSemanticKernelOrchestrationService_ValidOrchestrationType_ReturnsService()
        {
            // Act
            var result = InvokeSelectOrchestrationService(LLMOrchestrationService.SemanticKernel, _orchestrationServices);

            // Assert
            Assert.Equal(_semanticKernelService, result);
        }

        private ILLMOrchestrationService InvokeSelectOrchestrationService(LLMOrchestrationService orchestrationType, IEnumerable<ILLMOrchestrationService> orchestrationServices)
        {
            var methodInfo = typeof(AgentBuilder).GetMethod("SelectOrchestrationService", BindingFlags.NonPublic | BindingFlags.Static);
            return (ILLMOrchestrationService)methodInfo?.Invoke(null, new object[] { orchestrationType, orchestrationServices })!;
        }
    }
}
