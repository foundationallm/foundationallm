using FoundationaLLM.AgentFactory.Core.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Hubs;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Tests.Orchestration
{
    public class OrchestrationBaseTest
    {
        private readonly OrchestrationBase _orchestrationBase;
        private readonly AgentMetadata _agentMetadata = Substitute.For<AgentMetadata>();
        private readonly ILLMOrchestrationService _orchestrationService = Substitute.For<ILLMOrchestrationService>();
        private readonly IPromptHubAPIService _promptHubService = Substitute.For<IPromptHubAPIService>();
        private readonly IDataSourceHubAPIService _dataSourceHubService = Substitute.For<IDataSourceHubAPIService>();

        public OrchestrationBaseTest()
        {
            _orchestrationBase = new OrchestrationBase(_agentMetadata, _orchestrationService, _promptHubService, _dataSourceHubService);
        }

        [Fact]
        public async Task GetCompletion_ShouldReturnNullCompletionResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest(){ UserPrompt = ""};

            // Act
            var result = await _orchestrationBase.GetCompletion(completionRequest);

            // Assert
            Assert.Null(result);
        }
    }
}
