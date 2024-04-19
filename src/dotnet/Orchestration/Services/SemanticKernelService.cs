using System.Text;
using System.Text.Json;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// The FoundationaLLM Semantic Kernal Service
    /// </summary>
    /// <remarks>
    /// Constructor for the Semantic Kernal Service
    /// </remarks>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="httpClientFactoryService"></param>
    public class SemanticKernelService(
        IOptions<SemanticKernelServiceSettings> options,
        ILogger<SemanticKernelService> logger,
        IHttpClientFactoryService httpClientFactoryService) : ISemanticKernelService
    {
        readonly SemanticKernelServiceSettings _settings = options.Value;
        readonly ILogger<SemanticKernelService> _logger = logger;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        /// <summary>
        /// Checks the Semantic Service returns a call to signal it is initialized and ready for requests.
        /// </summary>
        public bool IsInitialized => GetServiceStatus();

        /// <summary>
        /// Gets a completion from the Semantic Kernel service.
        /// </summary>
        /// <param name="request">Request object populated from the hub APIs including agent, prompt, data source, and model information.</param>
        /// <returns>Returns a completion response from the orchestration engine.</returns>
        public async Task<LLMCompletionResponse> GetCompletion(LLMCompletionRequest request)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);

            var body = JsonSerializer.Serialize(request, _jsonSerializerOptions);
            var responseMessage = await client.PostAsync("orchestration/completion",
                new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                var completionResponse = JsonSerializer.Deserialize<LLMCompletionResponse>(responseContent);

                return new LLMCompletionResponse
                {
                    Completion = completionResponse!.Completion,
                    UserPrompt = completionResponse.UserPrompt,
                    FullPrompt = completionResponse.FullPrompt,
                    PromptTemplate = string.Empty,
                    AgentName = request.Agent.Name,
                    PromptTokens = completionResponse.PromptTokens,
                    CompletionTokens = completionResponse.CompletionTokens
                };
            }

            _logger.LogWarning("The LangChain orchestration service returned status code {StatusCode}: {ResponseContent}",
                responseMessage.StatusCode, responseContent);

            return new LLMCompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = request.UserPrompt,
                PromptTemplate = string.Empty,
                AgentName = request.Agent.Name,
                PromptTokens = 0,
                CompletionTokens = 0
            };
        }

        /// <summary>
        /// Gets the target Semantic Kernel API status.
        /// </summary>
        /// <returns></returns>
        private bool GetServiceStatus()
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "status"));

            return responseMessage.Content.ToString() == "ready";
        }
    }
}
