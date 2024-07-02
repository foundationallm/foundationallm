using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.Integration;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Contains methods for interacting with the Gatekeeper API.
    /// </summary>
    public class GatekeeperIntegrationAPIService : IGatekeeperIntegrationAPIService
    {
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatekeeperIntegrationAPIService"/> class.
        /// </summary>
        /// <param name="httpClientFactoryService">The <see cref="IHttpClientFactoryService"/>
        /// used to retrieve an <see cref="HttpClient"/> instance that contains required
        /// headers for Gatekeeper Integration API requests.</param>
        public GatekeeperIntegrationAPIService(IHttpClientFactoryService httpClientFactoryService)
        {
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();
        }

        /// <inheritdoc/>
        public async Task<List<PIIResult>> AnalyzeText(string text)
        {
            var client = await _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperIntegrationAPI);

            var content = JsonSerializer.Serialize(new AnalyzeRequest() { Content = text, Anonymize = false, Language = "en" });

            var responseMessage = await client.PostAsync("analyze", new StringContent(content, Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var analysisResults = JsonSerializer.Deserialize<AnalyzeResponse>(responseContent);

                return analysisResults!.Results;
            }
            else
                return [];
        }

        /// <inheritdoc/>
        public async Task<string> AnonymizeText(string text)
        {
            var client = await _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperIntegrationAPI);

            var content = JsonSerializer.Serialize(new AnalyzeRequest() { Content = text, Anonymize = true, Language = "en" });

            var responseMessage = await client.PostAsync("analyze", new StringContent(content, Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var analysisResults = JsonSerializer.Deserialize<AnonymizeResponse>(responseContent);

                return analysisResults!.Content;
            }
            else
                return "A problem on my side prevented me from responding.";
        }
    }
}
