using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using FoundationaLLM.Common.Interfaces;
using System.Collections;

namespace FoundationaLLM.AgentFactory.Core.Services;

/// <summary>
/// Class for the PromptHub API Service
/// </summary>
public class PromptHubAPIService : IPromptHubAPIService
{
    readonly PromptHubSettings _settings;
    readonly ILogger<PromptHubAPIService> _logger;
    private readonly IHttpClientFactoryService _httpClientFactoryService;
    readonly JsonSerializerSettings _jsonSerializerSettings;
    static Hashtable _cache = new Hashtable();

    /// <summary>
    /// Constructor for the PromptHub API Service
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="httpClientFactoryService"></param>
    public PromptHubAPIService(
            IOptions<PromptHubSettings> options,
            ILogger<PromptHubAPIService> logger,
            IHttpClientFactoryService httpClientFactoryService)
    {
        _settings = options.Value;
        _logger = logger;
        _httpClientFactoryService = httpClientFactoryService;
        _jsonSerializerSettings = Common.Settings.CommonJsonSerializerSettings.GetJsonSerializerSettings();
    }


    /// <summary>
    /// Gets the status of the Prompt Hub API
    /// </summary>
    /// <returns></returns>
    public async Task<string> Status()
    {
        try
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.PromptHubAPI);

            var responseMessage = await client.GetAsync("status");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return responseContent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting prompt hub status.");
            throw;
        }

        return "Error";
    }


    /// <summary>
    /// Used to get prompts for a target agent and user context.
    /// </summary>
    /// <param name="agentName">Name of the agent for which to retrieve prompt values.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="promptName">Name of the prompt for which to retrieve prompt values.</param>
    /// <returns>Returns a <see cref="PromptHubResponse"/> object containing the list of prompts for the specified agent.</returns>
    public async Task<PromptHubResponse> ResolveRequest(string agentName, string sessionId, string promptName = "default")
    {
        try
        {
            string responseContent = null;

            string _cacheKey = $"{sessionId}-{_httpClientFactoryService.GetAgent()}";

            if (_cache.ContainsKey(_cacheKey))
            {
                responseContent = _cache[_cacheKey].ToString();
                var response = JsonConvert.DeserializeObject<PromptHubResponse>(responseContent, _jsonSerializerSettings);
                return response!;
            }

            var request = new PromptHubRequest { AgentName = agentName, PromptName = promptName, SessionId = sessionId };
            
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.PromptHubAPI);
            var body = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                responseContent = await responseMessage.Content.ReadAsStringAsync();
                if (_cache.ContainsKey(_cacheKey))
                    _cache[_cacheKey] = responseContent;
                else
                    _cache.Add(_cacheKey, responseContent);
                var response = JsonConvert.DeserializeObject<PromptHubResponse>(responseContent, _jsonSerializerSettings);
                return response!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resolving request for prompt Hub.");
            throw;
        }

        return new PromptHubResponse();
    }    
}
