using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Implements the <see cref="IGatekeeperService"/> interface.
    /// </summary>
    /// <remarks>
    /// Constructor for the Gatekeeper service.
    /// </remarks>
    /// <param name="agentFactoryAPIService">The Agent Factory API client.</param>
    /// <param name="contentSafetyService">The user prompt Content Safety service.</param>
    /// <param name="gatekeeperIntegrationAPIService">The Gatekeeper Integration API client.</param>
    /// <param name="gatekeeperServiceSettings">The configuration options for the Gatekeeper service.</param>
    /// <param name="resourceProviderServices">The list of resurce providers registered with the main dependency injection container.</param>
    /// <param name="callContext">The call context of the request being handled.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class GatekeeperService(
        IDownstreamAPIService agentFactoryAPIService,
        IContentSafetyService contentSafetyService,
        IGatekeeperIntegrationAPIService gatekeeperIntegrationAPIService,
        IOptions<GatekeeperServiceSettings> gatekeeperServiceSettings,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ICallContext callContext,
        ILogger<GatekeeperService> logger) : IGatekeeperService
    {
        private readonly IDownstreamAPIService _agentFactoryAPIService = agentFactoryAPIService;
        private readonly IContentSafetyService _contentSafetyService = contentSafetyService;
        private readonly IGatekeeperIntegrationAPIService _gatekeeperIntegrationAPIService = gatekeeperIntegrationAPIService;
        private readonly GatekeeperServiceSettings _gatekeeperServiceSettings = gatekeeperServiceSettings.Value;
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
        private readonly ICallContext _callContext = callContext;
        private readonly ILogger<GatekeeperService> _logger = logger;

        /// <summary>
        /// Gets a completion from the Gatekeeper service.
        /// </summary>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
        /// <returns>The completion response.</returns>
        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            //TODO: Call the Refinement Service with the userPrompt
            //await _refinementService.RefineUserPrompt(completionRequest.Prompt);

            var settings = await GetGatekeeperServiceSettingsForAgent(completionRequest.AgentName);

            if (settings.EnableAzureContentSafety)
            {
                var contentSafetyResult = await _contentSafetyService.AnalyzeText(completionRequest.UserPrompt!);

                if (!contentSafetyResult.Safe)
                    return new CompletionResponse() { Completion = contentSafetyResult.Reason };
            }

            var completionResponse = await _agentFactoryAPIService.GetCompletion(completionRequest);

            if (settings.EnableMicrosoftPresidio)
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
            //TODO: Call the Refinement Service with the userPrompt
            //await _refinementService.RefineUserPrompt(summaryRequest.Prompt);

            var settings = await GetGatekeeperServiceSettingsForAgent(summaryRequest.AgentName);

            if (settings.EnableAzureContentSafety)
            {
                var contentSafetyResult = await _contentSafetyService.AnalyzeText(summaryRequest.UserPrompt!);

                if (!contentSafetyResult.Safe)
                    return new SummaryResponse() { Summary = contentSafetyResult.Reason };
            }

            var summaryResponse = await _agentFactoryAPIService.GetSummary(summaryRequest);

            if (settings.EnableMicrosoftPresidio)
                summaryResponse.Summary = await _gatekeeperIntegrationAPIService.AnonymizeText(summaryResponse.Summary!);

            return summaryResponse;
        }

        private async Task<GatekeeperServiceSettings> GetGatekeeperServiceSettingsForAgent(string agentName)
        {
            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");

            AgentBase? agentBase = default;

            if (!string.IsNullOrWhiteSpace(agentName))
            {
                try
                {
                    var agents = await agentResourceProvider.HandleGetAsync($"/{AgentResourceTypeNames.Agents}/{agentName}", _callContext.CurrentUserIdentity);
                    agentBase = ((List<AgentBase>)agents)[0];
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "The agent resource provider failed to properly retrieve the agent: /{Agents}/{AgentName}",
                        AgentResourceTypeNames.Agents, agentName);
                }
            }

            var enableAzureContentSafety = _gatekeeperServiceSettings.EnableAzureContentSafety;
            var enableMicrosoftPresidio = _gatekeeperServiceSettings.EnableMicrosoftPresidio;

            agentBase = new AgentBase() { Name = "FoundationaLLM", Gatekeeper = new Common.Models.Agents.Gatekeeper { UseSystemSetting = false, Options = ["Presidio"] } };

            if (agentBase != null)
            {
                if (agentBase.Gatekeeper?.UseSystemSetting == false)
                {
                    enableAzureContentSafety = agentBase.Gatekeeper.Options != null && agentBase.Gatekeeper.Options.Any(x => x == GatekeeperOptionNames.AzureContentSafety);
                    enableMicrosoftPresidio = agentBase.Gatekeeper.Options != null && agentBase.Gatekeeper.Options.Any(x => x == GatekeeperOptionNames.MicrosoftPresidio);
                }
            }

            return new GatekeeperServiceSettings
            {
                EnableAzureContentSafety = enableAzureContentSafety,
                EnableMicrosoftPresidio = enableMicrosoftPresidio
            };
        }
    }
}
