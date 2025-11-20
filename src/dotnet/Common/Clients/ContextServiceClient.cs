using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Provides methods to call the FoundationaLLM Context API service.
    /// </summary>
    public class ContextServiceClient(
        IOrchestrationContext callContext,
        IHttpClientFactoryService httpClientFactoryService,
        ILogger<ContextServiceClient> logger) : IContextServiceClient
    {
        private readonly IOrchestrationContext _callContext = callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        private readonly ILogger<ContextServiceClient> _logger = logger;

        /// <inheritdoc/>
        public async Task<Result<ContextFileContent>> GetFileContent(
            string instanceId,
            string fileId)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.GetAsync($"instances/{instanceId}/files/{fileId}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    return Result<ContextFileContent>.Success(
                        new ContextFileContent
                        {
                            FileContent = await responseMessage.Content.ReadAsStreamAsync(),
                            FileName = responseMessage!.Content.Headers.ContentDisposition!.FileNameStar!,
                            ContentType = responseMessage!.Content.Headers.ContentType!.MediaType!
                        });
                }

                _logger.LogError(
                    "An error occurred while retrieving the file content. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return await Result<ContextFileContent>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the file content.");
                return Result<ContextFileContent>.FailureFromErrorMessage(
                    "An error occurred while retrieving the file content.");
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ContextFileRecord>> GetFileRecord(
            string instanceId,
            string fileId)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.GetAsync($"instances/{instanceId}/fileRecords/{fileId}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextFileRecord>(responseContent);

                    return response == null
                        ? Result<ContextFileRecord>.FailureFromErrorMessage(
                            $"An error occurred deserializing the response from the service for file {fileId}.")
                        : Result<ContextFileRecord>.Success(response);
                }

                _logger.LogError(
                    "An error occurred while retrieving the file record for file {FileId}. Status code: {StatusCode}.",
                    fileId, responseMessage.StatusCode);

                return new ContextServiceResponse<ContextFileRecord>
                {
                    IsSuccess = false,
                    ErrorMessage = $"The service responded with an error status code for file {fileId}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the file record for file {FileId}.", fileId);
                return new ContextServiceResponse<ContextFileRecord>
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while retrieving the file record for file {fileId}."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse> DeleteFileRecord(
            string instanceId,
            string fileId)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.DeleteAsync($"instances/{instanceId}/fileRecords/{fileId}");

                if (responseMessage.IsSuccessStatusCode)
                    return new ContextServiceResponse
                    {
                        IsSuccess = true
                    };

                _logger.LogError(
                    "An error occurred while deleting the file record for file {FileId}. Status code: {StatusCode}.",
                    fileId, responseMessage.StatusCode);

                return new ContextServiceResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"The service responded with an error status code for file {fileId}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the file record for file {FileId}.", fileId);
                return new ContextServiceResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while deleting the file record for file {fileId}."
                };
            }
        }


        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ContextFileRecord>> CreateFileForConversation(
            string instanceId,
            string agentName,
            string conversationId,
            string fileName,
            string fileContentType,
            Stream fileContent) =>
            await CreateFile(
                instanceId,
                $"conversations/{conversationId}",
                $"agentName={agentName}",
                fileName,
                fileContentType,
                fileContent);

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ContextFileRecord>> CreateFileForAgent(
            string instanceId,
            string agentName,
            string fileName,
            string fileContentType,
            Stream fileContent) =>
            await CreateFile(
                instanceId,
                $"agents/{agentName}",
                null,
                fileName,
                fileContentType,
                fileContent);

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ContextFileRecord>> CreateFile(
            string instanceId,
            string resourceRoute,
            string? queryParameters,
            string fileName,
            string fileContentType,
            Stream fileContent)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var multipartFormDataContent = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileContent);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(fileContentType);
                streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = fileName
                };
                multipartFormDataContent.Add(streamContent);

                queryParameters = string.IsNullOrWhiteSpace(queryParameters)
                    ? string.Empty
                    : $"?{queryParameters}";

                var responseMessage = await client.PostAsync(
                    $"instances/{instanceId}/{resourceRoute}/files{queryParameters}",
                    multipartFormDataContent);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextFileRecord>(responseContent);

                    return response == null
                        ? new ContextServiceResponse<ContextFileRecord>
                        {
                            IsSuccess = false,
                            ErrorMessage = "An error occurred deserializing the response from the service."
                        }
                        : new ContextServiceResponse<ContextFileRecord>
                        {
                            IsSuccess = true,
                            Result = response
                        };
                }

                _logger.LogError(
                    "An error occurred while creating a file. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextServiceResponse<ContextFileRecord>
                {
                    IsSuccess = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a file.");
                return new ContextServiceResponse<ContextFileRecord>
                {
                    IsSuccess = false,
                    ErrorMessage = "An error occurred while creating a file."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<CreateCodeSessionResponse>> CreateCodeSession(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string endpointProvider,
            string language)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.PostAsync(
                    $"instances/{instanceId}/codeSessions",
                    JsonContent.Create(
                        new CreateCodeSessionRequest
                        {
                            AgentName = agentName,
                            ConversationId = conversationId,
                            Context = context,
                            EndpointProvider = endpointProvider,
                            Language = language
                        }));

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<CreateCodeSessionResponse>(responseContent);

                    return response == null
                        ? new ContextServiceResponse<CreateCodeSessionResponse>
                        {
                            IsSuccess = false,
                            ErrorMessage = "An error occurred deserializing the response from the service."
                        }
                        : new ContextServiceResponse<CreateCodeSessionResponse>
                        {
                            IsSuccess = true,
                            Result = response
                        };
                }

                _logger.LogError(
                    "An error occurred while creating a code session. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextServiceResponse<CreateCodeSessionResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a code session.");
                return new ContextServiceResponse<CreateCodeSessionResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = "An error occurred while creating a code session."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ResourceProviderGetResult<KnowledgeUnit>>> GetKnowledgeUnit(
            string instanceId,
            string knowledgeUnitId,
            string? agentName = null) =>
            await GetKnowledgeResource<KnowledgeUnit>(
                instanceId,
                ContextResourceTypeNames.KnowledgeUnits,
                knowledgeUnitId,
                agentName);

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ResourceProviderGetResult<KnowledgeSource>>> GetKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            string? agentName = null) =>
            await GetKnowledgeResource<KnowledgeSource>(
                instanceId,
                ContextResourceTypeNames.KnowledgeSources,
                knowledgeSourceId,
                agentName);

        private async Task<ContextServiceResponse<ResourceProviderGetResult<T>>> GetKnowledgeResource<T>(
            string instanceId,
            string knowledgeResourceType,
            string knowledgeResourceId,
            string? agentName = null)
            where T : ResourceBase
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);
                var responseMessage = await client.GetAsync(
                    agentName is null
                        ? $"instances/{instanceId}/{knowledgeResourceType}/{knowledgeResourceId}"
                        : $"instances/{instanceId}/{knowledgeResourceType}/{knowledgeResourceId}?agentName={agentName}");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextServiceResponse<ResourceProviderGetResult<T>>>(responseContent);
                    return response!;
                }
                _logger.LogError(
                    "An error occurred while retrieving the knowledge resource {KnowledgeResourceId} of type {KnowledgeResourceType}. Status code: {StatusCode}.",
                    knowledgeResourceId,
                    knowledgeResourceType,
                    responseMessage.StatusCode);
                return new ContextServiceResponse<ResourceProviderGetResult<T>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"The service responded with error status code {responseMessage.StatusCode}.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the knowledge resource {KnowledgeResourceId} of type {KnowledgeResourceType}.",
                    knowledgeResourceId,
                    knowledgeResourceType);
                return new ContextServiceResponse<ResourceProviderGetResult<T>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while retrieving the knowledge resource {knowledgeResourceId} of type {knowledgeResourceType}: {ex.Message}."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<IEnumerable<ResourceProviderGetResult<KnowledgeSource>>>> GetKnowledgeSources(
            string instanceId,
            IEnumerable<string>? knowledgeSourceNames = null) =>
            await GetKnowledgeResources<KnowledgeSource>(
                instanceId,
                ContextResourceTypeNames.KnowledgeSources,
                knowledgeSourceNames);

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<IEnumerable<ResourceProviderGetResult<KnowledgeUnit>>>> GetKnowledgeUnits(
            string instanceId,
            IEnumerable<string>? knowledgeUnitNames = null) =>
            await GetKnowledgeResources<KnowledgeUnit>(
                instanceId,
                ContextResourceTypeNames.KnowledgeUnits,
                knowledgeUnitNames);

        private async Task<ContextServiceResponse<IEnumerable<ResourceProviderGetResult<T>>>> GetKnowledgeResources<T>(
            string instanceId,
            string knowledgeResourceType,
            IEnumerable<string>? knowledgeResourceNames = null)
            where T: ResourceBase
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);
                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/{knowledgeResourceType}/list",
                    new ContextKnowledgeResourceListRequest
                    {
                        KnowledgeResourceNames = knowledgeResourceNames?.ToList()
                    });
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextServiceResponse<IEnumerable<ResourceProviderGetResult<T>>>>(responseContent);
                    return response!;
                }
                _logger.LogError(
                    "An error occurred while retrieving the knowledge resources of type {KnowledgeResourceType}. Status code: {StatusCode}.",
                    knowledgeResourceType,
                    responseMessage.StatusCode);
                return new ContextServiceResponse<IEnumerable<ResourceProviderGetResult<T>>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"The service responded with error status code {responseMessage.StatusCode}.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the knowledge resources of type {KnowledgeResourceType}.",
                    knowledgeResourceType);
                return new ContextServiceResponse<IEnumerable<ResourceProviderGetResult<T>>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while retrieving the knowledge resources of type {knowledgeResourceType}: {ex.Message}."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ResourceProviderUpsertResult<KnowledgeUnit>>> UpsertKnowledgeUnit(
            string instanceId,
            KnowledgeUnit knowledgeUnit) =>
            await UpsertKnowledgeResource<KnowledgeUnit>(
                instanceId,
                ContextResourceTypeNames.KnowledgeUnits,
                knowledgeUnit);

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ResourceProviderUpsertResult<KnowledgeSource>>> UpsertKnowledgeSource(
            string instanceId,
            KnowledgeSource knowledgeSource) =>
            await UpsertKnowledgeResource<KnowledgeSource>(
                instanceId,
                ContextResourceTypeNames.KnowledgeSources,
                knowledgeSource);

        private async Task<ContextServiceResponse<ResourceProviderUpsertResult<T>>> UpsertKnowledgeResource<T>(
            string instanceId,
            string knowledgeResourceType,
            T resource)
            where T : ResourceBase
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);
                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/{knowledgeResourceType}",
                    resource);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextServiceResponse<ResourceProviderUpsertResult<T>>>(responseContent);
                    return response!;
                }
                _logger.LogError(
                    "An error occurred while upserting the knowledge resorce {ResourceName}. Status code: {StatusCode}.",
                    resource.Name,
                    responseMessage.StatusCode);
                return new ContextServiceResponse<ResourceProviderUpsertResult<T>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"The service responded with error status code {responseMessage.StatusCode}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while upserting the knowledge resource {ResourceName}.",
                    resource.Name);
                return new ContextServiceResponse<ResourceProviderUpsertResult<T>>
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while upserting the knowledge resource {resource.Name}: {ex.Message}."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ResourceProviderActionResult>> SetKnowledgeUnitGraph(
            string instanceId,
            string knowledgeUnitId,
            ContextKnowledgeUnitSetGraphRequest setGraphRequest)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/knowledgeUnits/{knowledgeUnitId}/set-graph",
                    setGraphRequest);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextServiceResponse<ResourceProviderActionResult>>(responseContent);
                    return response!;
                }

                _logger.LogError(
                    "An error occurred while setting the knowledge graph for the knowledge unit. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextServiceResponse<ResourceProviderActionResult>
                {
                    IsSuccess = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting the knowledge graph for the knowledge unit.");

                return new ContextServiceResponse<ResourceProviderActionResult>
                {
                    IsSuccess = false,
                    ErrorMessage = "An error occurred while setting the knowledge graph for the knowledge unit."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextKnowledgeSourceQueryResponse> QueryKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceQueryRequest queryRequest)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/knowledgeSources/{knowledgeSourceId}/query",
                    queryRequest);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextKnowledgeSourceQueryResponse>(responseContent);
                    return response!;
                }

                _logger.LogError(
                    "An error occurred while querying the knowledge source. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextKnowledgeSourceQueryResponse
                {
                    Source = knowledgeSourceId,
                    IsSuccess = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while querying the knowledge source.");

                return new ContextKnowledgeSourceQueryResponse
                {
                    Source = knowledgeSourceId,
                    IsSuccess = false,
                    ErrorMessage = "An error occurred while querying the knowledge source."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextKnowledgeUnitRenderGraphResponse> RenderKnowledgeUnitGraph(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceQueryRequest? queryRequest)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/{ContextResourceTypeNames.KnowledgeUnits}/{knowledgeSourceId}/render-graph",
                    queryRequest);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextKnowledgeUnitRenderGraphResponse>(responseContent);
                    return response!;
                }

                _logger.LogError(
                    "An error occurred while rendering the knowledge source's graph. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextKnowledgeUnitRenderGraphResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rendering the knowledge source's graph.");

                return new ContextKnowledgeUnitRenderGraphResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "An error occurred while rendering the knowledge source's graph."
                };
            }
        }
    }
}
