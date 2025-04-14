using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Context.Constants;
using FoundationaLLM.Context.Exceptions;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models;
using FoundationaLLM.Context.Models.Configuration;
using FoundationaLLM.Context.Models.CustomContainer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides a code session service that uses Azure Container Apps Dynamic Sessions to execute code.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create <see cref="HttpClient"/> instances.</param>
    /// <param name="options">The options for the Azure Container Apps code execution service.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class AzureContainerAppsCustomContainerService(
        IHttpClientFactory httpClientFactory,
        IOptions<AzureContainerAppsCustomContainerServiceSettings> options,
        ILogger<AzureContainerAppsCodeInterpreterService> logger) :
        AzureContainerAppsServiceBase(httpClientFactory, logger), ICodeSessionProviderService
    {
        private readonly AzureContainerAppsCustomContainerServiceSettings _settings = options.Value;

        /// <inheritdoc />
        public string ProviderName => CodeSessionProviderNames.AzureContainerAppsCustomContainer;

        /// <inheritdoc />
        public async Task<CreateCodeSessionResponse> CreateCodeSession(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string language,
            UnifiedUserIdentity userIdentity) =>
            _settings.Endpoints.TryGetValue(language, out var endpoints)
            && endpoints != null
            && endpoints.Count > 0
                ? await CreateCodeSessionInternal(
                    instanceId,
                    agentName,
                    conversationId,
                    context,
                    _settings.Endpoints[language].First(),
                    userIdentity)
                : throw new ContextServiceException(
                    $"Cound not find any endpoints for the [{language}] language.",
                    StatusCodes.Status400BadRequest);

        /// <inheritdoc />
        public async Task<bool> UploadFileToCodeSession(
            string codeSessionId,
            string endpoint,
            string fileName,
            Stream fileContent)
        {
            var httpClient = await CreateHttpClient();

            var multipartFormDataContent = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileContent);
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = fileName
            };
            multipartFormDataContent.Add(streamContent);

            var responseMessage = await httpClient.PostAsync(
                $"{endpoint}/files/upload?api-version=2024-10-02-preview&identifier={codeSessionId}",
                multipartFormDataContent);

            return responseMessage.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<List<CodeSessionFileStoreItem>> GetCodeSessionFileStoreItems(
            string codeSessionId,
            string endpoint)
        {
            var httpClient = await CreateHttpClient();

            var responseMessage = await httpClient.GetAsync(
                $"{endpoint}/files?api-version=2024-10-02-preview&identifier={codeSessionId}");

            if (!responseMessage.IsSuccessStatusCode)
            {
                _logger.LogError("Unable to get file store items from code session {CodeSession} on endpoint {EndpointId}.",
                    codeSessionId, endpoint);
                return [];
            }
            else
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var listFilesResponse = JsonSerializer.Deserialize<ListFilesResponse>(responseContent);

                return listFilesResponse!.Files?.Select(f => new CodeSessionFileStoreItem
                {
                    Name = Path.GetFileName(f),
                    Type = "file",
                    ParentPath = Path.GetDirectoryName(f) ?? string.Empty,
                    SizeInBytes = 0,
                    LastModifiedAt = DateTime.UtcNow,
                    ContentType = string.Empty
                }).ToList() ?? [];
            }
        }

        /// <inheritdoc/>
        public async Task DeleteCodeSessionFileStoreItems(
            string codeSessionId,
            string endpoint)
        {
            var httpClient = await CreateHttpClient();

            await httpClient.PostAsync(
                $"{endpoint}/files/delete?api-version=2024-10-02-preview&identifier={codeSessionId}",
                new StringContent("{}"));
        }

        /// <inheritdoc />
        public async Task<Stream?> DownloadFileFromCodeSession(
            string codeSessionId,
            string endpoint,
            string fileName,
            string filePath)
        {
            var httpClient = await CreateHttpClient();

            var responseMessage = await httpClient.PostAsync(
                $"{endpoint}/files/download?api-version=2024-10-02-preview&identifier={codeSessionId}",
                JsonContent.Create<DownloadFileRequest>(new DownloadFileRequest
                {
                    FileName = Path.Combine(filePath, fileName)
                }));

            if (responseMessage.IsSuccessStatusCode)
                return responseMessage.Content.ReadAsStream();
            else
                return null;
        }
    }
}
