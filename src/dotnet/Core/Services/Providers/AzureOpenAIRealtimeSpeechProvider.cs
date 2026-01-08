using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FoundationaLLM.Common.Models.RealtimeSpeech;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Core.Services.Providers
{
    /// <summary>
    /// Azure OpenAI GPT-Realtime provider implementation.
    /// Adapted from realtime_client.py in the realtime-speech-prototype.
    /// </summary>
    public class AzureOpenAIRealtimeSpeechProvider : IRealtimeSpeechProvider
    {
        private readonly ILogger<AzureOpenAIRealtimeSpeechProvider> _logger;

        public string ProviderType => "azure-openai-realtime";

        public AzureOpenAIRealtimeSpeechProvider(
            ILogger<AzureOpenAIRealtimeSpeechProvider> logger)
        {
            _logger = logger;
        }

        public async Task<WebSocket> ConnectAsync(
            RealtimeSpeechAIModel model,
            CancellationToken cancellationToken)
        {
            var clientWebSocket = new ClientWebSocket();
            
            // Build WebSocket URL for Azure OpenAI Realtime API
            var wsUrl = BuildWebSocketUrl(model);
            _logger.LogInformation(
                "Connecting to Azure OpenAI Realtime API: {Url}, Deployment: {Deployment}, Version: {Version}",
                wsUrl.Split('?')[0], // Log URL without query params for security
                model.DeploymentName,
                model.Version);
            
            // Set authentication header
            if (!string.IsNullOrWhiteSpace(model.ApiKey))
            {
                clientWebSocket.Options.SetRequestHeader("api-key", model.ApiKey);
            }
            else
            {
                _logger.LogWarning("No API key provided for Azure OpenAI Realtime connection");
            }
            
            await clientWebSocket.ConnectAsync(new Uri(wsUrl), cancellationToken);
            _logger.LogInformation("Successfully connected to Azure OpenAI Realtime API");
            return clientWebSocket;
        }

        public async Task ConfigureSessionAsync(
            WebSocket backendSocket,
            RealtimeSpeechAIModel model,
            string? instructions,
            CancellationToken cancellationToken)
        {
            // Build turn detection config - default to server_vad if not specified
            // Server VAD is required for automatic response generation
            object? turnDetectionConfig;
            if (model.TurnDetection != null)
            {
                turnDetectionConfig = new
                {
                    type = model.TurnDetection.Type ?? "server_vad",
                    threshold = model.TurnDetection.Threshold,
                    prefix_padding_ms = model.TurnDetection.PrefixPaddingMs,
                    silence_duration_ms = model.TurnDetection.SilenceDurationMs
                };
            }
            else
            {
                // Default server VAD settings for good user experience
                turnDetectionConfig = new
                {
                    type = "server_vad",
                    threshold = 0.5,
                    prefix_padding_ms = 300,
                    silence_duration_ms = 500
                };
            }

            var sessionUpdate = new
            {
                type = "session.update",
                session = new
                {
                    modalities = new[] { "text", "audio" },
                    instructions = instructions,
                    voice = model.Voice ?? "alloy",
                    input_audio_format = model.InputAudioFormat ?? "pcm16",
                    output_audio_format = model.OutputAudioFormat ?? "pcm16",
                    input_audio_transcription = new
                    {
                        model = model.InputAudioTranscriptionModel ?? "whisper-1"
                    },
                    turn_detection = turnDetectionConfig
                }
            };

            var json = JsonSerializer.Serialize(sessionUpdate);
            _logger.LogInformation("Configuring session with: {Config}", json);
            
            var bytes = Encoding.UTF8.GetBytes(json);
            await backendSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);
        }

        public RealtimeSpeechMessageResult ParseBackendMessage(byte[] message)
        {
            var result = new RealtimeSpeechMessageResult();
            
            try
            {
                var json = Encoding.UTF8.GetString(message);
                var doc = JsonDocument.Parse(json);
                
                if (doc.RootElement.TryGetProperty("type", out var typeElement))
                {
                    result.MessageType = typeElement.GetString();
                    
                    // Log important message types (skip audio deltas to avoid spam)
                    if (result.MessageType != "response.audio.delta" && 
                        result.MessageType != "response.audio_transcript.delta")
                    {
                        _logger.LogInformation("Azure OpenAI message: {MessageType}", result.MessageType);
                    }
                }

                // Handle user transcription completed
                if (result.MessageType == "conversation.item.input_audio_transcription.completed")
                {
                    if (doc.RootElement.TryGetProperty("transcript", out var transcriptElement))
                    {
                        result.UserTranscription = transcriptElement.GetString();
                    }
                }

                // Handle assistant response transcription
                if (result.MessageType == "response.audio_transcript.done")
                {
                    if (doc.RootElement.TryGetProperty("transcript", out var transcriptElement))
                    {
                        result.AgentTranscription = transcriptElement.GetString();
                    }
                }

                // Handle errors
                if (result.MessageType == "error")
                {
                    result.IsError = true;
                    if (doc.RootElement.TryGetProperty("error", out var errorElement))
                    {
                        if (errorElement.TryGetProperty("message", out var messageElement))
                        {
                            result.ErrorMessage = messageElement.GetString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing Azure OpenAI realtime message");
            }

            return result;
        }

        public byte[] TransformClientMessage(byte[] clientMessage)
        {
            // Azure OpenAI uses the same format, no transformation needed
            return clientMessage;
        }

        private string BuildWebSocketUrl(RealtimeSpeechAIModel model)
        {
            // Build URL for Azure OpenAI Realtime API
            // Format: wss://{resource}.openai.azure.com/openai/realtime?api-version={version}&deployment={deployment}
            var endpoint = model.Endpoint?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException("Endpoint is required for Azure OpenAI Realtime API");
            }
            
            var deployment = model.DeploymentName;
            if (string.IsNullOrWhiteSpace(deployment))
            {
                throw new InvalidOperationException($"DeploymentName is required for Azure OpenAI Realtime API. Model: {model.Name}, Endpoint: {endpoint}");
            }
            
            var apiVersion = model.Version ?? "2024-10-01-preview";
            
            // Convert HTTPS endpoint to WSS
            var baseUrl = endpoint.Replace("https://", "wss://").Replace("http://", "ws://");
            
            // Strip any trailing path segments that would cause duplication
            // Users sometimes configure the full realtime path instead of just the base URL
            if (baseUrl.EndsWith("/openai/realtime"))
            {
                baseUrl = baseUrl[..^16]; // Remove trailing /openai/realtime
                _logger.LogWarning("Endpoint contained /openai/realtime path - this should be removed from the configuration. Using base URL: {BaseUrl}", baseUrl);
            }
            else if (baseUrl.EndsWith("/openai"))
            {
                baseUrl = baseUrl[..^7]; // Remove trailing /openai
                _logger.LogWarning("Endpoint contained /openai path - this should be removed from the configuration. Using base URL: {BaseUrl}", baseUrl);
            }
            
            var url = $"{baseUrl}/openai/realtime?api-version={apiVersion}&deployment={deployment}";
            _logger.LogDebug("Built Azure OpenAI Realtime URL: {Url}", url);
            return url;
        }
    }
}
