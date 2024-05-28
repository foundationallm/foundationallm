using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Implements the <see cref="IGatekeeperService"/> interface.
    /// </summary>
    /// <remarks>
    /// Constructor for the Gatekeeper service.
    /// </remarks>
    /// <param name="orchestrationAPIService">The Orchestration API client.</param>
    /// <param name="contentSafetyService">The user prompt Content Safety service.</param>
    /// <param name="lakeraGuardService">The Lakera Guard service.</param>
    /// <param name="enkryptGuardrailsService">The Enkrypt Guardrails service.</param>
    /// <param name="gatekeeperIntegrationAPIService">The Gatekeeper Integration API client.</param>
    /// <param name="gatekeeperServiceSettings">The configuration options for the Gatekeeper service.</param>
    public class GatekeeperService(
        IDownstreamAPIService orchestrationAPIService,
        IContentSafetyService contentSafetyService,
        ILakeraGuardService lakeraGuardService,
        IEnkryptGuardrailsService enkryptGuardrailsService,
        IGatekeeperIntegrationAPIService gatekeeperIntegrationAPIService,
        IOptions<GatekeeperServiceSettings> gatekeeperServiceSettings) : IGatekeeperService
    {
        private readonly IDownstreamAPIService _orchestrationAPIService = orchestrationAPIService;
        private readonly IContentSafetyService _contentSafetyService = contentSafetyService;
        private readonly ILakeraGuardService _lakeraGuardService = lakeraGuardService;
        private readonly IEnkryptGuardrailsService _enkryptGuardrailsService = enkryptGuardrailsService;
        private readonly IGatekeeperIntegrationAPIService _gatekeeperIntegrationAPIService = gatekeeperIntegrationAPIService;
        private readonly GatekeeperServiceSettings _gatekeeperServiceSettings = gatekeeperServiceSettings.Value;

        /// <summary>
        /// Gets a completion from the Gatekeeper service.
        /// </summary>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
        /// <returns>The completion response.</returns>
        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            if (completionRequest.GatekeeperOptions != null && completionRequest.GatekeeperOptions.Length > 0)
            {
                _gatekeeperServiceSettings.EnableAzureContentSafety = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.AzureContentSafety);
                _gatekeeperServiceSettings.EnableMicrosoftPresidio = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.MicrosoftPresidio);
                _gatekeeperServiceSettings.EnableLakeraGuard = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.LakeraGuard);
                _gatekeeperServiceSettings.EnableEnkryptGuardrails = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.EnkryptGuardrails);
                _gatekeeperServiceSettings.EnableAzureContentSafetyPromptShield = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.AzureContentSafetyPromptShield);
            }

            if (_gatekeeperServiceSettings.EnableLakeraGuard)
            {
                var promptInjectionResult = await _lakeraGuardService.DetectPromptInjection(completionRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new CompletionResponse() { Completion = promptInjectionResult };
            }

            if (_gatekeeperServiceSettings.EnableEnkryptGuardrails)
            {
                var promptInjectionResult = await _enkryptGuardrailsService.DetectPromptInjection(completionRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new CompletionResponse() { Completion = promptInjectionResult };
            }

            if (_gatekeeperServiceSettings.EnableAzureContentSafetyPromptShield)
            {
                var promptInjectionResult = await _contentSafetyService.DetectPromptInjection(completionRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new CompletionResponse() { Completion = promptInjectionResult };
            }

            if (_gatekeeperServiceSettings.EnableAzureContentSafety)
            {
                var contentSafetyResult = await _contentSafetyService.AnalyzeText(completionRequest.UserPrompt!);

                if (!contentSafetyResult.Safe)
                    return new CompletionResponse() { Completion = contentSafetyResult.Reason };
            }

            var completionResponse = await _orchestrationAPIService.GetCompletion(completionRequest);

            if (_gatekeeperServiceSettings.EnableMicrosoftPresidio)
                completionResponse.Completion = await _gatekeeperIntegrationAPIService.AnonymizeText(completionResponse.Completion);

            return completionResponse;
        }

        /// <summary>
        /// Gets a summary from the Gatekeeper service.
        /// </summary>
        /// <param name="summaryRequest">The summarize request containing the user prompt.</param>
        /// <returns>The summary response.</returns>
        public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            if (_gatekeeperServiceSettings.EnableLakeraGuard)
            {
                var promptinjectionResult = await _lakeraGuardService.DetectPromptInjection(summaryRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptinjectionResult))
                    return new SummaryResponse() { Summary = promptinjectionResult };
            }

            if (_gatekeeperServiceSettings.EnableEnkryptGuardrails)
            {
                var promptInjectionResult = await _enkryptGuardrailsService.DetectPromptInjection(summaryRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new SummaryResponse() { Summary = promptInjectionResult };
            }

            if (_gatekeeperServiceSettings.EnableAzureContentSafetyPromptShield)
            {
                var promptInjectionResult = await _contentSafetyService.DetectPromptInjection(summaryRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new SummaryResponse() { Summary = promptInjectionResult };
            }

            if (_gatekeeperServiceSettings.EnableAzureContentSafety)
            {
                var contentSafetyResult = await _contentSafetyService.AnalyzeText(summaryRequest.UserPrompt!);

                if (!contentSafetyResult.Safe)
                    return new SummaryResponse() { Summary = contentSafetyResult.Reason };
            }

            var summaryResponse = await _orchestrationAPIService.GetSummary(summaryRequest);

            if (_gatekeeperServiceSettings.EnableMicrosoftPresidio)
                summaryResponse.Summary = await _gatekeeperIntegrationAPIService.AnonymizeText(summaryResponse.Summary!);

            return summaryResponse;
        }
    }
}
