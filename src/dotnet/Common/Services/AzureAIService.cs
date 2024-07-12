using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.AzureAIService;
using FoundationaLLM.Common.Models.Configuration.AzureAI;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Service to interact with Azure AI Studio.
    /// </summary>
    /// <remarks>
    /// Constructor for Azure AI Service.
    /// </remarks>
    /// <param name="azureAISettings"></param>
    /// <param name="logger"></param>
    /// <param name="blobStorageService"></param>
    /// <param name="callContext"></param>
    /// <param name="httpClientFactoryService"></param>
    public class AzureAIService(
        IOptions<AzureAISettings> azureAISettings,
        ILogger<AzureAIService> logger,
        IStorageService blobStorageService,
        ICallContext callContext,
        IHttpClientFactoryService httpClientFactoryService) : IAzureAIService
    {
        private readonly ILogger<AzureAIService> _logger = logger;
        private readonly IStorageService _blobStorageService = blobStorageService;
        private readonly AzureAISettings _settings = azureAISettings.Value;
        private readonly ICallContext _callContext = callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        /// <inheritdoc/>
        public async Task<string> CreateDataSet(InputsMapping data, string blobName)
        {
            var now = DateTime.UtcNow;

            var path = $"UI/{now.ToString("yyyy-MM-dd_ffffff_UTC")}";

            var dataSetBytes = JsonSerializer.SerializeToUtf8Bytes(data, _jsonSerializerOptions);
            Stream stream = new MemoryStream(dataSetBytes);

            await _blobStorageService.WriteFileAsync(_settings.ContainerName, path + $"/{blobName}.jsonl", stream, null, default);

            return $"azureml://subscriptions/{_settings.SubscriptionId}/resourcegroups/{_settings.ResourceGroup}/workspaces/{_settings.ProjectName}/datastores/workspaceblobstore/paths/{path}/{blobName}.jsonl";
        }

        /// <inheritdoc/>
        public async Task<DataVersionResponse> CreateDataSetVersion(string dataSetName, string dataSetPath, int version=1)
        {
            var req = new DatasetVersionRequest
            {
                DataContainerName = dataSetName,
                DataType = "UriFile",
                DataUri = dataSetPath,
                MutableProps = new Dictionary<string, string> { { "isArchived", "false" } },
                IsRegistered = true
            };

            try
            {
                var httpClient = await _httpClientFactoryService.CreateClient(HttpClients.AzureAIStudioAPI, _callContext.CurrentUserIdentity);
                var response = await httpClient.PostAsync(
                    $"{_settings.BaseUrl}/api/{_settings.Region}/data/v1.0/subscriptions/{_settings.SubscriptionId}/resourceGroups/{_settings.ResourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{_settings.ProjectName}/dataversion/{dataSetName}/versions",
                    JsonContent.Create(req));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<DataVersionResponse>(responseContent, _jsonSerializerOptions) ?? new DataVersionResponse();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error creating data set version.");
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<Guid> SubmitJob(string displayName, string dataSetName, int dataSetVersion, string metrics)
        {
            if (string.IsNullOrWhiteSpace(metrics) && _settings is {Metrics: not null})
            {
                metrics = _settings.Metrics;
            }
            var job = new AzureAIJobRequest
            {
                FlowDefinitionResourceId = _settings.FlowDefinitionResourceId,
                RunId = Guid.NewGuid().ToString()
            };
            job.BatchDataInput = new BatchDataInput { DataUri = $"azureml://locations/{_settings.Region}/workspaces/{job.RunId}/data/{dataSetName}/versions/{dataSetVersion}" };
            job.Connections = new GptConnections
            {
                GptCoherence = new GptConnection { Connection = "Default_AzureOpenAI", DeploymentName = _settings.Deployment },
                GptFluency = new GptConnection { Connection = "Default_AzureOpenAI", DeploymentName = _settings.Deployment },
                GptGroundedness = new GptConnection { Connection = "Default_AzureOpenAI", DeploymentName = _settings.Deployment },
                GptRelevance = new GptConnection { Connection = "Default_AzureOpenAI", DeploymentName = _settings.Deployment },
                GptSimilarity = new GptConnection { Connection = "Default_AzureOpenAI", DeploymentName = _settings.Deployment }
            };
            job.InputsMapping = new InputsMapping { Question = "${data.question}", Answer = "${data.answer}", Context = "${data.context}", GroundTruth = "${data.ground_truth}", Metrics = metrics };
            job.RunExperimentName = job.RunId;
            job.RuntimeName = "automatic";
            job.RunDisplayNameGenerationType = "UserProvidedMacro";
            job.Properties = new Dictionary<string, string> { { "runType", "eval_run" } };
            job.SessionSetupMode = "SystemWait";

            try
            {
                var options = new JsonSerializerOptions
                {
                    MaxDepth = 10
                };

                var httpClient = await _httpClientFactoryService.CreateClient(HttpClients.AzureAIStudioAPI, _callContext.CurrentUserIdentity);
                var response = await httpClient.PostAsync(
                    $"{_settings.BaseUrl}/api/{_settings.Region}/flow/api/subscriptions/{_settings.SubscriptionId}/resourceGroups/{_settings.ResourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{_settings.ProjectName}/BulkRuns/submit",
                    JsonContent.Create(job));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Guid.Parse(responseContent);
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error submitting job");
            }

            return Guid.Empty;
        }

        /// <inheritdoc/>
        public async Task<string> GetJobStatus(Guid jobId)
        {
            try
            {
                var httpClient = await _httpClientFactoryService.CreateClient(HttpClients.AzureAIStudioAPI, _callContext.CurrentUserIdentity);
                var response = await httpClient.GetAsync(
                    $"{_settings.BaseUrl}/api/{_settings.Region}/history/v1.0/subscriptions/{_settings.SubscriptionId}/resourceGroups/{_settings.ResourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{_settings.ProjectName}/runs/{jobId}"
                    );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error getting job status");
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<string> GetResultsByIndex(Guid jobId, int startIndex=0, int endIndex=149)
        {
            try
            {
                var httpClient = await _httpClientFactoryService.CreateClient(HttpClients.AzureAIStudioAPI, _callContext.CurrentUserIdentity);
                var response = await httpClient.GetAsync(
                    $"{_settings.BaseUrl}/api/{_settings.Region}/flow/api/subscriptions/{_settings.SubscriptionId}/resourceGroups/{_settings.ResourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{_settings.ProjectName}/BulkRuns/{jobId}/childRuns?startIndex={startIndex}&endIndex={endIndex}"
                    );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error downloading results by index");
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<string> DownloadResults(Guid jobId)
        {
            try
            {
                var httpClient = await _httpClientFactoryService.CreateClient(HttpClients.AzureAIStudioAPI, _callContext.CurrentUserIdentity);
                var response = await httpClient.GetAsync(
                    $"/api/{_settings.Region}/flow/api/subscriptions/{_settings.SubscriptionId}/resourceGroups/{_settings.ResourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{_settings.ProjectName}/BulkRuns/{jobId}/results"
                    );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error downloading results");                
            }

            return null;
        }
    }
}
