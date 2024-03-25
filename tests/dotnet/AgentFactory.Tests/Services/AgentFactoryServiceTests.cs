using FoundationaLLM.AgentFactory.Core.Services;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.Extensions.Configuration;

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
        private IEnumerable<IResourceProviderService> _resourceProviderServices = new List<IResourceProviderService>
        {
            Substitute.For<IResourceProviderService>()
        };
        private ICallContext _callContext = Substitute.For<ICallContext>();
        private ILoggerFactory _loggerFactory =  Substitute.For<ILoggerFactory>();
        private IConfiguration _configuration = Substitute.For<IConfiguration>();


        public AgentFactoryServiceTests()
        {
            _agentFactoryService = new AgentFactoryService(
                _resourceProviderServices,
                _orchestrationServices,
                _callContext,
                _configuration,
                _agentHubAPIService,
                _promptHubAPIService,
                _dataSourceHubAPIService,
                _loggerFactory
            );
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
            var result = await _agentFactoryService.GetCompletion(completionRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(completionRequest.UserPrompt, result.UserPrompt);
        }

        [Fact]
        public async Task GetCompletion_ExceptionThrown_ReturnsErrorResponse()
        {
            // Act 
            var result = await _agentFactoryService.GetCompletion(new CompletionRequest() { UserPrompt = "Error" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("A problem on my side prevented me from responding.", result.Completion);
        }
    }
}
