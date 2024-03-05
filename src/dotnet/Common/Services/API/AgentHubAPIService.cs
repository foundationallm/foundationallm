using System.Text;
using System.Text.Json;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Hubs;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services.API;

/// <summary>
/// Class for the Agent Hub API Service.
/// </summary>
/// <remarks>
/// Constructor for the Agent Hub API Service
/// </remarks>
/// <param name="options"></param>
/// <param name="logger"></param>
/// <param name="httpClientFactoryService"></param>
public class AgentHubAPIService(
        IOptions<AgentHubSettings> options,
        ILogger<AgentHubAPIService> logger,
        IHttpClientFactoryService httpClientFactoryService) : APIServiceBase(HttpClients.AgentHubAPI, httpClientFactoryService, logger), IAgentHubAPIService
{
    readonly AgentHubSettings _settings = options.Value;
    readonly ILogger<AgentHubAPIService> _logger = logger;
    private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
    readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

    /// <summary>
    /// Gets the status of the Agent Hub API
    /// </summary>
    /// <returns></returns>
    public async Task<string> Status()
    {
        try
        {
            var client = _httpClientFactoryService.CreateClient(HttpClients.AgentHubAPI);

            var responseMessage = await client.GetAsync("status");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<string>(responseContent)!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting agent hub status.");
            throw;
        }

        return "Error";
    }

    /// <inheritdoc/>
    public async Task<AgentHubResponse> ResolveRequest(string userPrompt, string sessionId)
    {
        try
        {
            var request = new AgentHubRequest { UserPrompt = userPrompt, SessionId = sessionId };

            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentHubAPI);
                        
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    JsonSerializer.Serialize(request, _jsonSerializerOptions),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<AgentHubResponse>(responseContent, _jsonSerializerOptions);
                return response!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resolving request for Agent Hub.");
            throw;
        }

        return new AgentHubResponse();
    }

    /// <inheritdoc/>
    public async Task<List<AgentMetadata>> ListAgents()
    {
        try
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentHubAPI);

            var responseMessage = await client.GetAsync("list");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<List<AgentMetadata>>(responseContent, _jsonSerializerOptions);
                return response!;
            }

            throw new Exception($"The Agent Hub API call returned with status {responseMessage.StatusCode}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving list of agents from Agent Hub.");
            throw;
        }
    }
}
