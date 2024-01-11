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
/// Class for the Data Source Hub API Service
/// </summary>
public class DataSourceHubAPIService : IDataSourceHubAPIService
{
    readonly DataSourceHubSettings _settings;
    readonly ILogger<DataSourceHubAPIService> _logger;
    private readonly IHttpClientFactoryService _httpClientFactoryService;
    readonly JsonSerializerSettings _jsonSerializerSettings;
    static Hashtable _cache = new Hashtable();

    /// <summary>
    /// Constructor of the DataSource Hub API Service
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="httpClientFactoryService"></param>
    public DataSourceHubAPIService(
            IOptions<DataSourceHubSettings> options,
            ILogger<DataSourceHubAPIService> logger,
            IHttpClientFactoryService httpClientFactoryService)
    {
        _settings = options.Value;
        _logger = logger;
        _httpClientFactoryService = httpClientFactoryService;
        _jsonSerializerSettings = Common.Settings.CommonJsonSerializerSettings.GetJsonSerializerSettings();
    }


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
                return responseContent;
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
            string responseContent = null;

            if (_cache.ContainsKey($"{sessionId}-{_httpClientFactoryService.GetAgent()}"))
            {
                responseContent = _cache[$"{sessionId}-{_httpClientFactoryService.GetAgent()}"].ToString();
                var response = JsonConvert.DeserializeObject<DataSourceHubResponse>(responseContent, _jsonSerializerSettings);
                return response!;
            }

            var request = new DataSourceHubRequest { DataSources =  sources, SessionId = sessionId };
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.DataSourceHubAPI);
            
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    JsonConvert.SerializeObject(request, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                responseContent = await responseMessage.Content.ReadAsStringAsync();
                _cache.Add($"{sessionId}-{_httpClientFactoryService.GetAgent()}", responseContent);
                var response = JsonConvert.DeserializeObject<DataSourceHubResponse>(responseContent, _jsonSerializerSettings);
                
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
