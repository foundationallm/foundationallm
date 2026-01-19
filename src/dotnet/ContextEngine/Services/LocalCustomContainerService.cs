using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Context.Constants;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models;
using FoundationaLLM.Context.Services;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.ContextEngine.Services
{
    /// <summary>
    /// Provides a code session provider service that interacts with a local custom container instance
    /// for developing and testing code session functionalities.
    /// </summary>
    /// <param name="endpoint">The base URL of the local custom container instance used for code session operations. Cannot be null or empty.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    /// <param name="httpClientFactory">The factory used to create <see cref="HttpClient"/> instances.</param>
    public class LocalCustomContainerService(
        string endpoint,
        ILogger logger,
        IHttpClientFactory httpClientFactory) : ICodeSessionProviderService
    {
        private readonly string _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        private readonly ILogger _logger = logger;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory
            ?? throw new ArgumentNullException(nameof(httpClientFactory));

        private readonly CustomContainerServiceBase _customContainerServiceBase = new(
            logger,
            string.Empty);

        /// <inheritdoc/>
        public string ProviderName => CodeSessionProviderNames.LocalCustomContainer;

        /// <inheritdoc/>
        public async Task<CreateCodeSessionResponse> CreateCodeSession(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string language,
            UnifiedUserIdentity userIdentity) =>
            await Task.FromResult(new CreateCodeSessionResponse
            {
                SessionId = $"__local_code_session_{Guid.NewGuid().ToBase64String()}__",
                Endpoint = _endpoint
            });

        /// <inheritdoc/>
        public async Task<bool> UploadFileToCodeSession(
            string codeSessionId,
            string endpoint,
            string fileName,
            BinaryData fileContent)
        {
            var httpClient = _httpClientFactory.CreateClient();

            return await _customContainerServiceBase.UploadFileToCodeSession(
                httpClient,
                codeSessionId,
                endpoint,
                fileName,
                fileContent);
        }

        /// <inheritdoc/>
        public async Task<List<CodeSessionFileStoreItem>> GetCodeSessionFileStoreItems(
            string codeSessionId,
            string endpoint)
        {
            var httpClient = _httpClientFactory.CreateClient();

            return await _customContainerServiceBase.GetCodeSessionFileStoreItems(
                httpClient,
                codeSessionId,
                endpoint);
        }

        /// <inheritdoc/>
        public async Task DeleteCodeSessionFileStoreItems(
            string codeSessionId,
            string endpoint)
        {
            var httpClient = _httpClientFactory.CreateClient();

            await _customContainerServiceBase.DeleteCodeSessionFileStoreItems(
                httpClient,
                codeSessionId,
                endpoint);
        }

        /// <inheritdoc/>
        public async Task<Stream?> DownloadFileFromCodeSession(
            string codeSessionId,
            string endpoint,
            string fileName,
            string filePath)
        {
            var httpClient = _httpClientFactory.CreateClient();

            return await _customContainerServiceBase.DownloadFileFromCodeSession(
                httpClient,
                codeSessionId,
                endpoint,
                fileName,
                filePath);
        }

        /// <inheritdoc/>
        public async Task<CodeSessionCodeExecuteResponse> ExecuteCodeInCodeSession(
            string codeSessionId,
            string endpoint,
            string codeToExecute)
        {
            var httpClient = _httpClientFactory.CreateClient();

            return await _customContainerServiceBase.ExecuteCodeInCodeSession(
                httpClient,
                codeSessionId,
                endpoint,
                codeToExecute);
        }
    }
}
