# Realtime Speech-to-Speech Integration Plan for FoundationaLLM

## Executive Summary

This document outlines a comprehensive plan to integrate realtime speech-to-speech models (like GPT-Realtime) into FoundationaLLM. The integration enables voice-based conversations with AI agents through the Chat User Portal, with full transcription support and seamless integration into the existing conversation history.

**Reference Implementation:** This plan is based on the [FoundationaLLM Realtime Speech Prototype](https://github.com/foundationallm/realtime-speech-prototype/tree/cursor/realtime-agent-mcp-integration-1f31), which provides proven patterns for:
- Python/FastAPI backend with WebSocket proxy to Azure Realtime API
- JavaScript frontend with Web Audio API for microphone capture (24kHz, PCM16 format)
- Audio playback with queue management and interrupt handling
- Session configuration with voice, temperature, VAD settings
- MCP (Model Context Protocol) tool integration
- User/agent audio visualization with canvas-based waveforms
- Transcription display and conversation management
- Goodbye phrase detection for session termination

The prototype architecture will be adapted to FoundationaLLM's .NET/Vue.js stack while maintaining the same WebSocket message protocol and audio handling patterns.

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [AI Model Registry Updates](#2-ai-model-registry-updates)
3. [Agent Configuration Updates](#3-agent-configuration-updates)
4. [CoreAPI Updates](#4-coreapi-updates)
5. [User Portal UI Updates](#5-user-portal-ui-updates)
6. [WebSocket Communication Layer](#6-websocket-communication-layer)
7. [Transcription and Conversation Integration](#7-transcription-and-conversation-integration)
8. [Implementation Phases](#8-implementation-phases)
9. [Security Considerations](#9-security-considerations)
10. [Testing Strategy](#10-testing-strategy)

---

## Reference Implementation Patterns

The [realtime-speech-prototype](https://github.com/foundationallm/realtime-speech-prototype/tree/cursor/realtime-agent-mcp-integration-1f31) provides the authoritative patterns for this integration. All implementations should follow these patterns:

### Backend Patterns (from Python/FastAPI → adapt to .NET)

| Prototype File | Purpose | FoundationaLLM Equivalent |
|---------------|---------|--------------------------|
| `app.py` | FastAPI app with `/ws` WebSocket endpoint | `RealtimeSpeechController.cs` |
| `realtime_client.py` | Azure Realtime API WebSocket client | `RealtimeSpeechService.cs` |
| `websocket_handler.py` | Browser WebSocket message handling | Part of `RealtimeSpeechController.cs` |
| `conversation_manager.py` | State tracking, goodbye detection | Extend existing `ICoreService` |
| `config.py` | Configuration (endpoint, API key, MCP) | App settings / `AIModelBase` properties |

### Frontend Patterns (from JavaScript → adapt to TypeScript/Vue.js)

| Prototype Pattern | Description | FoundationaLLM Implementation |
|------------------|-------------|------------------------------|
| `VoiceAgentClient` class | Main client class in `app.js` | Vue composable `useRealtimeSpeech.ts` |
| `ScriptProcessorNode` | Audio capture at 24kHz | Same pattern in TypeScript |
| `nextPlayTime` queue | Smooth audio playback scheduling | Same pattern |
| Canvas visualization | Frequency spectrum display | Vue component with canvas |
| Session settings panel | Voice, temperature, VAD config | Vue component with form controls |

### WebSocket Message Protocol (use exactly as in prototype)

**Client → Server Messages:**
```javascript
{ type: 'start', settings: {...} }     // Start conversation with settings
{ type: 'stop' }                        // Stop conversation
{ type: 'audio', data: '<base64>' }     // Send audio chunk
{ type: 'interrupt' }                   // Interrupt agent response
{ type: 'update_settings', settings: {...} }  // Update settings mid-session
```

**Server → Client Messages:**
```javascript
{ type: 'status', status: 'connected' | 'disconnected' }
{ type: 'audio', data: '<base64>' }     // Agent audio chunk
{ type: 'transcript', text: '<delta>' } // Transcript delta (optional)
{ type: 'transcript_done', text: '...', speaker: 'user' | 'agent' }
{ type: 'tool_call', tool: '...', args: {...} }
{ type: 'tool_result', call_id: '...', result: '...' }
{ type: 'greeting_complete' }           // Agent greeting done, enable mic
{ type: 'response_done' }               // Agent response finished
{ type: 'interrupted' }                 // Response was interrupted
{ type: 'settings_applied' }            // Settings update confirmed
{ type: 'settings_error', message: '...' }
{ type: 'error', message: '...' }
```

### Azure Realtime API Protocol (from `realtime_client.py`)

**Session Configuration:**
```python
{
    "type": "session.update",
    "session": {
        "type": "realtime",
        "instructions": "<system prompt>",
        "temperature": 0.8,
        "turn_detection": {
            "type": "server_vad",
            "threshold": 0.5,
            "silence_duration_ms": 500,
            "prefix_padding_ms": 300
        },
        "input_audio_transcription": { "model": "whisper-1" },
        "tools": [{ "type": "mcp", "server_url": "...", "server_label": "..." }],
        "tool_choice": "auto"
    }
}
```

**Audio Streaming:**
```python
{ "type": "input_audio_buffer.append", "audio": "<base64 PCM16>" }
```

**Key Event Types to Handle:**
- `session.created` / `session.updated`
- `response.output_audio.delta` (audio data)
- `response.output_audio_transcript.delta` / `.done` (agent transcript)
- `conversation.item.input_audio_transcription.completed` (user transcript)
- `response.done` (response complete)
- `input_audio_buffer.speech_started` / `.speech_stopped`
- `response.function_call_arguments.done` (MCP tool call)
- `error`

### Audio Format Requirements

From the prototype's audio handling:
- **Sample Rate**: 24000 Hz (required by Azure Realtime API)
- **Format**: PCM16 (16-bit signed integers)
- **Channels**: Mono (1 channel)
- **Encoding**: Base64 for WebSocket transmission
- **Buffer Size**: 4096 samples per chunk (170ms at 24kHz)

### Session Settings (from prototype settings panel)

```typescript
interface SessionSettings {
    voice: 'alloy' | 'ash' | 'ballad' | 'coral' | 'echo' | 'sage' | 'shimmer' | 'verse';
    temperature: number;  // 0.6 - 1.2
    vadType: 'server_vad' | 'semantic_vad' | 'none';
    vadThreshold: number;  // 0.0 - 1.0
    silenceDuration: number;  // 200 - 1500 ms
    prefixPadding: number;  // 100 - 1000 ms
    eagerness: 'low' | 'medium' | 'high' | 'auto';
    maxTokens: 'inf' | number;
    transcriptionEnabled: boolean;
    customInstructions: string;
}
```

---

## 1. Architecture Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         User Portal (Vue.js/Nuxt)                        │
│  ┌─────────────┐  ┌──────────────────┐  ┌─────────────────────────────┐ │
│  │ Waveform    │  │ Audio Capture    │  │ Realtime Session Manager    │ │
│  │ Visualizer  │  │ (WebAudio API)   │  │ (WebSocket Client)          │ │
│  └─────────────┘  └──────────────────┘  └─────────────────────────────┘ │
└───────────────────────────────┬─────────────────────────────────────────┘
                                │ WebSocket (wss://)
                                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          CoreAPI (.NET)                                  │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                  RealtimeSpeechController                           ││
│  │  - WebSocket endpoint for realtime audio streaming                  ││
│  │  - Session management & lifecycle                                   ││
│  │  - Transcription handling                                           ││
│  └─────────────────────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                  RealtimeSpeechService                              ││
│  │  - GPT-Realtime API client                                          ││
│  │  - Audio format conversion                                          ││
│  │  - Conversation context management                                  ││
│  └─────────────────────────────────────────────────────────────────────┘│
└───────────────────────────────┬─────────────────────────────────────────┘
                                │ WebSocket / HTTP
                                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    Azure OpenAI GPT-Realtime API                         │
│  - Speech-to-speech with native voice                                    │
│  - Real-time audio streaming                                             │
│  - Built-in transcription                                                │
└─────────────────────────────────────────────────────────────────────────┘
```

### Key Components

Based on the [realtime-speech-prototype](https://github.com/foundationallm/realtime-speech-prototype):

1. **User Portal (Vue.js)**: Adapts the `VoiceAgentClient` JavaScript class from the prototype
   - Microphone capture with ScriptProcessorNode (24kHz, PCM16)
   - Audio playback with queue management for smooth streaming
   - Canvas-based frequency visualizations for user and agent audio
   - Session settings panel (voice, temperature, VAD configuration)
   - Interrupt detection and handling

2. **CoreAPI (.NET)**: Adapts the Python FastAPI backend pattern
   - `RealtimeSpeechController`: WebSocket endpoint (equivalent to `websocket_handler.py`)
   - `RealtimeSpeechService`: Azure Realtime API client (equivalent to `realtime_client.py`)
   - Session configuration with MCP tool support
   - Greeting flow management (prevents audio until agent greets)

3. **Azure OpenAI GPT-Realtime API**: The underlying realtime speech-to-speech service
   - WebSocket protocol with base64-encoded PCM16 audio
   - Server-side VAD (Voice Activity Detection)
   - Built-in transcription via Whisper-1
   - MCP (Model Context Protocol) tool integration

4. **Conversation Store**: FoundationaLLM's existing conversation persistence
   - Stores transcriptions as messages in the conversation history
   - Links realtime speech sessions to existing text conversations

---

## 2. AI Model Registry Updates

### 2.1 New AI Model Type: `realtime-speech`

Add a new AI model type to support realtime speech models.

#### File: `src/dotnet/Common/Constants/ResourceProviders/AIModelTypes.cs`

```csharp
namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    public static class AIModelTypes
    {
        // ... existing types ...
        
        /// <summary>
        /// Realtime speech-to-speech model type
        /// </summary>
        public const string RealtimeSpeech = "realtime-speech";
    }
}
```

#### File: `src/dotnet/Common/Models/ResourceProviders/AIModel/RealtimeSpeechAIModel.cs` (New)

```csharp
using FoundationaLLM.Common.Constants.ResourceProviders;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    /// <summary>
    /// Provides the properties for AI models used for realtime speech-to-speech.
    /// </summary>
    public class RealtimeSpeechAIModel : AIModelBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RealtimeSpeechAIModel"/> AI model.
        /// </summary>
        public RealtimeSpeechAIModel() =>
            Type = AIModelTypes.RealtimeSpeech;

        /// <summary>
        /// The voice to use for the model's audio responses.
        /// Options: alloy, echo, shimmer, etc.
        /// </summary>
        [JsonPropertyName("voice")]
        public string Voice { get; set; } = "alloy";

        /// <summary>
        /// The audio format for input. Default: pcm16
        /// </summary>
        [JsonPropertyName("input_audio_format")]
        public string InputAudioFormat { get; set; } = "pcm16";

        /// <summary>
        /// The audio format for output. Default: pcm16
        /// </summary>
        [JsonPropertyName("output_audio_format")]
        public string OutputAudioFormat { get; set; } = "pcm16";

        /// <summary>
        /// Turn detection configuration.
        /// </summary>
        [JsonPropertyName("turn_detection")]
        public TurnDetectionConfig? TurnDetection { get; set; }

        /// <summary>
        /// Whether input audio transcription is enabled.
        /// </summary>
        [JsonPropertyName("input_audio_transcription_enabled")]
        public bool InputAudioTranscriptionEnabled { get; set; } = true;

        /// <summary>
        /// The model to use for input audio transcription.
        /// </summary>
        [JsonPropertyName("input_audio_transcription_model")]
        public string? InputAudioTranscriptionModel { get; set; } = "whisper-1";
    }

    /// <summary>
    /// Configuration for voice activity detection / turn detection.
    /// </summary>
    public class TurnDetectionConfig
    {
        /// <summary>
        /// Type of turn detection: server_vad or none
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "server_vad";

        /// <summary>
        /// Activation threshold (0.0 to 1.0)
        /// </summary>
        [JsonPropertyName("threshold")]
        public double Threshold { get; set; } = 0.5;

        /// <summary>
        /// Audio to include before speech starts (ms)
        /// </summary>
        [JsonPropertyName("prefix_padding_ms")]
        public int PrefixPaddingMs { get; set; } = 300;

        /// <summary>
        /// Duration of silence to detect speech stop (ms)
        /// </summary>
        [JsonPropertyName("silence_duration_ms")]
        public int SilenceDurationMs { get; set; } = 500;
    }
}
```

### 2.2 Update AIModelBase Polymorphism

#### File: `src/dotnet/Common/Models/ResourceProviders/AIModel/AIModelBase.cs`

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EmbeddingAIModel), AIModelTypes.Embedding)]
[JsonDerivedType(typeof(CompletionAIModel), AIModelTypes.Completion)]
[JsonDerivedType(typeof(ImageGenerationAIModel), AIModelTypes.ImageGeneration)]
[JsonDerivedType(typeof(RealtimeSpeechAIModel), AIModelTypes.RealtimeSpeech)]  // NEW
public class AIModelBase : ResourceBase
{
    // ... existing code ...
}
```

### 2.3 Update Management Portal Model Types

#### File: `src/ui/ManagementPortal/pages/models/create.vue`

Add to `aiModelTypeOptions`:

```typescript
{
    label: 'Realtime Speech',
    value: 'realtime-speech',
}
```

Add additional configuration fields for realtime speech models (voice selection, turn detection settings, etc.)

---

## 3. Agent Configuration Updates

### 3.1 Add Realtime Speech Model Reference to Agent

#### File: `src/dotnet/Common/Models/ResourceProviders/Agent/AgentBase.cs`

```csharp
/// <summary>
/// The object identifier of the realtime speech AI model configured for this agent.
/// When set, enables speech-to-speech capabilities in the User Portal.
/// </summary>
[JsonPropertyName("realtime_speech_model_object_id")]
public string? RealtimeSpeechModelObjectId { get; set; }

/// <summary>
/// Configuration for realtime speech sessions.
/// </summary>
[JsonPropertyName("realtime_speech_settings")]
public RealtimeSpeechSettings? RealtimeSpeechSettings { get; set; }
```

#### File: `src/dotnet/Common/Models/ResourceProviders/Agent/RealtimeSpeechSettings.cs` (New)

```csharp
namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Configuration settings for realtime speech functionality.
    /// </summary>
    public class RealtimeSpeechSettings
    {
        /// <summary>
        /// Whether realtime speech is enabled for this agent.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Stop words that will terminate the realtime session.
        /// </summary>
        [JsonPropertyName("stop_words")]
        public List<string> StopWords { get; set; } = new() { "stop", "end conversation", "goodbye" };

        /// <summary>
        /// Maximum duration of a realtime session in seconds (0 = unlimited).
        /// </summary>
        [JsonPropertyName("max_session_duration_seconds")]
        public int MaxSessionDurationSeconds { get; set; } = 0;

        /// <summary>
        /// Whether to show transcriptions in the chat thread.
        /// </summary>
        [JsonPropertyName("show_transcriptions")]
        public bool ShowTranscriptions { get; set; } = true;

        /// <summary>
        /// Whether to include conversation history in the realtime session context.
        /// </summary>
        [JsonPropertyName("include_conversation_history")]
        public bool IncludeConversationHistory { get; set; } = true;
    }
}
```

### 3.2 Update Agent Response with Speech Capabilities

The agent response to the User Portal should indicate whether realtime speech is available.

#### File: `src/dotnet/Common/Models/ResourceProviders/Agent/AgentBase.cs`

Add a computed property or ensure the `RealtimeSpeechSettings` is included in serialization:

```csharp
/// <summary>
/// Indicates whether realtime speech capabilities are available for this agent.
/// </summary>
[JsonIgnore]
public bool HasRealtimeSpeechCapabilities =>
    !string.IsNullOrWhiteSpace(RealtimeSpeechModelObjectId) &&
    RealtimeSpeechSettings?.Enabled == true;
```

### 3.3 Update Management Portal Agent Configuration

#### File: `src/ui/ManagementPortal/pages/agents/create.vue`

Add a new section for "Realtime Speech Configuration":

```vue
<!-- Realtime Speech Configuration -->
<section aria-labelledby="realtime-speech-config" class="col-span-2 steps">
    <h3 id="realtime-speech-config" class="step-section-header col-span-2">
        Realtime Speech Configuration
    </h3>

    <div id="aria-realtime-speech-enabled" class="step-header">
        Enable realtime speech-to-speech?
    </div>
    <div>
        <ToggleButton
            v-model="realtimeSpeechEnabled"
            on-label="Yes"
            on-icon="pi pi-check-circle"
            off-label="No"
            off-icon="pi pi-times-circle"
            aria-labelledby="aria-realtime-speech-enabled"
        />
    </div>

    <template v-if="realtimeSpeechEnabled">
        <div class="step-header col-span-2">
            Which realtime speech model should be used?
        </div>
        <div class="col-span-2">
            <Dropdown
                v-model="realtimeSpeechModelObjectId"
                :options="realtimeSpeechModelOptions"
                option-label="name"
                option-value="object_id"
                class="dropdown--agent"
                placeholder="--Select--"
            />
        </div>

        <div class="step-header col-span-2">
            Stop words (comma-separated):
        </div>
        <div class="col-span-2">
            <InputText
                v-model="realtimeSpeechStopWords"
                type="text"
                class="w-full"
                placeholder="stop, end conversation, goodbye"
            />
        </div>
    </template>
</section>
```

---

## 4. CoreAPI Updates

The CoreAPI updates adapt the Python backend from the [realtime-speech-prototype](https://github.com/foundationallm/realtime-speech-prototype):

| Prototype Component | .NET Implementation |
|--------------------|---------------------|
| `app.py` WebSocket endpoint `/ws` | `RealtimeSpeechController` endpoint `/sessions/{sessionId}/realtime-speech` |
| `websocket_handler.py` `WebSocketHandler` class | Integrated into `RealtimeSpeechController` |
| `realtime_client.py` `RealtimeClient` class | `RealtimeSpeechService` service |
| `conversation_manager.py` | Leverage existing `ICoreService` conversation methods |

### 4.1 New Controller: RealtimeSpeechController

Adapts the WebSocket handling from `websocket_handler.py` in the prototype.

#### File: `src/dotnet/CoreAPI/Controllers/RealtimeSpeechController.cs` (New)

```csharp
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides WebSocket endpoints for realtime speech-to-speech communication.
    /// </summary>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [ApiController]
    [Route("instances/{instanceId}")]
    public class RealtimeSpeechController : ControllerBase
    {
        private readonly IRealtimeSpeechService _realtimeSpeechService;
        private readonly ICoreService _coreService;
        private readonly ILogger<RealtimeSpeechController> _logger;

        public RealtimeSpeechController(
            IRealtimeSpeechService realtimeSpeechService,
            ICoreService coreService,
            ILogger<RealtimeSpeechController> logger)
        {
            _realtimeSpeechService = realtimeSpeechService;
            _coreService = coreService;
            _logger = logger;
        }

        /// <summary>
        /// Establishes a WebSocket connection for realtime speech communication.
        /// </summary>
        [HttpGet("sessions/{sessionId}/realtime-speech")]
        public async Task<IActionResult> ConnectRealtimeSpeech(
            string instanceId,
            string sessionId,
            [FromQuery] string agentName)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                return BadRequest("WebSocket connection required");
            }

            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            
            try
            {
                await _realtimeSpeechService.HandleWebSocketConnection(
                    instanceId,
                    sessionId,
                    agentName,
                    webSocket,
                    HttpContext.RequestAborted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in realtime speech WebSocket connection");
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.InternalServerError,
                        "Internal error",
                        CancellationToken.None);
                }
            }

            return new EmptyResult();
        }

        /// <summary>
        /// Gets the realtime speech configuration for an agent.
        /// </summary>
        [HttpGet("agents/{agentName}/realtime-speech/config")]
        public async Task<IActionResult> GetRealtimeSpeechConfig(
            string instanceId,
            string agentName)
        {
            var config = await _realtimeSpeechService.GetConfigurationAsync(
                instanceId,
                agentName);
            
            return Ok(config);
        }
    }
}
```

### 4.2 New Service: IRealtimeSpeechService

#### File: `src/dotnet/Core/Interfaces/IRealtimeSpeechService.cs` (New)

```csharp
using System.Net.WebSockets;
using FoundationaLLM.Common.Models.RealtimeSpeech;

namespace FoundationaLLM.Core.Interfaces
{
    /// <summary>
    /// Service for handling realtime speech-to-speech communication.
    /// </summary>
    public interface IRealtimeSpeechService
    {
        /// <summary>
        /// Handles a WebSocket connection for realtime speech.
        /// </summary>
        Task HandleWebSocketConnection(
            string instanceId,
            string sessionId,
            string agentName,
            WebSocket webSocket,
            CancellationToken cancellationToken);

        /// <summary>
        /// Gets the realtime speech configuration for an agent.
        /// </summary>
        Task<RealtimeSpeechConfiguration> GetConfigurationAsync(
            string instanceId,
            string agentName);
    }
}
```

### 4.3 RealtimeSpeechService Implementation

This service adapts the `RealtimeClient` class from `realtime_client.py` in the prototype. Key patterns:

1. **WebSocket URL construction**: Handle both GA and preview API versions (see `_build_websocket_url`)
2. **Session configuration**: Send `session.update` event with instructions, VAD, transcription, and MCP tools
3. **Event handling**: Process all Azure Realtime API event types (see `_handle_message`)
4. **Greeting flow**: Track `greeting_sent` to prevent duplicate greetings
5. **MCP tool support**: Configure MCP servers via the `tools` array in session config

#### File: `src/dotnet/Core/Services/RealtimeSpeechService.cs` (New)

```csharp
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.RealtimeSpeech;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Core.Interfaces;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// Service for handling realtime speech-to-speech communication with GPT-Realtime API.
    /// Adapted from realtime_client.py in the realtime-speech-prototype.
    /// </summary>
    public class RealtimeSpeechService : IRealtimeSpeechService
    {
        private readonly IResourceProviderService _agentResourceProvider;
        private readonly IResourceProviderService _aiModelResourceProvider;
        private readonly IResourceProviderService _promptResourceProvider;
        private readonly IConversationService _conversationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RealtimeSpeechService> _logger;

        public RealtimeSpeechService(
            IEnumerable<IResourceProviderService> resourceProviderServices,
            IConversationService conversationService,
            IHttpClientFactory httpClientFactory,
            ILogger<RealtimeSpeechService> logger)
        {
            var providers = resourceProviderServices.ToDictionary(rps => rps.Name);
            _agentResourceProvider = providers["FoundationaLLM_Agent"];
            _aiModelResourceProvider = providers["FoundationaLLM_AIModel"];
            _promptResourceProvider = providers["FoundationaLLM_Prompt"];
            _conversationService = conversationService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task HandleWebSocketConnection(
            string instanceId,
            string sessionId,
            string agentName,
            WebSocket clientWebSocket,
            CancellationToken cancellationToken)
        {
            // 1. Load agent and its realtime speech configuration
            var agent = await GetAgentAsync(instanceId, agentName);
            if (agent?.RealtimeSpeechModelObjectId == null)
            {
                await CloseWithError(clientWebSocket, "Agent does not support realtime speech");
                return;
            }

            // 2. Load the realtime speech model
            var realtimeModel = await GetRealtimeSpeechModelAsync(
                instanceId,
                agent.RealtimeSpeechModelObjectId);

            // 3. Get agent's main prompt for instructions
            var mainPrompt = await GetAgentMainPromptAsync(instanceId, agent);

            // 4. Get conversation history if enabled
            List<Message>? conversationHistory = null;
            if (agent.RealtimeSpeechSettings?.IncludeConversationHistory == true)
            {
                conversationHistory = await _conversationService.GetMessagesAsync(
                    instanceId,
                    sessionId);
            }

            // 5. Connect to GPT-Realtime API
            using var gptWebSocket = await ConnectToGptRealtimeAsync(realtimeModel);

            // 6. Send session configuration with instructions
            await SendSessionUpdateAsync(
                gptWebSocket,
                realtimeModel,
                mainPrompt,
                conversationHistory);

            // 7. Start bidirectional message proxying
            await ProxyWebSocketMessagesAsync(
                clientWebSocket,
                gptWebSocket,
                instanceId,
                sessionId,
                agent,
                cancellationToken);
        }

        private async Task SendSessionUpdateAsync(
            ClientWebSocket gptWebSocket,
            RealtimeSpeechAIModel model,
            string? instructions,
            List<Message>? conversationHistory)
        {
            var sessionUpdate = new
            {
                type = "session.update",
                session = new
                {
                    modalities = new[] { "text", "audio" },
                    instructions = BuildInstructions(instructions, conversationHistory),
                    voice = model.Voice,
                    input_audio_format = model.InputAudioFormat,
                    output_audio_format = model.OutputAudioFormat,
                    input_audio_transcription = new
                    {
                        model = model.InputAudioTranscriptionModel ?? "whisper-1"
                    },
                    turn_detection = model.TurnDetection != null ? new
                    {
                        type = model.TurnDetection.Type,
                        threshold = model.TurnDetection.Threshold,
                        prefix_padding_ms = model.TurnDetection.PrefixPaddingMs,
                        silence_duration_ms = model.TurnDetection.SilenceDurationMs
                    } : null
                }
            };

            var json = JsonSerializer.Serialize(sessionUpdate);
            var bytes = Encoding.UTF8.GetBytes(json);
            await gptWebSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
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
            ClientWebSocket gptWebSocket,
            string instanceId,
            string sessionId,
            AgentBase agent,
            CancellationToken cancellationToken)
        {
            var clientToGpt = ProxyMessages(
                clientWebSocket,
                gptWebSocket,
                "Client->GPT",
                cancellationToken);

            var gptToClient = ProxyMessagesWithTranscriptionCapture(
                gptWebSocket,
                clientWebSocket,
                instanceId,
                sessionId,
                agent,
                cancellationToken);

            await Task.WhenAny(clientToGpt, gptToClient);
        }

        private async Task ProxyMessagesWithTranscriptionCapture(
            ClientWebSocket source,
            WebSocket destination,
            string instanceId,
            string sessionId,
            AgentBase agent,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];
            var messageBuilder = new List<byte>();

            while (!cancellationToken.IsCancellationRequested &&
                   source.State == WebSocketState.Open)
            {
                var result = await source.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                messageBuilder.AddRange(buffer.Take(result.Count));

                if (result.EndOfMessage)
                {
                    var message = messageBuilder.ToArray();
                    messageBuilder.Clear();

                    // Check for transcription events and save to conversation
                    await HandleGptMessageAsync(
                        message,
                        instanceId,
                        sessionId,
                        agent);

                    // Forward to client
                    await destination.SendAsync(
                        new ArraySegment<byte>(message),
                        result.MessageType,
                        true,
                        cancellationToken);
                }
            }
        }

        private async Task HandleGptMessageAsync(
            byte[] message,
            string instanceId,
            string sessionId,
            AgentBase agent)
        {
            try
            {
                var json = Encoding.UTF8.GetString(message);
                var doc = JsonDocument.Parse(json);
                var type = doc.RootElement.GetProperty("type").GetString();

                // Handle user transcription completed
                if (type == "conversation.item.input_audio_transcription.completed")
                {
                    var transcript = doc.RootElement
                        .GetProperty("transcript")
                        .GetString();

                    if (!string.IsNullOrWhiteSpace(transcript))
                    {
                        await SaveTranscriptionToConversation(
                            instanceId,
                            sessionId,
                            transcript,
                            "User",
                            agent);
                    }
                }

                // Handle assistant response transcription
                if (type == "response.audio_transcript.done")
                {
                    var transcript = doc.RootElement
                        .GetProperty("transcript")
                        .GetString();

                    if (!string.IsNullOrWhiteSpace(transcript))
                    {
                        await SaveTranscriptionToConversation(
                            instanceId,
                            sessionId,
                            transcript,
                            "Agent",
                            agent);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing GPT realtime message");
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

            await _conversationService.AddMessageAsync(instanceId, sessionId, message);
        }

        // ... additional helper methods ...
    }
}
```

### 4.4 Message Models for Realtime Speech

#### File: `src/dotnet/Common/Models/RealtimeSpeech/RealtimeSpeechConfiguration.cs` (New)

```csharp
namespace FoundationaLLM.Common.Models.RealtimeSpeech
{
    /// <summary>
    /// Configuration returned to the client for realtime speech capabilities.
    /// </summary>
    public class RealtimeSpeechConfiguration
    {
        /// <summary>
        /// Whether realtime speech is enabled for the agent.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Stop words that will terminate the session.
        /// </summary>
        [JsonPropertyName("stop_words")]
        public List<string> StopWords { get; set; } = new();

        /// <summary>
        /// The WebSocket endpoint URL for establishing a realtime connection.
        /// </summary>
        [JsonPropertyName("websocket_url")]
        public string? WebSocketUrl { get; set; }

        /// <summary>
        /// Voice identifier being used.
        /// </summary>
        [JsonPropertyName("voice")]
        public string? Voice { get; set; }
    }
}
```

### 4.5 WebSocket Middleware Configuration

#### File: `src/dotnet/CoreAPI/Program.cs`

Add WebSocket support:

```csharp
// Add WebSocket support
builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
});

// In the middleware pipeline:
app.UseWebSockets();
```

---

## 5. User Portal UI Updates

The User Portal updates adapt the JavaScript frontend from the [realtime-speech-prototype](https://github.com/foundationallm/realtime-speech-prototype):

| Prototype Component | Vue.js Implementation |
|--------------------|----------------------|
| `static/app.js` `VoiceAgentClient` class | `useRealtimeSpeech.ts` composable |
| Audio capture with `ScriptProcessorNode` | Same pattern in TypeScript |
| Canvas-based frequency visualization | `RealtimeSpeechButton.vue` with canvas |
| `static/index.html` settings panel | Settings integrated into component |
| `static/styles.css` | Scoped styles in Vue component |

### 5.1 New Component: RealtimeSpeechButton

#### File: `src/ui/UserPortal/components/RealtimeSpeechButton.vue` (New)

```vue
<template>
    <div class="realtime-speech-container">
        <!-- Waveform Visualizer (shown when active) -->
        <div v-if="isActive" class="waveform-container">
            <canvas ref="userWaveformCanvas" class="waveform user-waveform"></canvas>
            <canvas ref="aiWaveformCanvas" class="waveform ai-waveform"></canvas>
            <div class="waveform-labels">
                <span class="user-label">You</span>
                <span class="ai-label">AI</span>
            </div>
        </div>

        <!-- Speech Button -->
        <VTooltip :popper-triggers="['hover']">
            <Button
                :class="['realtime-speech-button', { active: isActive }]"
                :icon="buttonIcon"
                :disabled="!isSupported || isConnecting"
                :loading="isConnecting"
                aria-label="Toggle voice conversation"
                @click="toggleRealtimeSpeech"
            />
            <template #popper>
                <div role="tooltip">
                    {{ tooltipText }}
                </div>
            </template>
        </VTooltip>
    </div>
</template>

<script lang="ts">
import { useRealtimeSpeech } from '@/composables/useRealtimeSpeech';

export default {
    name: 'RealtimeSpeechButton',

    props: {
        agentName: {
            type: String,
            required: true,
        },
        sessionId: {
            type: String,
            required: true,
        },
    },

    emits: ['transcription', 'status-change'],

    setup(props, { emit }) {
        const {
            isActive,
            isConnecting,
            isSupported,
            userAudioLevel,
            aiAudioLevel,
            connect,
            disconnect,
            error,
        } = useRealtimeSpeech({
            agentName: props.agentName,
            sessionId: props.sessionId,
            onTranscription: (text, sender) => {
                emit('transcription', { text, sender });
            },
            onStatusChange: (status) => {
                emit('status-change', status);
            },
        });

        return {
            isActive,
            isConnecting,
            isSupported,
            userAudioLevel,
            aiAudioLevel,
            connect,
            disconnect,
            error,
        };
    },

    computed: {
        buttonIcon() {
            if (this.isActive) {
                return 'pi pi-stop-circle';
            }
            return 'pi pi-microphone';
        },

        tooltipText() {
            if (!this.isSupported) {
                return 'Voice conversations are not supported in this browser';
            }
            if (this.isConnecting) {
                return 'Connecting...';
            }
            if (this.isActive) {
                return 'Click or say "stop" to end voice conversation';
            }
            return 'Start voice conversation';
        },
    },

    watch: {
        userAudioLevel: 'drawUserWaveform',
        aiAudioLevel: 'drawAiWaveform',
    },

    methods: {
        async toggleRealtimeSpeech() {
            if (this.isActive) {
                await this.disconnect();
            } else {
                await this.connect();
            }
        },

        drawUserWaveform() {
            this.drawWaveform(this.$refs.userWaveformCanvas, this.userAudioLevel, '#4CAF50');
        },

        drawAiWaveform() {
            this.drawWaveform(this.$refs.aiWaveformCanvas, this.aiAudioLevel, '#2196F3');
        },

        drawWaveform(canvas: HTMLCanvasElement, level: number, color: string) {
            if (!canvas) return;
            
            const ctx = canvas.getContext('2d');
            const width = canvas.width;
            const height = canvas.height;
            
            ctx.clearRect(0, 0, width, height);
            ctx.fillStyle = color;
            
            // Draw waveform bars
            const barCount = 20;
            const barWidth = width / barCount - 2;
            
            for (let i = 0; i < barCount; i++) {
                const barHeight = Math.random() * level * height;
                const x = i * (barWidth + 2);
                const y = (height - barHeight) / 2;
                ctx.fillRect(x, y, barWidth, barHeight);
            }
        },
    },
};
</script>

<style lang="scss" scoped>
.realtime-speech-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
}

.waveform-container {
    display: flex;
    gap: 16px;
    padding: 8px 16px;
    background: rgba(0, 0, 0, 0.05);
    border-radius: 8px;
    margin-bottom: 8px;
}

.waveform {
    width: 100px;
    height: 40px;
    border-radius: 4px;
}

.user-waveform {
    background: rgba(76, 175, 80, 0.1);
}

.ai-waveform {
    background: rgba(33, 150, 243, 0.1);
}

.waveform-labels {
    display: flex;
    gap: 16px;
    font-size: 0.75rem;
    color: #666;
}

.realtime-speech-button {
    width: 48px;
    height: 48px;
    border-radius: 50%;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border: none;
    color: white;
    transition: all 0.3s ease;

    &:hover:not(:disabled) {
        transform: scale(1.1);
        box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
    }

    &.active {
        background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
        animation: pulse 2s infinite;
    }

    &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }
}

@keyframes pulse {
    0%, 100% {
        box-shadow: 0 0 0 0 rgba(245, 87, 108, 0.4);
    }
    50% {
        box-shadow: 0 0 0 10px rgba(245, 87, 108, 0);
    }
}
</style>
```

### 5.2 VoiceAgentClient Class (Based on realtime-speech-prototype)

#### File: `src/ui/UserPortal/js/voiceAgentClient.ts` (New)

This implementation is adapted from the `VoiceAgentClient` class in the [realtime-speech-prototype](https://github.com/foundationallm/realtime-speech-prototype) `static/app.js`.

Key patterns from the prototype:
- Uses `ScriptProcessorNode` for audio capture (with AudioWorklet as future enhancement)
- Queued audio playback with `nextPlayTime` for smooth streaming
- Interrupt detection based on user speech level threshold
- Canvas-based frequency visualization
- Session settings management (voice, temperature, VAD)

```typescript
/**
 * AudioHandler manages audio recording and playback for realtime speech.
 * Based on Azure OpenAI Realtime Audio SDK patterns.
 */
export class AudioHandler {
    private context: AudioContext;
    private workletNode: AudioWorkletNode | null = null;
    private stream: MediaStream | null = null;
    private source: MediaStreamAudioSourceNode | null = null;
    private readonly sampleRate = 24000; // Required by GPT-Realtime API

    // Playback scheduling for smooth audio
    private nextPlayTime: number = 0;
    private isPlaying: boolean = false;
    private playbackQueue: AudioBufferSourceNode[] = [];

    constructor() {
        this.context = new AudioContext({ sampleRate: this.sampleRate });
    }

    async initialize() {
        // Load the audio worklet processor
        await this.context.audioWorklet.addModule('/audio-processor.js');
    }

    /**
     * Start recording audio from the microphone.
     * @param onChunk Callback for each audio chunk (PCM16 as Uint8Array)
     */
    async startRecording(onChunk: (chunk: Uint8Array) => void) {
        try {
            if (!this.workletNode) {
                await this.initialize();
            }

            // Request microphone with optimal settings for realtime speech
            this.stream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    channelCount: 1,
                    sampleRate: this.sampleRate,
                    echoCancellation: true,
                    noiseSuppression: true,
                },
            });

            await this.context.resume();
            this.source = this.context.createMediaStreamSource(this.stream);
            this.workletNode = new AudioWorkletNode(
                this.context,
                'audio-recorder-processor',
            );

            this.workletNode.port.onmessage = (event) => {
                if (event.data.eventType === 'audio') {
                    const float32Data = event.data.audioData;
                    // Convert Float32 to Int16 (PCM16 format required by API)
                    const int16Data = new Int16Array(float32Data.length);
                    for (let i = 0; i < float32Data.length; i++) {
                        const s = Math.max(-1, Math.min(1, float32Data[i]));
                        int16Data[i] = s < 0 ? s * 0x8000 : s * 0x7fff;
                    }
                    const uint8Data = new Uint8Array(int16Data.buffer);
                    onChunk(uint8Data);
                }
            };

            this.source.connect(this.workletNode);
            this.workletNode.connect(this.context.destination);
            this.workletNode.port.postMessage({ command: 'START_RECORDING' });
        } catch (error) {
            console.error('Error starting recording:', error);
            throw error;
        }
    }

    stopRecording() {
        if (!this.workletNode || !this.source || !this.stream) {
            return;
        }
        this.workletNode.port.postMessage({ command: 'STOP_RECORDING' });
        this.workletNode.disconnect();
        this.source.disconnect();
        this.stream.getTracks().forEach((track) => track.stop());
    }

    /**
     * Start streaming playback mode - resets playback timing.
     */
    startStreamingPlayback() {
        this.isPlaying = true;
        this.nextPlayTime = this.context.currentTime;
    }

    /**
     * Stop streaming playback and clear the queue.
     */
    stopStreamingPlayback() {
        this.isPlaying = false;
        this.playbackQueue.forEach((source) => source.stop());
        this.playbackQueue = [];
    }

    /**
     * Play an audio chunk. Chunks are scheduled sequentially for smooth playback.
     * @param chunk PCM16 audio data as Uint8Array
     */
    playChunk(chunk: Uint8Array) {
        if (!this.isPlaying) return;

        // Convert Int16 (PCM16) to Float32 for Web Audio API
        const int16Data = new Int16Array(chunk.buffer);
        const float32Data = new Float32Array(int16Data.length);
        for (let i = 0; i < int16Data.length; i++) {
            float32Data[i] = int16Data[i] / (int16Data[i] < 0 ? 0x8000 : 0x7fff);
        }

        // Create audio buffer and source
        const audioBuffer = this.context.createBuffer(
            1,
            float32Data.length,
            this.sampleRate,
        );
        audioBuffer.getChannelData(0).set(float32Data);

        const source = this.context.createBufferSource();
        source.buffer = audioBuffer;
        source.connect(this.context.destination);

        // Schedule playback for smooth audio
        const chunkDuration = audioBuffer.length / this.sampleRate;
        source.start(this.nextPlayTime);

        // Track source for cleanup
        this.playbackQueue.push(source);
        source.onended = () => {
            const index = this.playbackQueue.indexOf(source);
            if (index > -1) {
                this.playbackQueue.splice(index, 1);
            }
        };

        this.nextPlayTime += chunkDuration;

        // Handle timing drift
        if (this.nextPlayTime < this.context.currentTime) {
            this.nextPlayTime = this.context.currentTime;
        }
    }

    /**
     * Clear the playback buffer (e.g., when user starts speaking).
     */
    clearPlayback() {
        this.stopStreamingPlayback();
        this.nextPlayTime = this.context.currentTime;
    }

    async close() {
        this.workletNode?.disconnect();
        this.source?.disconnect();
        this.stream?.getTracks().forEach((track) => track.stop());
        await this.context.close();
    }
}
```

### 5.3 Audio Worklet Processor

#### File: `src/ui/UserPortal/public/audio-processor.js` (New)

```javascript
/**
 * AudioWorklet processor for recording audio at 24kHz.
 * Converts Float32 samples to format suitable for GPT-Realtime API.
 */
class AudioRecorderProcessor extends AudioWorkletProcessor {
    constructor() {
        super();
        this.isRecording = false;
        this.port.onmessage = (event) => {
            if (event.data.command === 'START_RECORDING') {
                this.isRecording = true;
            } else if (event.data.command === 'STOP_RECORDING') {
                this.isRecording = false;
            }
        };
    }

    process(inputs, outputs, parameters) {
        const input = inputs[0];
        if (this.isRecording && input.length > 0) {
            const audioData = input[0];
            // Send audio data to main thread
            this.port.postMessage({
                eventType: 'audio',
                audioData: audioData,
            });
        }
        return true;
    }
}

registerProcessor('audio-recorder-processor', AudioRecorderProcessor);
```

### 5.4 Composable: useRealtimeSpeech

This composable adapts the `VoiceAgentClient` class from the prototype's `static/app.js`. Key patterns to preserve:

1. **Microphone gating**: Don't send audio until `greeting_complete` is received (prevents interrupting the agent's greeting)
2. **Audio queue management**: Use `nextPlayTime` for smooth playback scheduling
3. **Interrupt detection**: Track `isAgentSpeaking` and detect sustained user speech above threshold
4. **Session settings**: Support the full range of settings from the prototype

#### File: `src/ui/UserPortal/composables/useRealtimeSpeech.ts` (New)

```typescript
import { ref, onUnmounted } from 'vue';
import api from '@/js/api';

// Adapted from VoiceAgentClient in realtime-speech-prototype/static/app.js

interface UseRealtimeSpeechOptions {
    agentName: string;
    sessionId: string;
    onTranscription?: (text: string, sender: 'User' | 'AI') => void;
    onStatusChange?: (status: 'connecting' | 'connected' | 'disconnected' | 'error') => void;
}

export function useRealtimeSpeech(options: UseRealtimeSpeechOptions) {
    const isActive = ref(false);
    const isConnecting = ref(false);
    const isSupported = ref(typeof AudioContext !== 'undefined' && 'mediaDevices' in navigator);
    const userAudioLevel = ref(0);
    const aiAudioLevel = ref(0);
    const error = ref<string | null>(null);

    let websocket: WebSocket | null = null;
    let audioHandler: AudioHandler | null = null;
    let recordingActive = false;

    async function connect() {
        if (isActive.value || isConnecting.value) return;

        try {
            isConnecting.value = true;
            error.value = null;
            options.onStatusChange?.('connecting');

            // Initialize audio handler
            audioHandler = new AudioHandler();
            await audioHandler.initialize();

            // Get WebSocket URL and connect
            const bearerToken = await api.getBearerToken();
            const wsProtocol = api.getApiUrl()?.startsWith('https') ? 'wss' : 'ws';
            const wsHost = api.getApiUrl()?.replace(/^https?:\/\//, '');
            const wsUrl = `${wsProtocol}://${wsHost}/instances/${api.instanceId}/sessions/${options.sessionId}/realtime-speech?agentName=${options.agentName}&access_token=${bearerToken}`;

            websocket = new WebSocket(wsUrl);

            websocket.onopen = async () => {
                isConnecting.value = false;
                isActive.value = true;
                options.onStatusChange?.('connected');

                // Start recording and streaming audio
                await startAudioCapture();
            };

            websocket.onmessage = handleWebSocketMessage;

            websocket.onerror = (event) => {
                console.error('WebSocket error:', event);
                error.value = 'Connection error';
                disconnect();
            };

            websocket.onclose = () => {
                isActive.value = false;
                options.onStatusChange?.('disconnected');
            };

        } catch (err) {
            console.error('Failed to connect:', err);
            error.value = err instanceof Error ? err.message : 'Connection failed';
            isConnecting.value = false;
            options.onStatusChange?.('error');
        }
    }

    async function startAudioCapture() {
        if (!audioHandler || !websocket) return;

        recordingActive = true;
        await audioHandler.startRecording((chunk: Uint8Array) => {
            if (!recordingActive || websocket?.readyState !== WebSocket.OPEN) return;

            // Calculate audio level for visualization
            const int16Data = new Int16Array(chunk.buffer);
            let sum = 0;
            for (let i = 0; i < int16Data.length; i++) {
                const normalized = int16Data[i] / 32768;
                sum += normalized * normalized;
            }
            userAudioLevel.value = Math.sqrt(sum / int16Data.length);

            // Send audio as base64-encoded PCM16
            const base64Audio = btoa(String.fromCharCode(...chunk));
            websocket.send(JSON.stringify({
                type: 'input_audio_buffer.append',
                audio: base64Audio,
            }));
        });
    }

    function handleWebSocketMessage(event: MessageEvent) {
        try {
            const message = JSON.parse(event.data);

            switch (message.type) {
                case 'session.created':
                    console.log('Realtime session created');
                    break;

                case 'input_audio_buffer.speech_started':
                    // User started speaking - stop AI playback
                    audioHandler?.stopStreamingPlayback();
                    break;

                case 'response.audio.delta':
                    // Receive AI audio chunk
                    if (!audioHandler) break;
                    const audioData = Uint8Array.from(
                        atob(message.delta),
                        (c) => c.charCodeAt(0)
                    );
                    
                    // Start playback on first chunk
                    if (message.content_index === 0 && message.output_index === 0) {
                        audioHandler.startStreamingPlayback();
                    }
                    
                    audioHandler.playChunk(audioData);

                    // Calculate AI audio level for visualization
                    const int16Data = new Int16Array(audioData.buffer);
                    let sum = 0;
                    for (let i = 0; i < int16Data.length; i++) {
                        const normalized = int16Data[i] / 32768;
                        sum += normalized * normalized;
                    }
                    aiAudioLevel.value = Math.sqrt(sum / int16Data.length);
                    break;

                case 'conversation.item.input_audio_transcription.completed':
                    // User speech transcription completed
                    options.onTranscription?.(message.transcript, 'User');
                    break;

                case 'response.audio_transcript.delta':
                    // AI response transcription (streaming)
                    // Could be used for real-time display
                    break;

                case 'response.audio_transcript.done':
                    // AI response transcription completed
                    options.onTranscription?.(message.transcript, 'AI');
                    break;

                case 'response.done':
                    // Response completed
                    aiAudioLevel.value = 0;
                    break;

                case 'error':
                    console.error('Realtime API error:', message.error);
                    error.value = message.error?.message || 'API error';
                    break;
            }
        } catch (err) {
            console.error('Failed to parse message:', err);
        }
    }

    async function disconnect() {
        recordingActive = false;

        if (audioHandler) {
            audioHandler.stopRecording();
            audioHandler.stopStreamingPlayback();
            await audioHandler.close();
            audioHandler = null;
        }

        if (websocket) {
            websocket.close();
            websocket = null;
        }

        isActive.value = false;
        isConnecting.value = false;
        userAudioLevel.value = 0;
        aiAudioLevel.value = 0;
    }

    onUnmounted(() => {
        disconnect();
    });

    return {
        isActive,
        isConnecting,
        isSupported,
        userAudioLevel,
        aiAudioLevel,
        error,
        connect,
        disconnect,
    };
}
```

### 5.3 Update ChatInput Component

#### File: `src/ui/UserPortal/components/ChatInput.vue`

Add the RealtimeSpeechButton next to the existing input controls:

```vue
<!-- Add after file upload button, before textarea -->
<RealtimeSpeechButton
    v-if="showRealtimeSpeech"
    :agent-name="currentAgent?.name"
    :session-id="$appStore.currentSession.sessionId"
    @transcription="handleTranscription"
    @status-change="handleRealtimeStatusChange"
/>
```

Add computed property:

```typescript
showRealtimeSpeech() {
    const agent = this.$appStore.lastSelectedAgent?.resource;
    return agent?.realtime_speech_settings?.enabled === true;
}
```

### 5.4 Update TypeScript Types

#### File: `src/ui/UserPortal/js/types/index.ts`

```typescript
// Add to AgentBase interface
export interface AgentBase extends ResourceBase {
    // ... existing properties ...
    
    /**
     * Object ID of the realtime speech AI model.
     */
    realtime_speech_model_object_id?: string | null;

    /**
     * Realtime speech configuration settings.
     */
    realtime_speech_settings?: RealtimeSpeechSettings | null;
}

export interface RealtimeSpeechSettings {
    enabled: boolean;
    stop_words: string[];
    max_session_duration_seconds: number;
    show_transcriptions: boolean;
    include_conversation_history: boolean;
}

export interface RealtimeSpeechConfiguration {
    enabled: boolean;
    stop_words: string[];
    websocket_url: string;
    voice: string;
}
```

### 5.5 Update API Module

#### File: `src/ui/UserPortal/js/api.ts`

```typescript
/**
 * Gets the realtime speech configuration for an agent.
 * @param agentName The agent name.
 * @returns The realtime speech configuration.
 */
async getRealtimeSpeechConfig(agentName: string): Promise<RealtimeSpeechConfiguration> {
    return await this.fetch<RealtimeSpeechConfiguration>(
        `/instances/${this.instanceId}/agents/${agentName}/realtime-speech/config`
    );
}
```

---

## 6. WebSocket Communication Layer

### 6.1 Message Protocol

The communication between the User Portal and CoreAPI uses the following message types:

#### Client to Server Messages:

```typescript
// Audio data from microphone
{
    type: 'input_audio_buffer.append',
    audio: string  // Base64 encoded PCM16 audio
}

// Commit audio buffer (trigger response)
{
    type: 'input_audio_buffer.commit'
}

// Clear audio buffer
{
    type: 'input_audio_buffer.clear'
}

// End session
{
    type: 'session.end'
}
```

#### Server to Client Messages:

```typescript
// Session created
{
    type: 'session.created',
    session: { id: string, ... }
}

// Audio response delta
{
    type: 'response.audio.delta',
    delta: string  // Base64 encoded PCM16 audio
}

// User transcription complete
{
    type: 'conversation.item.input_audio_transcription.completed',
    transcript: string
}

// AI transcription complete
{
    type: 'response.audio_transcript.done',
    transcript: string
}

// Response complete
{
    type: 'response.done'
}

// Error
{
    type: 'error',
    error: { code: string, message: string }
}
```

---

## 7. Transcription and Conversation Integration

### 7.1 Saving Transcriptions

Transcriptions from both user speech and AI responses are saved to the FoundationaLLM conversation store. Each transcription creates a new message entry with type `realtime-speech-transcription`.

### 7.2 Message Display

The ChatMessage component should handle the new message type:

```vue
<!-- In ChatMessage.vue -->
<template v-if="message.type === 'realtime-speech-transcription'">
    <div class="transcription-message">
        <i class="pi pi-microphone transcription-icon"></i>
        <span class="transcription-text">{{ message.text }}</span>
    </div>
</template>
```

### 7.3 Context Inclusion

When starting a realtime session, the conversation history is formatted and included in the GPT-Realtime session instructions, providing context continuity.

---

## 8. Implementation Phases

### Phase 1: Foundation (2-3 weeks)
1. Add `RealtimeSpeechAIModel` type to the AI Model registry
2. Update `AgentBase` with realtime speech properties
3. Create Management Portal UI for configuring realtime speech models
4. Add agent configuration UI for realtime speech settings

### Phase 2: Backend Services (2-3 weeks)
1. Implement `IRealtimeSpeechService` interface
2. Create `RealtimeSpeechService` with GPT-Realtime API integration
3. Add `RealtimeSpeechController` WebSocket endpoint
4. Implement transcription persistence

### Phase 3: User Portal Integration (2-3 weeks)
1. Create `RealtimeSpeechButton` component
2. Implement `useRealtimeSpeech` composable
3. Add audio capture and playback logic
4. Create waveform visualizer
5. Update ChatInput component

### Phase 4: Testing & Polish (1-2 weeks)
1. End-to-end testing
2. Browser compatibility testing
3. Performance optimization
4. Error handling improvements
5. Documentation

---

## 9. Security Considerations

### 9.1 Authentication
- WebSocket connections require the same JWT authentication as REST endpoints
- Token is passed as a query parameter or via the initial WebSocket handshake

### 9.2 Authorization
- Users can only access realtime speech for agents they have permission to use
- Rate limiting applies to realtime sessions

### 9.3 Data Privacy
- Transcriptions are stored with the same privacy controls as regular messages
- Audio data is not persisted; only transcriptions are saved
- All WebSocket communication uses TLS (wss://)

### 9.4 Input Validation
- Audio format validation before forwarding to GPT-Realtime
- Stop word detection for session termination
- Maximum session duration enforcement

---

## 10. Testing Strategy

### 10.1 Unit Tests
- RealtimeSpeechService methods
- Audio format conversion
- Message parsing and serialization

### 10.2 Integration Tests
- WebSocket connection establishment
- Message proxying between client and GPT-Realtime
- Transcription persistence

### 10.3 End-to-End Tests
- Full conversation flow from UI to GPT-Realtime and back
- Stop word detection
- Session timeout handling

### 10.4 Browser Compatibility
- Chrome (primary)
- Firefox
- Safari
- Edge

---

## Appendix A: Configuration Examples

### Agent Configuration JSON

```json
{
    "name": "voice-assistant",
    "type": "generic-agent",
    "display_name": "Voice Assistant",
    "realtime_speech_model_object_id": "/instances/{instanceId}/providers/FoundationaLLM.AIModel/aiModels/gpt-4o-realtime",
    "realtime_speech_settings": {
        "enabled": true,
        "stop_words": ["stop", "end conversation", "goodbye"],
        "max_session_duration_seconds": 600,
        "show_transcriptions": true,
        "include_conversation_history": true
    }
}
```

### AI Model Configuration JSON

```json
{
    "name": "gpt-4o-realtime",
    "type": "realtime-speech",
    "endpoint_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/azure-openai-realtime",
    "deployment_name": "gpt-4o-realtime-preview",
    "version": "2024-10-01-preview",
    "voice": "alloy",
    "input_audio_format": "pcm16",
    "output_audio_format": "pcm16",
    "input_audio_transcription_enabled": true,
    "input_audio_transcription_model": "whisper-1",
    "turn_detection": {
        "type": "server_vad",
        "threshold": 0.5,
        "prefix_padding_ms": 300,
        "silence_duration_ms": 500
    }
}
```

---

## Appendix B: Dependencies

### NuGet Packages (CoreAPI)
- `System.Net.WebSockets.Client` - WebSocket client for connecting to GPT-Realtime

### NPM Packages (User Portal)
- No additional packages required; uses native Web Audio API

---

## Appendix C: API Endpoint Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET (WS) | `/instances/{instanceId}/sessions/{sessionId}/realtime-speech` | WebSocket endpoint for realtime speech |
| GET | `/instances/{instanceId}/agents/{agentName}/realtime-speech/config` | Get realtime speech configuration |

---

*Document Version: 1.0*
*Last Updated: December 2024*
