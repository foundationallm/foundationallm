using System.Net.WebSockets;
using System.Text;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.RealtimeSpeech;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Core.Interfaces;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly ILogger<RealtimeSpeechService> _logger;

        public RealtimeSpeechService(
            IRealtimeSpeechProviderFactory providerFactory,
            IEnumerable<IResourceProviderService> resourceProviderServices,
            IAzureCosmosDBService cosmosDBService,
            IOrchestrationContext orchestrationContext,
            IConfiguration configuration,
            ILogger<RealtimeSpeechService> logger)
        {
            _providerFactory = providerFactory;
            _agentResourceProvider = resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Agent);
            _aiModelResourceProvider = resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AIModel);
            _promptResourceProvider = resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Prompt);
            _configurationResourceProvider = resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Configuration);
            _cosmosDBService = cosmosDBService;
            _orchestrationContext = orchestrationContext;
            _configuration = configuration;
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
                
                _logger.LogInformation(
                    "Loaded realtime speech model: Name={Name}, Endpoint={Endpoint}, DeploymentName={Deployment}, Version={Version}",
                    realtimeModel.Name,
                    realtimeModel.Endpoint ?? "(not set)",
                    realtimeModel.DeploymentName ?? "(not set)",
                    realtimeModel.Version ?? "(not set)");

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
            _logger.LogInformation("Starting bidirectional WebSocket proxy");
            
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

            var completedTask = await Task.WhenAny(clientToBackend, backendToClient);
            _logger.LogInformation("WebSocket proxy ended - completed task: {Task}", 
                completedTask == clientToBackend ? "ClientToBackend" : "BackendToClient");
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

                    // Log client message type (parse JSON to get type)
                    try
                    {
                        var msgText = Encoding.UTF8.GetString(message);
                        var msgDoc = System.Text.Json.JsonDocument.Parse(msgText);
                        if (msgDoc.RootElement.TryGetProperty("type", out var typeEl))
                        {
                            var msgType = typeEl.GetString();
                            if (msgType != "input_audio_buffer.append") // Don't log every audio chunk
                            {
                                _logger.LogInformation("Client -> Azure OpenAI: {MessageType}", msgType);
                            }
                        }
                    }
                    catch { /* Ignore parse errors for binary messages */ }

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
            {
                _logger.LogWarning("Cannot save transcription: UPN is not available");
                return;
            }

            var userIdentity = _orchestrationContext.CurrentUserIdentity;
            var senderDisplayName = sender == "User" 
                ? userIdentity?.Name ?? upn 
                : agent.Name;

            // For Agent messages, the UI expects text in the Content array
            // For User messages, the UI reads from the Text property directly
            List<MessageContent>? content = null;
            if (sender != "User")
            {
                content = new List<MessageContent>
                {
                    new MessageContent
                    {
                        Type = "text",
                        Value = transcript
                    }
                };
            }

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                Sender = sender,
                Text = transcript,
                TimeStamp = DateTime.UtcNow,
                Type = nameof(Message),  // Use standard Message type so it's retrieved with other messages
                SenderDisplayName = senderDisplayName,
                UPN = upn,  // Required for message retrieval
                Content = content,
                Status = Common.Models.Orchestration.OperationStatus.Completed  // Mark as completed so UI doesn't show "Thinking"
            };

            _logger.LogInformation(
                "Saving {Sender} transcription to conversation {SessionId}: {TextPreview}",
                sender, sessionId, transcript.Length > 50 ? transcript[..50] + "..." : transcript);

            await _cosmosDBService.InsertMessageAsync(message);
        }

        private async Task<AgentBase> GetAgentAsync(string instanceId, string agentName)
        {
            var userIdentity = GetValidatedUserIdentity();
            var result = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                instanceId,
                agentName,
                userIdentity);
            
            return result;
        }

        private UnifiedUserIdentity GetValidatedUserIdentity()
        {
            var userIdentity = _orchestrationContext.CurrentUserIdentity;
            if (userIdentity is null || string.IsNullOrWhiteSpace(userIdentity.UserId))
            {
                throw new InvalidOperationException("User identity not available or invalid");
            }
            return userIdentity;
        }

        private async Task<RealtimeSpeechAIModel> GetRealtimeSpeechModelAsync(
            string instanceId,
            string modelObjectId)
        {
            var userIdentity = GetValidatedUserIdentity();
            // modelObjectId is a full resource path, use the overload that takes resourcePath directly
            var model = await _aiModelResourceProvider.GetResourceAsync<AIModelBase>(
                modelObjectId,
                userIdentity);

            if (model is not RealtimeSpeechAIModel realtimeModel)
            {
                throw new InvalidOperationException($"Model {modelObjectId} is not a realtime speech model");
            }

            // Get endpoint configuration directly - same pattern as AzureOpenAIDirectService
            var endpointConfig = await _configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                realtimeModel.EndpointObjectId,
                userIdentity);

            _logger.LogInformation(
                "Endpoint config loaded: Name={Name}, AuthType={AuthType}, Url={Url}",
                endpointConfig.Name, endpointConfig.AuthenticationType, endpointConfig.Url);

            // Set endpoint URL if not already set on the model
            if (string.IsNullOrWhiteSpace(realtimeModel.Endpoint))
            {
                realtimeModel.Endpoint = endpointConfig.Url;
            }

            // Set API version from endpoint config if not already set
            if (string.IsNullOrWhiteSpace(realtimeModel.Version) && !string.IsNullOrWhiteSpace(endpointConfig.APIVersion))
            {
                realtimeModel.Version = endpointConfig.APIVersion;
            }

            // Get API key using the same pattern as HttpClientFactoryService.CreateClient
            // This works for both APIKey and AzureIdentity auth types that have api_key_configuration_name
            if (string.IsNullOrWhiteSpace(realtimeModel.ApiKey))
            {
                if (endpointConfig.AuthenticationParameters.TryGetValue(
                    AuthenticationParametersKeys.APIKeyConfigurationName, out var apiKeyConfigNameObj))
                {
                    // Handle JsonElement from deserialization (same pattern as HttpClientFactoryService)
                    var apiKeyConfigName = apiKeyConfigNameObj?.ToString();

                    if (!string.IsNullOrWhiteSpace(apiKeyConfigName))
                    {
                        realtimeModel.ApiKey = _configuration.GetValue<string>(apiKeyConfigName);
                        if (!string.IsNullOrWhiteSpace(realtimeModel.ApiKey))
                        {
                            _logger.LogInformation("Successfully retrieved API key from configuration");
                        }
                        else
                        {
                            _logger.LogWarning(
                                "API key configuration entry '{ConfigName}' not found or empty in IConfiguration",
                                apiKeyConfigName);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Endpoint {EndpointName} does not have {ParamName} in authentication parameters",
                        endpointConfig.Name, AuthenticationParametersKeys.APIKeyConfigurationName);
                }
            }

            // Use default API version if not set
            if (string.IsNullOrWhiteSpace(realtimeModel.Version) || realtimeModel.Version == "0.0")
            {
                realtimeModel.Version = "2024-10-01-preview";
            }

            return realtimeModel;
        }

        // This is a simplified version - in reality, you'd need to get the prompt from the agent's workflow
        // For now, return null and let the agent's default behavior handle it
        private Task<string?> GetAgentMainPromptAsync(string instanceId, AgentBase agent) =>
            Task.FromResult<string?>(null);

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
