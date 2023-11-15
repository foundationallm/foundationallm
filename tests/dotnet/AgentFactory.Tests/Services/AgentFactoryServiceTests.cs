using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Services;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Tests.Services
{
    public class AgentFactoryServiceTests
    {
        private readonly IEnumerable<ILLMOrchestrationService> _orchestrationServices = new List<ILLMOrchestrationService>
        {
            Substitute.For<ILLMOrchestrationService>(),
            Substitute.For<ILLMOrchestrationService>()
        };
        private readonly IAgentHubAPIService _agentHubAPIService = Substitute.For<IAgentHubAPIService>();
        private readonly IPromptHubAPIService _promptHubAPIService = Substitute.For<IPromptHubAPIService>();
        private readonly IDataSourceHubAPIService _dataSourceHubAPIService = Substitute.For<IDataSourceHubAPIService>();
        private readonly ILogger<AgentFactoryService> _logger = Substitute.For<ILogger<AgentFactoryService>>();
        private readonly AgentFactoryService _agentFactoryService;
        public AgentFactoryServiceTests()
        {
            _agentFactoryService = new AgentFactoryService(
                _orchestrationServices,
                _agentHubAPIService,
                _promptHubAPIService,
                _dataSourceHubAPIService,
                _logger
            );
        }

        [Fact]
        public void Status_AllOrchestrationServicesInitialized_ReturnsReady()
        {
            // Arrange
            _orchestrationServices.ToList().ForEach(os => os.IsInitialized.Returns(true));
            
            // Assert
            Assert.Equal("ready", _agentFactoryService.Status);
        }

        [Fact]
        public async Task GetCompletion_ValidCompletionRequest_ReturnsCompletionResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest
            {
                UserPrompt = "TestPrompt"
            };

            // Act
            var completionResponse = await _agentFactoryService.GetCompletion(completionRequest);

            // Assert
            Assert.NotNull(completionResponse);
        }

        [Fact]
        public async Task GetSummary_ValidSummaryRequest_ReturnsSummaryResponse()
        {
            // Arrange
            var summaryRequest = new SummaryRequest
            {
                UserPrompt = "TestPrompt"
            };

            // Act
            var summaryResponse = await _agentFactoryService.GetSummary(summaryRequest);

            // Assert
            Assert.NotNull(summaryResponse);
        }
    }
}
