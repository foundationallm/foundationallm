using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Models.EnkryptGuardrails;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Implements the <see cref="IEnkryptGuardrailsService"/> interface.
    /// </summary>
    public class EnkryptGuardrailsService : IEnkryptGuardrailsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EnkryptGuardrailsServiceSettings _settings;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for the Azure Content Safety service.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="options">The configuration options for the Azure Content Safety service.</param>
        /// <param name="logger">The logger for the Azure Content Safety service.</param>
        public EnkryptGuardrailsService(
            IHttpClientFactory httpClientFactory,
            IOptions<EnkryptGuardrailsServiceSettings> options,
            ILogger<EnkryptGuardrailsService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string?> DetectPromptInjection(string content)
        {
            var client = CreateHttpClient();

            var response = await client.PostAsync("/api/guardrails/detect",
                new StringContent(JsonSerializer.Serialize(new
                {
                    text = content,
                    detectors = new
                    {
                        injection_attack = new
                        {
                            enabled = true
                        }
                    }
                }),
                Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<DetectResult>(responseContent);
                var promptinjectionResult = results!.Summary.InjectionAttack;

                if (promptinjectionResult != 0)
                {
                    return "The prompt text did not pass the safety filter. Reason: Prompt injection or jailbreak detected.";
                }
            }

            return null;
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient();

            httpClient.BaseAddress = new Uri(_settings.APIUrl);
            httpClient.DefaultRequestHeaders.Add("api_key", _settings.APIKey);

            return httpClient;
        }
    }
}
