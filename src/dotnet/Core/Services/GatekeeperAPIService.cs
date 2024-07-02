using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Interfaces;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// Contains methods for interacting with the Gatekeeper API.
    /// </summary>
    public class GatekeeperAPIService : IGatekeeperAPIService
    {
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatekeeperAPIService"/> class.
        /// </summary>
        /// <param name="httpClientFactoryService">The <see cref="IHttpClientFactoryService"/>
        /// used to retrieve an <see cref="HttpClient"/> instance that contains required
        /// headers for Gateway API requests.</param>
        public GatekeeperAPIService(IHttpClientFactoryService httpClientFactoryService)
        {
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();
        }

        /// <inheritdoc/>
        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            // TODO: Call RefinementService to refine userPrompt
            // await _refinementService.RefineUserPrompt(completionRequest);

            var client = await _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperAPI);
                       
            var responseMessage = await client.PostAsync("orchestration/completion",
            new StringContent(
                    JsonSerializer.Serialize(completionRequest, _jsonSerializerOptions),
                    Encoding.UTF8, "application/json"));

            var defaultCompletionResponse = new CompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = completionRequest.UserPrompt ?? string.Empty,
                PromptTokens = 0,
                CompletionTokens = 0,
                UserPromptEmbedding = new float[] {0}
            };

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonSerializer.Deserialize<CompletionResponse>(responseContent);

                return completionResponse ?? defaultCompletionResponse;
            }

            return defaultCompletionResponse;
        }

        /// <inheritdoc/>
        public async Task<string> GetSummary(SummaryRequest summaryRequest)
        {
            // TODO: Call RefinementService to refine userPrompt
            // await _refinementService.RefineUserPrompt(content);

            var client = await _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperAPI);

            var responseMessage = await client.PostAsync("orchestration/summary",
                new StringContent(
                    JsonSerializer.Serialize(summaryRequest, _jsonSerializerOptions),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summarizeResponse = JsonSerializer.Deserialize<SummaryResponse>(responseContent);

                return summarizeResponse?.Summary ?? string.Empty;
            }
            else
                return "A problem on my side prevented me from responding.";
        }

        /// <inheritdoc/>
        public Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public Task RemoveMemory(object item) =>
            throw new NotImplementedException();
    }
}
