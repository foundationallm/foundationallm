using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Context.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Base class for Azure Container Apps Dynamic Sessions services.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create <see cref="HttpClient"/> instances.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class AzureContainerAppsServiceBase(
        IHttpClientFactory httpClientFactory,
        ILogger logger)
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        protected readonly ILogger _logger = logger;

        protected Task<CreateCodeSessionResponse> CreateCodeSessionInternal(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string sessionEndpoint,
            UnifiedUserIdentity userIdentity)
        {
            ContextServiceException.ThrowIfNullOrWhiteSpace(instanceId, nameof(instanceId));
            ContextServiceException.ThrowIfNullOrWhiteSpace(agentName, nameof(agentName));
            ContextServiceException.ThrowIfNullOrWhiteSpace(conversationId, nameof(conversationId));
            ContextServiceException.ThrowIfNullOrWhiteSpace(context, nameof(context));
            ContextServiceException.ThrowIfNullOrWhiteSpace(userIdentity?.UPN, nameof(userIdentity));

            var newSessionId = GetNewSessionId(
                conversationId,
                context);

            // Ensure the session identifier is no longer than 128 characters.
            if (newSessionId.Length > 128)
            {
                _logger.LogWarning("The generated code execution session identifier is longer than 128 characters. It will be truncated.");
                newSessionId = newSessionId[..128];
            }

            return Task.FromResult(new CreateCodeSessionResponse
            {
                SessionId = newSessionId.ToLower(),
                Endpoint = sessionEndpoint
            });
        }

        protected async Task<HttpClient> CreateHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient();

            var credentials = ServiceContext.AzureCredential;
            var tokenResult = await credentials!.GetTokenAsync(
                new(["https://dynamicsessions.io/.default"]),
                default);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResult.Token);
            httpClient.Timeout = TimeSpan.FromMinutes(10);

            return httpClient;
        }

        /// <summary>
        /// Generates a new session identifier based on the conversation identifier and context.
        /// </summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="context">Additional context for the code session.</param>
        /// <returns></returns>
        /// <remarks>The context is usually the name of the agent tool that requests the code session.</remarks>
        protected virtual string GetNewSessionId(
            string conversationId,
            string context) =>
            $"code-{conversationId}-{context}";
    }
}
