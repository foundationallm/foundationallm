using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Contains methods for interacting with the Gatekeeper API.
    /// </summary>
    public class GatekeeperIntegrationAPIService : IGatekeeperIntegrationAPIService
    {
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatekeeperIntegrationAPIService"/> class.
        /// </summary>
        /// <param name="httpClientFactoryService">The <see cref="IHttpClientFactoryService"/>
        /// used to retrieve an <see cref="HttpClient"/> instance that contains required
        /// headers for Gatekeeper Integration API requests.</param>
        public GatekeeperIntegrationAPIService(IHttpClientFactoryService httpClientFactoryService)
        {
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        /// <inheritdoc/>
        public async Task<List<string>> AnalyzeText(string text)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperIntegrationAPI);

            var responseMessage = await client.PostAsync("analyze",
            new StringContent(text, Encoding.UTF8, "application/text"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var analysisResults = JsonConvert.DeserializeObject<List<string>>(responseContent);

                return analysisResults ?? [];
            }
            else
                return [];
        }

        /// <inheritdoc/>
        public async Task<string> AnonymizeText(string text)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperIntegrationAPI);

            var responseMessage = await client.PostAsync("anonymize",
            new StringContent(text, Encoding.UTF8, "application/text"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var anonymizedText = await responseMessage.Content.ReadAsStringAsync();

                return anonymizedText;
            }
            else
                return "A problem on my side prevented me from responding.";
        }
    }
}
