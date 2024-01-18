﻿using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Services
{
    /// <summary>
    /// The LangChain orchestration service.
    /// </summary>
    public class LangChainService : ILangChainService
    {
        readonly LangChainServiceSettings _settings;
        readonly ILogger<LangChainService> _logger;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// LangChain Orchestration Service
        /// </summary>
        public LangChainService(
            IOptions<LangChainServiceSettings> options,
            ILogger<LangChainService> logger,
            IHttpClientFactoryService httpClientFactoryService) 
        {
            _settings = options.Value;
            _logger = logger;
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        /// <summary>
        /// Flag indicating whether the orchestration service has been initialized.
        /// </summary>
        public bool IsInitialized => GetServiceStatus();


        /// <summary>
        /// Executes a completion request against the orchestration service.
        /// </summary>
        /// <param name="request">Request object populated from the hub APIs including agent, prompt, data source, and model information.</param>
        /// <returns>Returns a completion response from the orchestration engine.</returns>
        public async Task<LLMOrchestrationCompletionResponse> GetCompletion(LLMOrchestrationCompletionRequest request)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.LangChainAPI);           

            var body = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
            var responseMessage = await client.PostAsync("orchestration/completion",
                new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                var completionResponse = JsonConvert.DeserializeObject<LLMOrchestrationCompletionResponse>(responseContent);

                return new LLMOrchestrationCompletionResponse
                {
                    Completion = completionResponse!.Completion,
                    UserPrompt = completionResponse.UserPrompt,
                    PromptTemplate = request.Agent?.PromptPrefix,
                    AgentName = request.Agent?.Name,
                    PromptTokens = completionResponse.PromptTokens,
                    CompletionTokens = completionResponse.CompletionTokens
                };
            }

            _logger.LogWarning($"The LangChain orchestration service returned status code {responseMessage.StatusCode}: {responseContent}");

            return new LLMOrchestrationCompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = request.UserPrompt,
                PromptTemplate = request.Agent?.PromptPrefix,
                AgentName = request.Agent?.Name,
                PromptTokens = 0,
                CompletionTokens = 0
            };
        }



        /// <summary>
        /// Summarizes the input text.
        /// </summary>
        /// <param name="orchestrationRequest">The orchestration request that includes the text to summarize.</param>
        /// <returns>Returns a summary of the input text.</returns>
        public async Task<string> GetSummary(LLMOrchestrationRequest orchestrationRequest)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.LangChainAPI);

            var request = new LLMOrchestrationCompletionRequest()
            {
                SessionId = orchestrationRequest.SessionId,
                UserPrompt = orchestrationRequest.UserPrompt,
                Agent = new Agent
                {
                    Name = "summarizer",
                    Type = "summary",
                    Description = "Useful for summarizing input text based on a set of rules.",
                    PromptPrefix = "Write a concise two-word summary of the following:\n\"{text}\"\nCONCISE SUMMARY IN TWO WORDS:"
                },
                LanguageModel = new LanguageModel
                {
                    Type = LanguageModelType.OPENAI,
                    Provider = LanguageModelProvider.MICROSOFT,
                    Temperature = 0f,
                    UseChat = true
                }
            };

            var body = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
            var responseMessage = await client.PostAsync("orchestration/completion",
                new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summaryResponse = JsonConvert.DeserializeObject<LLMOrchestrationCompletionResponse>(responseContent);
                return summaryResponse!.Completion!;
            }

            return "A problem on my side prevented me from responding.";              
        }

        /// <summary>
        /// Retrieves the status of the orchestration service.
        /// </summary>
        /// <returns>True if the service is ready. Otherwise, returns false.</returns>
        private bool GetServiceStatus()
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.LangChainAPI);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "status"));

            return responseMessage.Content.ToString() == "ready";
        }
    }
}
