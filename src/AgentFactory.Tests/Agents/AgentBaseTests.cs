using FoundationaLLM.AgentFactory.Core.Agents;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Tests.Agents
{
    public class AgentBaseTests
    {
        private readonly AgentBase _agentBase;
        private readonly AgentMetadata _agentMetadata = Substitute.For<AgentMetadata>();
        private readonly ILLMOrchestrationService _orchestrationService = Substitute.For<ILLMOrchestrationService>();
        private readonly IPromptHubAPIService _promptHubService = Substitute.For<IPromptHubAPIService>();
        private readonly IDataSourceHubAPIService _dataSourceHubService = Substitute.For<IDataSourceHubAPIService>();

        public AgentBaseTests()
        {
            _agentBase = new AgentBase(_agentMetadata,_orchestrationService,_promptHubService,_dataSourceHubService);
        }

        [Fact]
        public async Task GetCompletion_ShouldReturnNullCompletionResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest();

            // Act
            var result = await _agentBase.GetCompletion(completionRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSummary_ShouldReturnNullSummaryResponse()
        {
            // Arrange
            var summaryRequest = new SummaryRequest();

            // Act
            var result = await _agentBase.GetSummary(summaryRequest);

            // Assert
            Assert.Null(result);
        }
    }
}
