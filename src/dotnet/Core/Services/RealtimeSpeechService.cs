using System.Net.WebSockets;
using System.Text;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.RealtimeSpeech;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// Service for handling realtime speech-to-speech communication.
    /// This service orchestrates between clients and realtime speech providers
    /// in a provider-agnostic manner.
    /// </summary>
    public class RealtimeSpeechService : IRealtimeSpeechService
    {
        private readonly IRealtimeSpeechProviderFactory _providerFactory;
        private readonly IResourceProviderService _agentResourceProvider;
        private readonly IResourceProviderService _aiModelResourceProvider;
        private readonly IResourceProviderService _promptResourceProvider;
        private readonly IResourceProviderService _configurationResourceProvider;
        private readonly IAzureCosmosDBService _cosmosDBService;
        private readonly IOrchestrationContext _orchestrationContext;
        private readonly ILogger<RealtimeSpeechService> _logger;

        public RealtimeSpeechService(
            IRealtimeSpeechProviderFactory providerFactory,
            IEnumerable<IResourceProviderService> resourceProviderServices,
            IAzureCosmosDBService cosmosDBService,
            IOrchestrationContext orchestrationContext,
            ILogger<RealtimeSpeechService> logger)
        {
            _providerFactory = providerFactory;
            var providers = resourceProviderServices.ToDictionary(rps => rps.Name);
            _agentResourceProvider = providers["FoundationaLLM_Agent"];
            _aiModelResourceProvider = providers["FoundationaLLM_AIModel"];
            _promptResourceProvider = providers["FoundationaLLM_Prompt"];
            _configurationResourceProvider = providers["FoundationaLLM_Configuration"];
            _cosmosDBService = cosmosDBService;
            _orchestrationContext = orchestrationContext;
            _logger = logger;
        }

        public async Task HandleWebSocketConnection(
            string instanceId,
            string sessionId,
            string agentName,
            WebSocket clientWebSocket,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Load agent and validate realtime speech configuration
                var agent = await GetAgentAsync(instanceId, agentName);
                if (agent?.RealtimeSpeechSettings?.Enabled != true ||
                    string.IsNullOrWhiteSpace(agent.RealtimeSpeechSettings?.RealtimeSpeechAIModelObjectId))
                {
                    await CloseWithError(clientWebSocket, "Agent does not support realtime speech", cancellationToken);
                    return;
                }

                // 2. Load the realtime speech model
                var realtimeModel = await GetRealtimeSpeechModelAsync(
                    instanceId,
                    agent.RealtimeSpeechSettings.RealtimeSpeechAIModelObjectId);

                // 3. Create the appropriate provider for this model
                var provider = _providerFactory.CreateProvider(realtimeModel);

                // 4. Get agent's main prompt for instructions
                var mainPrompt = await GetAgentMainPromptAsync(instanceId, agent);

                // 5. Get conversation history if enabled
                List<Message>? conversationHistory = null;
                if (agent.RealtimeSpeechSettings?.IncludeConversationHistory == true)
                {
                    var upn = _orchestrationContext.CurrentUserIdentity?.UPN;
                    if (!string.IsNullOrWhiteSpace(upn))
                    {
                        conversationHistory = await _cosmosDBService.GetConversationMessagesAsync(
                            sessionId,
                            upn,
                            max: 10);
                    }
                }

                // 6. Connect to realtime speech backend via provider
                using var backendSocket = await provider.ConnectAsync(realtimeModel, cancellationToken);

                // 7. Configure session with instructions
                var instructions = BuildInstructions(mainPrompt, conversationHistory);
                await provider.ConfigureSessionAsync(
                    backendSocket,
                    realtimeModel,
                    instructions,
                    cancellationToken);

                // 8. Start bidirectional message proxying
                await ProxyWebSocketMessagesAsync(
                    clientWebSocket,
                    backendSocket,
                    provider,
                    instanceId,
                    sessionId,
                    agent,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in realtime speech WebSocket connection");
                await CloseWithError(clientWebSocket, "Internal server error", cancellationToken);
            }
        }

        public async Task<RealtimeSpeechConfiguration> GetConfigurationAsync(
            string instanceId,
            string agentName)
        {
            var agent = await GetAgentAsync(instanceId, agentName);
            
            if (agent?.RealtimeSpeechSettings?.Enabled != true ||
                string.IsNullOrWhiteSpace(agent.RealtimeSpeechSettings?.RealtimeSpeechAIModelObjectId))
            {
                return new RealtimeSpeechConfiguration
                {
                    Enabled = false
                };
            }

            var model = await GetRealtimeSpeechModelAsync(
                instanceId,
                agent.RealtimeSpeechSettings.RealtimeSpeechAIModelObjectId);

            return new RealtimeSpeechConfiguration
            {
                Enabled = true,
                StopWords = agent.RealtimeSpeechSettings.StopWords,
                Voice = model.Voice
            };
        }

        private string BuildInstructions(string? mainPrompt, List<Message>? history)
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrWhiteSpace(mainPrompt))
            {
                sb.AppendLine(mainPrompt);
                sb.AppendLine();
            }

            if (history?.Count > 0)
            {
                sb.AppendLine("Previous conversation context:");
                foreach (var msg in history.TakeLast(10))
                {
                    var role = msg.Sender == "User" ? "User" : "Assistant";
                    sb.AppendLine($"{role}: {msg.Text}");
                }
            }

            return sb.ToString();
        }

        private async Task ProxyWebSocketMessagesAsync(
            WebSocket clientWebSocket,
            WebSocket backendSocket,
            IRealtimeSpeechProvider provider,
            string instanceId,
            string sessionId,
            AgentBase agent,
            CancellationToken cancellationToken)
        {
            var clientToBackend = ProxyClientToBackend(
                clientWebSocket,
                backendSocket,
                provider,
                cancellationToken);

            var backendToClient = ProxyBackendToClient(
                backendSocket,
                clientWebSocket,
                provider,
                instanceId,
                sessionId,
                agent,
                cancellationToken);

            await Task.WhenAny(clientToBackend, backendToClient);
        }

        private async Task ProxyClientToBackend(
            WebSocket clientSocket,
            WebSocket backendSocket,
            IRealtimeSpeechProvider provider,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];
            var messageBuilder = new List<byte>();

            while (!cancellationToken.IsCancellationRequested &&
                   clientSocket.State == WebSocketState.Open &&
                   backendSocket.State == WebSocketState.Open)
            {
                var result = await clientSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                messageBuilder.AddRange(buffer.Take(result.Count));

                if (result.EndOfMessage)
                {
                    var message = messageBuilder.ToArray();
                    messageBuilder.Clear();

                    // Transform message for the backend if needed
                    var transformedMessage = provider.TransformClientMessage(message);

                    await backendSocket.SendAsync(
                        new ArraySegment<byte>(transformedMessage),
                        result.MessageType,
                        true,
                        cancellationToken);
                }
            }
        }

        private async Task ProxyBackendToClient(
            WebSocket backendSocket,
            WebSocket clientSocket,
            IRealtimeSpeechProvider provider,
            string instanceId,
            string sessionId,
            AgentBase agent,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];
            var messageBuilder = new List<byte>();

            while (!cancellationToken.IsCancellationRequested &&
                   backendSocket.State == WebSocketState.Open)
            {
                var result = await backendSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                messageBuilder.AddRange(buffer.Take(result.Count));

                if (result.EndOfMessage)
                {
                    var message = messageBuilder.ToArray();
                    messageBuilder.Clear();

                    // Parse message and extract transcriptions
                    var parseResult = provider.ParseBackendMessage(message);
                    
                    // Save transcriptions to conversation
                    if (!string.IsNullOrWhiteSpace(parseResult.UserTranscription))
                    {
                        await SaveTranscriptionToConversation(
                            instanceId, sessionId, parseResult.UserTranscription, "User", agent);
                    }
                    if (!string.IsNullOrWhiteSpace(parseResult.AgentTranscription))
                    {
                        await SaveTranscriptionToConversation(
                            instanceId, sessionId, parseResult.AgentTranscription, "Agent", agent);
                    }

                    // Forward to client
                    await clientSocket.SendAsync(
                        new ArraySegment<byte>(message),
                        result.MessageType,
                        true,
                        cancellationToken);
                }
            }
        }

        private async Task SaveTranscriptionToConversation(
            string instanceId,
            string sessionId,
            string transcript,
            string sender,
            AgentBase agent)
        {
            if (agent.RealtimeSpeechSettings?.ShowTranscriptions != true)
                return;

            var upn = _orchestrationContext.CurrentUserIdentity?.UPN;
            if (string.IsNullOrWhiteSpace(upn))
                return;

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                Sender = sender,
                Text = transcript,
                TimeStamp = DateTime.UtcNow,
                Type = "realtime-speech-transcription",
                SenderDisplayName = sender == "User" ? null : agent.DisplayName
            };

            await _cosmosDBService.InsertMessageAsync(message);
        }

        private async Task<AgentBase> GetAgentAsync(string instanceId, string agentName)
        {
            var upn = _orchestrationContext.CurrentUserIdentity?.UPN;
            if (string.IsNullOrWhiteSpace(upn))
            {
                throw new InvalidOperationException("User identity not available");
            }

            var userIdentity = new UnifiedUserIdentity { UPN = upn };
            var result = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                instanceId,
                agentName,
                userIdentity);
            
            return result;
        }

        private async Task<RealtimeSpeechAIModel> GetRealtimeSpeechModelAsync(
            string instanceId,
            string modelObjectId)
        {
            var upn = _orchestrationContext.CurrentUserIdentity?.UPN;
            if (string.IsNullOrWhiteSpace(upn))
            {
                throw new InvalidOperationException("User identity not available");
            }

            var userIdentity = new UnifiedUserIdentity { UPN = upn };
            var model = await _aiModelResourceProvider.GetResourceAsync<AIModelBase>(
                instanceId,
                modelObjectId,
                userIdentity);

            if (model is not RealtimeSpeechAIModel realtimeModel)
            {
                throw new InvalidOperationException($"Model {modelObjectId} is not a realtime speech model");
            }

            // Get endpoint configuration to populate endpoint and API key
            var endpointConfig = await _configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                realtimeModel.EndpointObjectId,
                userIdentity);

            if (string.IsNullOrWhiteSpace(realtimeModel.Endpoint))
            {
                realtimeModel.Endpoint = endpointConfig.Url;
            }

            // Extract API key from authentication parameters if not already set
            if (string.IsNullOrWhiteSpace(realtimeModel.ApiKey) &&
                endpointConfig.AuthenticationParameters.TryGetValue("api_key", out var apiKeyObj))
            {
                realtimeModel.ApiKey = apiKeyObj?.ToString();
            }

            return realtimeModel;
        }

        private async Task<string?> GetAgentMainPromptAsync(string instanceId, AgentBase agent)
        {
            // This is a simplified version - in reality, you'd need to get the prompt from the agent's workflow
            // For now, return null and let the agent's default behavior handle it
            return null;
        }

        private async Task CloseWithError(
            WebSocket webSocket,
            string errorMessage,
            CancellationToken cancellationToken)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.InternalServerError,
                        errorMessage,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing WebSocket with error message");
                }
            }
        }
    }
}
