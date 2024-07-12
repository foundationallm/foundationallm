using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Models.LakeraGuard;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Implements the <see cref="ILakeraGuardService"/> interface.
    /// </summary>
    public class LakeraGuardService : ILakeraGuardService
    {
        private readonly ICallContext _callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        private readonly LakeraGuardServiceSettings _settings;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for the Azure Content Safety service.
        /// </summary>
        /// <param name="callContext">Stores context information extracted from the current HTTP request. This information
        /// is primarily used to inject HTTP headers into downstream HTTP calls.</param>
        /// <param name="httpClientFactoryService">The HTTP client factory service.</param>
        /// <param name="options">The configuration options for the Azure Content Safety service.</param>
        /// <param name="logger">The logger for the Azure Content Safety service.</param>
        public LakeraGuardService(
            ICallContext callContext,
            IHttpClientFactoryService httpClientFactoryService,
            IOptions<LakeraGuardServiceSettings> options,
            ILogger<LakeraGuardService> logger)
        {
            _callContext = callContext;
            _httpClientFactoryService = httpClientFactoryService;
            _settings = options.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string?> DetectPromptInjection(string content)
        {
            var client = await _httpClientFactoryService.CreateClient(HttpClients.LakeraGuard, _callContext.CurrentUserIdentity);

            var response = await client.PostAsync("prompt_injection",
                new StringContent(JsonSerializer.Serialize(new { input = content }), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<AnalyzePromptInjectionResult>(responseContent);
                var promptinjectionResult = results!.Results.FirstOrDefault();

                if (promptinjectionResult != null && promptinjectionResult.Flagged)
                {
                    if (promptinjectionResult.Categories["prompt_injection"])
                        return "The prompt text did not pass the safety filter. Reason: Prompt injection detected.";

                    if (promptinjectionResult.Categories["jailbreak"])
                        return "The prompt text did not pass the safety filter. Reason: Prompt jailbreak detected.";
                }
            }

            return null;
        }
    }
}
