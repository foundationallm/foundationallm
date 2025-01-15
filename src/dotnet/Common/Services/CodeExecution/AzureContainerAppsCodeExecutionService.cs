using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Common.Models.Configuration.CodeExecution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services.CodeExecution
{
    /// <summary>
    /// Provides a code execution service that uses Azure Container Apps Dynamic Sessions to execute code.
    /// </summary>
    /// <param name="options">The options for the Azure Container Apps code execution service.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class AzureContainerAppsCodeExecutionService(
        IOptions<AzureContainerAppsCodeExecutionServiceSettings> options,
        ILogger<AzureContainerAppsCodeExecutionService> logger) : ICodeExecutionService
    {
        private readonly AzureContainerAppsCodeExecutionServiceSettings _settings = options.Value;
        private readonly ILogger<AzureContainerAppsCodeExecutionService> _logger = logger;

        /// <inheritdoc />
        public Task<CodeExecutionSession> CreateCodeExecutionSession(
            string instanceId,
            string context,
            string conversationId,
            UnifiedUserIdentity userIdentity)
        {
            string newSessionId;

            if (string.IsNullOrWhiteSpace(context))
            {
                // Since the context is invalid, the session identifier will be a random GUID.

                _logger.LogWarning("An empty context was provided for creating a code execution session identifier.");
                newSessionId = Guid.NewGuid().ToString().ToLower();
            }
            else if (string.IsNullOrWhiteSpace(conversationId))
            {
                // Since the conversation identifier is invalid, the session identifier will use a random GUID instead.
                _logger.LogWarning("An empty conversation identifier was provided for creating a code execution session identifier.");
                newSessionId = $"{context}-{Guid.NewGuid().ToString().ToLower()}";
            }
            else
            {
                // The session identifier will be a combination of the context and conversation identifier.
                newSessionId = $"{context}-{conversationId}";
            }

            // Ensure the session identifier is no longer than 128 characters.
            if (newSessionId.Length > 128)
            {
                _logger.LogWarning("The generated code execution session identifier is longer than 128 characters. It will be truncated.");
                newSessionId = newSessionId[..128];
            }

            return Task.FromResult(new CodeExecutionSession
            {
                SessionId = newSessionId,
                Endpoint = _settings.DynamicSessionsEndpoints.First()
            });
        }
    }
}
