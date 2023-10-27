using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ContentSafety;
using FoundationaLLM.Gatekeeper.Core.Services;
using NSubstitute;

namespace Gatekeeper.Tests.Services
{
    public class GatekeeperServiceTests
    {
        private readonly GatekeeperService _gatekeeperService;

        private readonly IContentSafetyService _contentSafetyService = Substitute.For<IContentSafetyService>();
        private readonly IAgentFactoryAPIService _agentFactoryAPIService = Substitute.For<IAgentFactoryAPIService>();
        private readonly IRefinementService _refinementService = Substitute.For<IRefinementService>();

        public GatekeeperServiceTests()
        {
            _gatekeeperService = new GatekeeperService(_agentFactoryAPIService, _refinementService, _contentSafetyService);
        }

        [Fact]
        public async Task GetCompletion_CallsAgentFactoryAPIServiceWithCompletionRequest()
        {
            // Arrange
            var completionRequest = new CompletionRequest
            {
                UserPrompt = "Safe content."
            };

            var expectedResult = new CompletionResponse { Completion = "Completion from Agent Factory API Service." };

            var safeContentResult = new AnalyzeTextFilterResult { Reason = "Safe", Safe = true };
            _contentSafetyService.AnalyzeText(completionRequest.UserPrompt).Returns(safeContentResult);
            _agentFactoryAPIService.GetCompletion(completionRequest).Returns(expectedResult);

            // Act
            var result = await _gatekeeperService.GetCompletion(completionRequest);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetSummary_CallsAgentFactoryAPIServiceWithSummaryRequest()
        {
            // Arrange
            var summaryRequest = new SummaryRequest
            {
                UserPrompt = "Safe content for summary."
            };

            var expectedResult = new SummaryResponse { Summary = "Summary from Agent Factory API Service." };

            var safeContentResult = new AnalyzeTextFilterResult { Reason="Safe", Safe = true };

            _contentSafetyService.AnalyzeText(summaryRequest.UserPrompt).Returns(safeContentResult);
            _agentFactoryAPIService.GetSummary(summaryRequest).Returns(expectedResult);

            // Act
            var result = await _gatekeeperService.GetSummary(summaryRequest);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
