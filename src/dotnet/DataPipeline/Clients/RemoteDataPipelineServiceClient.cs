using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipeline.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.DataPipeline.Clients
{
    /// <summary>
    /// Remote client for the Data Pipeline API.
    /// </summary>
    public class RemoteDataPipelineServiceClient : IDataPipelineServiceClient
    {
        private readonly InstanceSettings _instanceSettings;
        private readonly Task<HttpClient> _httpClientTask;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes the remote client for the Data Pipeline API.
        /// </summary>
        /// <param name="instanceOptions">The FoundationaLLM instance options.</param>
        /// <param name="httpClientFactoryService">The HTTP client factory used to create HTTP clients.</param>
        /// <param name="logger">The logger used for logging.</param>
        public RemoteDataPipelineServiceClient(
            IOptions<InstanceSettings> instanceOptions,
            IHttpClientFactoryService httpClientFactoryService,
            ILogger<RemoteDataPipelineServiceClient> logger)
        {
            _instanceSettings = instanceOptions.Value;
            _httpClientTask = httpClientFactoryService.CreateClient(
                _instanceSettings.Id,
                HttpClientNames.DataPipelineAPI,
                ServiceContext.ServiceIdentity!);
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEnumerable<IResourceProviderService> ResourceProviders { set { } }

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> CreateDataPipelineRunAsync(
            string instanceId,
            DataPipelineRun dataPipelineRun,
            DataPipelineDefinitionSnapshot dataPipelineSnapshot,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await _httpClientTask;

                var responseMessage = await httpClient.PostAsJsonAsync<DataPipelineRun>(
                    $"instances/{instanceId}/datapipelineruns",
                    dataPipelineRun);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<DataPipelineRun>(responseContent);
                    return response;
                }

                _logger.LogError(
                    "An error occurred while creating the data pipeline run. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the data pipeline run.");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> GetDataPipelineRunAsync(
            string instanceId,
            string dataPipelineName,
            string runId,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await _httpClientTask;

                var responseMessage = await httpClient.GetAsync(
                    $"instances/{instanceId}/datapipelines/{dataPipelineName}/datapipelineruns/{runId}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<DataPipelineRun>(responseContent);
                    return response;
                }

                _logger.LogError(
                    "An error occurred while retrieving the data pipeline run. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the data pipeline run.");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DataPipelineRun>> GetDataPipelineRunsAsync(
            string instanceId,
            DataPipelineRunFilter dataPipelineRunFilter,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await _httpClientTask;

                var responseMessage = await httpClient.PostAsync(
                    $"instances/{instanceId}/datapipelineruns/filter",
                    new StringContent(JsonSerializer.Serialize(dataPipelineRunFilter), Encoding.UTF8, "application/json"));

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<List<DataPipelineRun>>(responseContent);
                    return response!;
                }

                _logger.LogError(
                    "An error occurred while retrieving the data pipeline runs. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the data pipeline runs.");
                return [];
            }
        }

        /// <inheritdoc/>
        public async Task<BinaryData> GetServiceStateAsync() =>
            await Task.FromResult<BinaryData>(BinaryData.FromString(ResourceProviderNames.FoundationaLLM_DataPipeline));
    }
}
