using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Context.Exceptions;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides a code session service that uses Azure Container Apps Dynamic Sessions to execute code.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create <see cref="HttpClient"/> instances.</param>
    /// <param name="options">The options for the Azure Container Apps code execution service.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class AzureContainerAppsCodeSessionProviderService(
        IHttpClientFactory httpClientFactory,
        IOptions<AzureContainerAppsCodeSessionProviderServiceSettings> options,
        ILogger<AzureContainerAppsCodeSessionProviderService> logger) : ICodeSessionProviderService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly AzureContainerAppsCodeSessionProviderServiceSettings _settings = options.Value;
        private readonly ILogger<AzureContainerAppsCodeSessionProviderService> _logger = logger;

        /// <inheritdoc />
        public Task<CreateCodeSessionResponse> CreateCodeSession(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            UnifiedUserIdentity userIdentity)
        {
            ContextServiceException.ThrowIfNullOrWhiteSpace(instanceId, nameof(instanceId));
            ContextServiceException.ThrowIfNullOrWhiteSpace(agentName, nameof(agentName));
            ContextServiceException.ThrowIfNullOrWhiteSpace(conversationId, nameof(conversationId));
            ContextServiceException.ThrowIfNullOrWhiteSpace(context, nameof(context));
            ContextServiceException.ThrowIfNullOrWhiteSpace(userIdentity?.UPN, nameof(userIdentity));

            var newSessionId = $"code-{conversationId}-{context}";

            // Ensure the session identifier is no longer than 128 characters.
            if (newSessionId.Length > 128)
            {
                _logger.LogWarning("The generated code execution session identifier is longer than 128 characters. It will be truncated.");
                newSessionId = newSessionId[..128];
            }

            return Task.FromResult(new CreateCodeSessionResponse
            {
                SessionId = newSessionId,
                Endpoint = _settings.DynamicSessionsEndpoints.First()
            });
        }

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
                $"{endpoint}/files?api-version=2024-10-02-preview&identifier={codeSessionId}",
                multipartFormDataContent);

            return responseMessage.IsSuccessStatusCode;
        }

        private async Task<HttpClient> CreateHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient();

            var credentials = ServiceContext.AzureCredential;
            var tokenResult = await credentials!.GetTokenAsync(
                new(["https://dynamicsessions.io/.default"]),
                default);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            return httpClient;
        }
    }
}
