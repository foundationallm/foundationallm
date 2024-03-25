using FoundationaLLM.Agent.Constants;
using FoundationaLLM.AgentFactory.Core.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Hubs;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace FoundationaLLM.AgentFactory.Tests.Orchestration
{
    public class OrchestrationBuilderTests
    {
        private string userPrompt = "TestPrompt";
        private readonly IAgentHubAPIService _agentHubAPIService = Substitute.For<IAgentHubAPIService>();
        private readonly ILangChainService _langChainService = Substitute.For<ILangChainService>();
        private readonly ICallContext _callContext = Substitute.For<ICallContext>();
        private Dictionary<string, IResourceProviderService> _resourceProviderServices = new Dictionary<string, IResourceProviderService>();
        private readonly ISemanticKernelService _semanticKernelService = Substitute.For<ISemanticKernelService>();
        private readonly IEnumerable<ILLMOrchestrationService> _orchestrationServices;
        private readonly IPromptHubAPIService _promptHubAPIService = Substitute.For<IPromptHubAPIService>();
        private readonly IDataSourceHubAPIService _dataSourceHubAPIService = Substitute.For<IDataSourceHubAPIService>();
        private readonly IConfiguration _configuration = Substitute.For<IConfiguration>();
        private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();

        public OrchestrationBuilderTests()
        {
            _orchestrationServices = new List<ILLMOrchestrationService>
            {
               _langChainService,
               _semanticKernelService
            };
        }

        [Fact]
        public async Task Build_AgentHintNotNull_KnowledgeManagementAgent()
        {
            // Arrange
            var completionRequest = new CompletionRequest()
            {
                UserPrompt = "Test_Userprompt",
                AgentName = "knowledge-management"
            };

            var agentResourceProvider = Substitute.For<IResourceProviderService>();
            var knowledgeManagementAgent = new KnowledgeManagementAgent() { Name = "knowledge-management", ObjectId = "Test_objectid", Type = AgentTypes.KnowledgeManagement };
            var agentList = new List<AgentBase> { knowledgeManagementAgent };
            agentResourceProvider.HandleGetAsync($"/{AgentResourceTypeNames.Agents}/{completionRequest.AgentName}", _callContext.CurrentUserIdentity).Returns(agentList);

            _resourceProviderServices.Add(ResourceProviderNames.FoundationaLLM_Agent, agentResourceProvider);

            // Act
            var result = await OrchestrationBuilder.Build(
                completionRequest,
                _callContext,
                _configuration,
                _resourceProviderServices,
                _agentHubAPIService,
                _orchestrationServices,
                _promptHubAPIService,
                _dataSourceHubAPIService,
                _loggerFactory);

            // Assert
            Assert.NotNull(result);
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
                await OrchestrationBuilder.Build(
                new CompletionRequest() { UserPrompt = userPrompt },
                _callContext,
                _configuration,
                _resourceProviderServices,
                _agentHubAPIService,
                _orchestrationServices,
                _promptHubAPIService,
                _dataSourceHubAPIService,
                _loggerFactory));
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
            var methodInfo = typeof(OrchestrationBuilder).GetMethod("SelectOrchestrationService", BindingFlags.NonPublic | BindingFlags.Static);
            return (ILLMOrchestrationService)methodInfo?.Invoke(null, new object[] { orchestrationType, orchestrationServices })!;
        }
    }
}
