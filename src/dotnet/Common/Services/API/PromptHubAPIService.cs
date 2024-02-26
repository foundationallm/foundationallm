using System.Text;
using System.Text.Json;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Hubs;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services.API;

/// <summary>
/// Class for the PromptHub API Service
/// </summary>
/// <remarks>
/// Constructor for the PromptHub API Service
/// </remarks>
/// <param name="options"></param>
/// <param name="logger"></param>
/// <param name="httpClientFactoryService"></param>
public class PromptHubAPIService(
        IOptions<PromptHubSettings> options,
        ILogger<PromptHubAPIService> logger,
        IHttpClientFactoryService httpClientFactoryService) : APIServiceBase(Common.Constants.HttpClients.PromptHubAPI, httpClientFactoryService, logger), IPromptHubAPIService
{
    readonly PromptHubSettings _settings = options.Value;
    readonly ILogger<PromptHubAPIService> _logger = logger;
    private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
    readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();


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
                return JsonSerializer.Deserialize<string>(responseContent)!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting prompt hub status.");
            throw;
        }

        return "Error";
    }


    /// <inheritdoc/>
    public async Task<PromptHubResponse> ResolveRequest(string promptContainer, string sessionId, string promptName = "default")
    {
        try
        {
            var request = new PromptHubRequest { PromptContainer = promptContainer, PromptName = promptName, SessionId = sessionId };
            
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.PromptHubAPI);
            var body = JsonSerializer.Serialize(request, _jsonSerializerOptions);
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<PromptHubResponse>(responseContent, _jsonSerializerOptions);
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
