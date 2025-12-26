using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
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
        private readonly IOrchestrationContext _callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatekeeperAPIService"/> class.
        /// </summary>
        /// <param name="callContext">Stores context information extracted from the current HTTP request. This information
        /// is primarily used to inject HTTP headers into downstream HTTP calls.</param>
        /// <param name="httpClientFactoryService">The <see cref="IHttpClientFactoryService"/>
        /// used to retrieve an <see cref="HttpClient"/> instance that contains required
        /// headers for Gateway API requests.</param>
        public GatekeeperAPIService(
            IOrchestrationContext callContext,
            IHttpClientFactoryService httpClientFactoryService)
        {
            _callContext = callContext;
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();
        }

        /// <inheritdoc/>
        public async Task<CompletionResponse> GetCompletion(string instanceId, CompletionRequest completionRequest)
        {
            // TODO: Call RefinementService to refine userPrompt
            // await _refinementService.RefineUserPrompt(completionRequest);

            var client = await _httpClientFactoryService.CreateClient(
                instanceId,
                Common.Constants.HttpClientNames.GatekeeperAPI,
                _callContext.CurrentUserIdentity!);
                       
            var responseMessage = await client.PostAsync($"instances/{instanceId}/completions",
            new StringContent(
                    JsonSerializer.Serialize(completionRequest, _jsonSerializerOptions),
                    Encoding.UTF8, "application/json"));

            var defaultCompletionResponse = new CompletionResponse
            {
                OperationId = completionRequest.OperationId ?? string.Empty,
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
        public Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public Task RemoveMemory(object item) =>
            throw new NotImplementedException();
    }
}
