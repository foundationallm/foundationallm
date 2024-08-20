using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<LLMOrchestrationService> _logger;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        private readonly ICallContext _callContext;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// LLM Orchestration Service
        /// </summary>
        public LLMOrchestrationService(
            string serviceName,
            ILogger<LLMOrchestrationService> logger,
            IHttpClientFactoryService httpClientFactoryService,
            ICallContext callContext) 
        {
            _serviceName = serviceName;
            _logger = logger;
            _httpClientFactoryService = httpClientFactoryService;
            _callContext = callContext;
            _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();
            _jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetStatus(string instanceId)
        {
            var client = await _httpClientFactoryService.CreateClient(_serviceName, _callContext.CurrentUserIdentity);
            var responseMessage = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, $"/instances/{instanceId}/status"));

            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent)!;
        }

        /// <inheritdoc/>
        public string Name => _serviceName;

        /// <summary>
        /// Executes a completion request against the orchestration service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="request">Request object populated from the hub APIs including agent, prompt, data source, and model information.</param>
        /// <returns>Returns a completion response from the orchestration engine.</returns>
        public async Task<LLMCompletionResponse> GetCompletion(string instanceId, LLMCompletionRequest request)
        {
            var client = await _httpClientFactoryService.CreateClient(_serviceName, _callContext.CurrentUserIdentity);
            var pollingClient = new PollingHttpClient<LLMCompletionRequest, LLMCompletionResponse>(
                client,
                request,
                $"instances/{instanceId}/completions",
                TimeSpan.FromSeconds(10),
                client.Timeout.Subtract(TimeSpan.FromSeconds(1)),
                _logger);

            var completionResponse = await pollingClient.GetResponseAsync();

            if (completionResponse != null)
            {
                return new LLMCompletionResponse
                {
                    OperationId = request.OperationId,
                    Completion = completionResponse.Completion,
                    Citations = completionResponse.Citations,
                    UserPrompt = completionResponse.UserPrompt,
                    FullPrompt = completionResponse.FullPrompt,
                    PromptTemplate = string.Empty,
                    AgentName = request.Agent.Name,
                    PromptTokens = completionResponse.PromptTokens,
                    CompletionTokens = completionResponse.CompletionTokens
                };
            }

            _logger.LogWarning("The orchestration service was not able to return a response.");

            return new LLMCompletionResponse
            {
                OperationId = request.OperationId,
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
