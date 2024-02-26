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
/// Class for the Data Source Hub API Service
/// </summary>
/// <remarks>
/// Constructor of the DataSource Hub API Service
/// </remarks>
/// <param name="options"></param>
/// <param name="logger"></param>
/// <param name="httpClientFactoryService"></param>
public class DataSourceHubAPIService(
        IOptions<DataSourceHubSettings> options,
        ILogger<DataSourceHubAPIService> logger,
        IHttpClientFactoryService httpClientFactoryService) : APIServiceBase(Common.Constants.HttpClients.DataSourceHubAPI, httpClientFactoryService, logger), IDataSourceHubAPIService
{
    readonly DataSourceHubSettings _settings = options.Value;
    readonly ILogger<DataSourceHubAPIService> _logger = logger;
    private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
    readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();


    /// <summary>
    /// Gets the status of the DataSource Hub API
    /// </summary>
    /// <returns></returns>
    public async Task<string> Status()
    {
        try
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.DataSourceHubAPI);

            var responseMessage = await client.GetAsync("status");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<string>(responseContent)!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting data hub status.");
            throw;
        }


        return "Error";
    }

    /// <summary>
    /// Gets a list of DataSources from the DataSource Hub
    /// </summary>
    /// <param name="sources">The data sources to resolve.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <returns></returns>
    public async Task<DataSourceHubResponse> ResolveRequest(List<string> sources, string sessionId)
    {
        try
        {
            var request = new DataSourceHubRequest { DataSources =  sources, SessionId = sessionId };
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.DataSourceHubAPI);
            
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    JsonSerializer.Serialize(request, _jsonSerializerOptions),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<DataSourceHubResponse>(responseContent, _jsonSerializerOptions);
                
                return response!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resolving request for data source hub.");
            throw;
        }

        return new DataSourceHubResponse();
    }

}
