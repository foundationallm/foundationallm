using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Models.ContentSafety;
using FoundationaLLM.Gatekeeper.Core.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Gatekeeper.Tests.Services
{
    public class GatekeeperServiceTests
    {
        private readonly GatekeeperService _testedService;

        private readonly IContentSafetyService _contentSafetyService = Substitute.For<IContentSafetyService>();
        private readonly ILakeraGuardService _lakeraGuardService = Substitute.For<ILakeraGuardService>();
        private readonly IDownstreamAPIService _orchestrationAPIService = Substitute.For<IDownstreamAPIService>();
        private readonly IGatekeeperIntegrationAPIService _gatekeeperIntegrationAPIService = Substitute.For<IGatekeeperIntegrationAPIService>();
        private IOptions<GatekeeperServiceSettings> _gatekeeperServiceSettings;

        public GatekeeperServiceTests()
        {
            _gatekeeperServiceSettings = Options.Create(new GatekeeperServiceSettings
            {
                EnableAzureContentSafety = true,
                EnableMicrosoftPresidio = true,
                EnableLakeraGuard = true,
            });

            _testedService = new GatekeeperService(
                _orchestrationAPIService,
                _contentSafetyService,
                _lakeraGuardService,
                _gatekeeperIntegrationAPIService,
                _gatekeeperServiceSettings);
        }

        [Fact]
        public async Task GetCompletion_CallsOrchestrationAPIServiceWithCompletionRequest()
        {
            // Arrange
            var completionRequest = new CompletionRequest
            {
                UserPrompt = "Safe content."
            };

            var expectedResult = new CompletionResponse { Completion = "Completion from Orchestration API Service." };

            var safeContentResult = new AnalyzeTextFilterResult { Safe = true, Reason = string.Empty };
            _contentSafetyService.AnalyzeText(completionRequest.UserPrompt).Returns(safeContentResult);
            _orchestrationAPIService.GetCompletion(completionRequest).Returns(expectedResult);

            // Act
            var actualResult = await _testedService.GetCompletion(completionRequest);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async Task GetSummary_CallsOrchestrationAPIServiceWithSummaryRequest()
        {
            // Arrange
            var summaryRequest = new SummaryRequest
            {
                UserPrompt = "Safe content for summary."
            };

            var expectedResult = new SummaryResponse { Summary = "Summary from Orchestration API Service." };

            var safeContentResult = new AnalyzeTextFilterResult { Safe = true, Reason = string.Empty };

            _contentSafetyService.AnalyzeText(summaryRequest.UserPrompt).Returns(safeContentResult);
            _orchestrationAPIService.GetSummary(summaryRequest).Returns(expectedResult);

            // Act
            var actualResult = await _testedService.GetSummary(summaryRequest);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
