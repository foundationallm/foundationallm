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
                            FileContent = BinaryData.FromStream(
                                await responseMessage.Content.ReadAsStreamAsync()),
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
                return Result<ContextFileContent>.FailureFromException(ex);
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

                return await Result<ContextFileRecord>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the file record for file {FileId}.", fileId);
                return Result<ContextFileRecord>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result> DeleteFileRecord(
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
                    return Result.Success();

                _logger.LogError(
                    "An error occurred while deleting the file record for file {FileId}. Status code: {StatusCode}.",
                    fileId, responseMessage.StatusCode);

                return await Result.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the file record for file {FileId}.", fileId);
                return Result.FailureFromException(ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Result<ContextFileRecord>> CreateFileForConversation(
            string instanceId,
            string agentName,
            string conversationId,
            string fileName,
            string fileContentType,
            BinaryData fileContent) =>
            await CreateFile(
                instanceId,
                $"conversations/{conversationId}",
                $"agentName={agentName}",
                fileName,
                fileContentType,
                fileContent);

        /// <inheritdoc/>
        public async Task<Result<ContextFileRecord>> CreateFileForAgent(
            string instanceId,
            string agentName,
            string fileName,
            string fileContentType,
            BinaryData fileContent) =>
            await CreateFile(
                instanceId,
                $"agents/{agentName}",
                null,
                fileName,
                fileContentType,
                fileContent);

        /// <inheritdoc/>
        public async Task<Result<ContextFileRecord>> CreateFile(
            string instanceId,
            string resourceRoute,
            string? queryParameters,
            string fileName,
            string fileContentType,
            BinaryData fileContent)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var multipartFormDataContent = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileContent.ToStream());
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
                        ? Result<ContextFileRecord>.FailureFromErrorMessage(
                            $"An error occurred deserializing the response from the service for file {fileName}.")
                        : Result<ContextFileRecord>.Success(response);
                }

                _logger.LogError(
                    "An error occurred while creating a file. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return await Result<ContextFileRecord>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a file.");
                return Result<ContextFileRecord>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<CreateCodeSessionResponse>> CreateCodeSession(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string endpointProvider,
            string language,
            CodeSessionEndpointProviderOverride? endpointProviderOverride = null)
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
                            Language = language,
                            EndpointProviderOverride = endpointProviderOverride
                        }));

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<CreateCodeSessionResponse>(responseContent);

                    return response == null
                        ? Result<CreateCodeSessionResponse>.FailureFromErrorMessage(
                            $"An error occurred deserializing the response from the service.")
                        : Result<CreateCodeSessionResponse>.Success(response);
                }

                _logger.LogError(
                    "An error occurred while creating a code session. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return await Result<CreateCodeSessionResponse>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a code session.");
                return Result<CreateCodeSessionResponse>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ResourceNameCheckResult>> CheckKnowledgeUnitName(
            string instanceId,
            ResourceName resourceName) =>
            await CheckResourceName<ResourceName>(
                instanceId,
                "knowledgeUnits/check-name",
                resourceName);

        /// <inheritdoc/>
        public async Task<Result<ResourceNameCheckResult>> CheckVectorStoreId(
            string instanceId,
            CheckVectorStoreIdRequest checkVectorStoreIdRequest) =>
            await CheckResourceName<CheckVectorStoreIdRequest>(
                instanceId,
                "knowledgeUnits/check-vectorstore-id",
                checkVectorStoreIdRequest);

        /// <inheritdoc/>
        public async Task<Result<ResourceNameCheckResult>> CheckKnowledgeSourceName(
            string instanceId,
            ResourceName resourceName) =>
            await CheckResourceName<ResourceName>(
                instanceId,
                "knowledgeSources/check-name",
                resourceName);

        private async Task<Result<ResourceNameCheckResult>> CheckResourceName<T>(
            string instanceId,
            string checkActionPath,
            T checkActionPayload)
            where T : class
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);
                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/{checkActionPath}",
                    checkActionPayload);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ResourceNameCheckResult>(responseContent);

                    return response == null
                        ? Result<ResourceNameCheckResult>.FailureFromErrorMessage(
                            $"An error occurred deserializing the check resource name response for {JsonSerializer.Serialize(checkActionPayload)}.")
                        : Result<ResourceNameCheckResult>.Success(response);
                }
                _logger.LogError(
                    "An error occurred while checking resource name for {CheckActionPayload}. Status code: {StatusCode}.",
                    JsonSerializer.Serialize(checkActionPayload),
                    responseMessage.StatusCode);
                return await Result<ResourceNameCheckResult>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking resource name for {CheckActionPayload}.",
                    JsonSerializer.Serialize(checkActionPayload));
                return Result<ResourceNameCheckResult>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ResourceProviderGetResult<KnowledgeUnit>>> GetKnowledgeUnit(
            string instanceId,
            string knowledgeUnitId,
            string? agentName = null,
            ResourceProviderGetOptions? options = null) =>
            await GetKnowledgeResource<KnowledgeUnit>(
                instanceId,
                ContextResourceTypeNames.KnowledgeUnits,
                knowledgeUnitId,
                agentName: agentName,
                options: options);

        /// <inheritdoc/>
        public async Task<Result<ResourceProviderGetResult<KnowledgeSource>>> GetKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            string? agentName = null,
            ResourceProviderGetOptions? options = null) =>
            await GetKnowledgeResource<KnowledgeSource>(
                instanceId,
                ContextResourceTypeNames.KnowledgeSources,
                knowledgeSourceId,
                agentName: agentName,
                options: options);

        private async Task<Result<ResourceProviderGetResult<T>>> GetKnowledgeResource<T>(
            string instanceId,
            string knowledgeResourceType,
            string knowledgeResourceId,
            string? agentName = null,
            ResourceProviderGetOptions? options = null)
            where T : ResourceBase
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var queryParameters = options is null
                    ? new Dictionary<string, string>()
                    : options.ToQueryParams();
                if (agentName is not null)
                    queryParameters["agentName"] = agentName;
                var queryParametersString = queryParameters.Count == 0
                    ? string.Empty
                    : "?" + string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));

                var requestUrl =
                    $"instances/{instanceId}/{knowledgeResourceType}/{knowledgeResourceId}{queryParametersString}";

                var responseMessage = await client.GetAsync(requestUrl);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ResourceProviderGetResult<T>>(responseContent);

                    return response == null
                        ? Result<ResourceProviderGetResult<T>>.FailureFromErrorMessage(
                            $"An error occurred deserializing the response from the service for knowledge resource {knowledgeResourceId} of type {knowledgeResourceType}.")
                        : Result<ResourceProviderGetResult<T>>.Success(response);
                }
                _logger.LogError(
                    "An error occurred while retrieving the knowledge resource {KnowledgeResourceId} of type {KnowledgeResourceType}. Status code: {StatusCode}.",
                    knowledgeResourceId,
                    knowledgeResourceType,
                    responseMessage.StatusCode);
                return await Result<ResourceProviderGetResult<T>>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the knowledge resource {KnowledgeResourceId} of type {KnowledgeResourceType}.",
                    knowledgeResourceId,
                    knowledgeResourceType);
                return Result<ResourceProviderGetResult<T>>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<IEnumerable<ResourceProviderGetResult<KnowledgeSource>>>> GetKnowledgeSources(
            string instanceId,
            IEnumerable<string>? knowledgeSourceNames = null,
            ResourceProviderGetOptions? options = null) =>
            await GetKnowledgeResources<KnowledgeSource>(
                instanceId,
                ContextResourceTypeNames.KnowledgeSources,
                knowledgeSourceNames,
                options: options);

        /// <inheritdoc/>
        public async Task<Result<IEnumerable<ResourceProviderGetResult<KnowledgeUnit>>>> GetKnowledgeUnits(
            string instanceId,
            IEnumerable<string>? knowledgeUnitNames = null,
            ResourceProviderGetOptions? options = null) =>
            await GetKnowledgeResources<KnowledgeUnit>(
                instanceId,
                ContextResourceTypeNames.KnowledgeUnits,
                knowledgeUnitNames,
                options: options);

        private async Task<Result<IEnumerable<ResourceProviderGetResult<T>>>> GetKnowledgeResources<T>(
            string instanceId,
            string knowledgeResourceType,
            IEnumerable<string>? knowledgeResourceNames = null,
            ResourceProviderGetOptions? options = null)
            where T: ResourceBase
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    instanceId,
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var queryParameters = options is null
                    ? new Dictionary<string, string>()
                    : options.ToQueryParams();
                var queryParametersString = queryParameters.Count == 0
                    ? string.Empty
                    : "?" + string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));

                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/{knowledgeResourceType}/list{queryParametersString}",
                    new ContextKnowledgeResourceListRequest
                    {
                        KnowledgeResourceNames = knowledgeResourceNames?.ToList()
                    });
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<IEnumerable<ResourceProviderGetResult<T>>>(responseContent);

                    return response == null
                        ? Result<IEnumerable<ResourceProviderGetResult<T>>>.FailureFromErrorMessage(
                            $"An error occurred deserializing the response from the service for knowledge resources of type {knowledgeResourceType}.")
                        : Result<IEnumerable<ResourceProviderGetResult<T>>>.Success(response);
                }
                _logger.LogError(
                    "An error occurred while retrieving the knowledge resources of type {KnowledgeResourceType}. Status code: {StatusCode}.",
                    knowledgeResourceType,
                    responseMessage.StatusCode);
                return await Result<IEnumerable<ResourceProviderGetResult<T>>>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the knowledge resources of type {KnowledgeResourceType}.",
                    knowledgeResourceType);
                return Result<IEnumerable<ResourceProviderGetResult<T>>>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ResourceProviderUpsertResult<KnowledgeUnit>>> UpsertKnowledgeUnit(
            string instanceId,
            KnowledgeUnit knowledgeUnit) =>
            await UpsertKnowledgeResource<KnowledgeUnit>(
                instanceId,
                ContextResourceTypeNames.KnowledgeUnits,
                knowledgeUnit);

        /// <inheritdoc/>
        public async Task<Result<ResourceProviderUpsertResult<KnowledgeSource>>> UpsertKnowledgeSource(
            string instanceId,
            KnowledgeSource knowledgeSource) =>
            await UpsertKnowledgeResource<KnowledgeSource>(
                instanceId,
                ContextResourceTypeNames.KnowledgeSources,
                knowledgeSource);

        private async Task<Result<ResourceProviderUpsertResult<T>>> UpsertKnowledgeResource<T>(
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
                    var response = JsonSerializer.Deserialize<ResourceProviderUpsertResult<T>>(responseContent);

                    return response == null
                        ? Result<ResourceProviderUpsertResult<T>>.FailureFromErrorMessage(
                            $"An error occurred deserializing the response from the service for knowledge resource {resource.Name} of type {knowledgeResourceType}.")
                        : Result<ResourceProviderUpsertResult<T>>.Success(response);
                }
                _logger.LogError(
                    "An error occurred while upserting the knowledge resorce {ResourceName}. Status code: {StatusCode}.",
                    resource.Name,
                    responseMessage.StatusCode);
                return await Result<ResourceProviderUpsertResult<T>>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while upserting the knowledge resource {ResourceName}.",
                    resource.Name);
                return Result<ResourceProviderUpsertResult<T>>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ResourceProviderActionResult>> SetKnowledgeUnitGraph(
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
                    var response = JsonSerializer.Deserialize<ResourceProviderActionResult>(responseContent);

                    return response == null
                        ? Result<ResourceProviderActionResult>.FailureFromErrorMessage(
                            $"An error occurred deserializing the response from the service for knowledge unit {knowledgeUnitId}.")
                        : Result<ResourceProviderActionResult>.Success(response);
                }

                _logger.LogError(
                    "An error occurred while setting the knowledge graph for the knowledge unit. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return await Result<ResourceProviderActionResult>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting the knowledge graph for the knowledge unit.");

                return Result<ResourceProviderActionResult>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ContextKnowledgeSourceQueryResponse>> QueryKnowledgeSource(
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

                    return response == null
                        ? Result<ContextKnowledgeSourceQueryResponse>.FailureFromErrorMessage(
                            $"An error occurred deserializing the response from the service for knowledge source {knowledgeSourceId}.")
                        : Result<ContextKnowledgeSourceQueryResponse>.Success(response);
                }

                _logger.LogError(
                    "An error occurred while querying the knowledge source. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return await Result<ContextKnowledgeSourceQueryResponse>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while querying the knowledge source.");

                return Result<ContextKnowledgeSourceQueryResponse>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ContextKnowledgeUnitRenderGraphResponse>> RenderKnowledgeUnitGraph(
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

                    return response == null
                        ? Result<ContextKnowledgeUnitRenderGraphResponse>.FailureFromErrorMessage(
                            $"An error occurred deserializing the response from the service for knowledge unit {knowledgeSourceId}.")
                        : Result<ContextKnowledgeUnitRenderGraphResponse>.Success(response);
                }

                _logger.LogError(
                    "An error occurred while rendering the knowledge source's graph. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return await Result<ContextKnowledgeUnitRenderGraphResponse>.FailureFromHttpResponse(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rendering the knowledge source's graph.");

                return Result<ContextKnowledgeUnitRenderGraphResponse>.FailureFromException(ex);
            }
        }
    }
}
