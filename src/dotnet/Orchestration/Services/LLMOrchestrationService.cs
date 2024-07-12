using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// Provides methods to call an external LLM orchestration service.
    /// </summary>
    public class LLMOrchestrationService : ILLMOrchestrationService
    {
        private readonly string _serviceName;
        private readonly APISettingsBase _settings;
        private readonly ILogger<LLMOrchestrationService> _logger;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        private readonly ICallContext _callContext;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// LLM Orchestration Service
        /// </summary>
        public LLMOrchestrationService(
            string serviceName,
            IOptions<APISettingsBase> options,
            ILogger<LLMOrchestrationService> logger,
            IHttpClientFactoryService httpClientFactoryService,
            ICallContext callContext) 
        {
            _serviceName = serviceName;
            _settings = options.Value;
            _logger = logger;
            _httpClientFactoryService = httpClientFactoryService;
            _callContext = callContext;
            _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();
            _jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetStatus()
        {
            var client = await _httpClientFactoryService.CreateClient(_serviceName, _callContext.CurrentUserIdentity);
            var responseMessage = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, "status"));

            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent)!;
        }

        /// <inheritdoc/>
        public string Name => _serviceName;

        /// <summary>
        /// Executes a completion request against the orchestration service.
        /// </summary>
        /// <param name="request">Request object populated from the hub APIs including agent, prompt, data source, and model information.</param>
        /// <returns>Returns a completion response from the orchestration engine.</returns>
        public async Task<LLMCompletionResponse> GetCompletion(LLMCompletionRequest request)
        {
            var client = await _httpClientFactoryService.CreateClient(_serviceName, _callContext.CurrentUserIdentity);

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
                    Citations = completionResponse.Citations,
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
    }
}
