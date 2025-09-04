using Azure;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ContentSafety;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Models.ContentSafety;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ClientModel;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Implements the <see cref="IContentSafetyService"/> interface.
    /// </summary>
    public class AzureContentSafetyService : IContentSafetyService
    {
        private readonly IOrchestrationContext _callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        private readonly AzureContentSafetySettings _settings;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for the Azure Content Safety service.
        /// </summary>
        /// <param name="callContext">Stores context information extracted from the current HTTP request. This information
        /// is primarily used to inject HTTP headers into downstream HTTP calls.</param>
        /// <param name="httpClientFactoryService">The HTTP client factory service.</param>
        /// <param name="options">The configuration options for the Azure Content Safety service.</param>
        /// <param name="logger">The logger for the Azure Content Safety service.</param>
        public AzureContentSafetyService(
            IOrchestrationContext callContext,
            IHttpClientFactoryService httpClientFactoryService,
            IOptions<AzureContentSafetySettings> options,
            ILogger<AzureContentSafetyService> logger)
        {
            _callContext = callContext;
            _httpClientFactoryService = httpClientFactoryService;
            _settings = options.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<AnalyzeTextFilterResult> AnalyzeText(string content)
        {
            var client = await _httpClientFactoryService.CreateClient<IAzureAIContentSafetyClient>(
                _callContext.InstanceId!,
                HttpClientNames.AzureContentSafety,
                _callContext.CurrentUserIdentity!,
                AzureAIContentSafetyClient.BuildClient);

            AnalyzeTextResult? results = null;
            try
            {
                var clientResult = await client.AnalyzeText(new AnalyzeTextRequest
                {
                    Text = content
                });
                results = clientResult.Value;
            }
            catch (ClientResultException ex)
            {
                _logger.LogError(ex, "Azure AI Content Safety Analyze Text failed with status code: {StatusCode}, message: {Message}",
                    ex.Status, ex.Message);
                results = null;
            }

            if (results == null)
                return new AnalyzeTextFilterResult { Safe = false, Reason = "The content safety service was unable to validate the prompt text due to an internal error." };

            var safe = true;
            var reason = "The prompt text did not pass the content safety filter. Reason:";

            var hateSeverity = results.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Hate)?.Severity ?? 0;
            if (hateSeverity > _settings.HateSeverity)
            {
                reason += $" hate";
                safe = false;
            }

            var violenceSeverity = results.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Violence)?.Severity ?? 0;
            if (violenceSeverity > _settings.ViolenceSeverity)
            {
                reason += $" violence";
                safe = false;
            }

            var selfHarmSeverity = results.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.SelfHarm)?.Severity ?? 0;
            if (selfHarmSeverity > _settings.SelfHarmSeverity)
            {
                reason += $" self-harm";
                safe = false;
            }

            var sexualSeverity = results.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Sexual)?.Severity ?? 0;
            if (sexualSeverity > _settings.SexualSeverity)
            {
                reason += $" sexual";
                safe = false;
            }

            return new AnalyzeTextFilterResult() { Safe = safe, Reason = safe ? string.Empty : reason };
        }

        /// <inheritdoc/>
        public async Task<string?> DetectPromptInjection(string content)
        {
            var client = await _httpClientFactoryService.CreateClient<IAzureAIContentSafetyClient>(
                _callContext.InstanceId!,
                HttpClientNames.AzureContentSafety,
                _callContext.CurrentUserIdentity!,
                AzureAIContentSafetyClient.BuildClient);

            try
            {
                var clientResult = await client.ShieldPrompt(new ShieldPromptRequest
                {
                    UserPrompt = content,
                    Documents = []
                });

                if (clientResult.Value.UserPromptAnalysis.AttackDetected)
                {
                    return "The prompt text did not pass the safety filter. Reason: Prompt injection or jailbreak detected.";
                }
            }
            catch (ClientResultException ex)
            {
                _logger.LogError(ex, "Azure AI Content Safety Shield Prompt failed with status code: {StatusCode}, message: {Message}",
                    ex.Status, ex.Message);
                return null;
            }

            return null;
        }
    }
}
