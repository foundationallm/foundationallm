# OpenAI-Compatible API Implementation Plan for FoundationaLLM CoreAPI

## Executive Summary

This document outlines a comprehensive plan to implement OpenAI-compatible API endpoints within the FoundationaLLM CoreAPI. The goal is to enable drop-in replacement compatibility for clients using OpenAI SDKs, LiteLLM, LangChain, and other OpenAI-compatible tools.

## Table of Contents

1. [Current Architecture Overview](#1-current-architecture-overview)
2. [Scope and Compatibility Layers](#2-scope-and-compatibility-layers)
3. [Implementation Phases](#3-implementation-phases)
4. [Detailed Technical Specifications](#4-detailed-technical-specifications)
5. [File Structure and Organization](#5-file-structure-and-organization)
6. [Data Model Mappings](#6-data-model-mappings)
7. [Authentication Strategy](#7-authentication-strategy)
8. [Streaming Implementation](#8-streaming-implementation)
9. [Error Handling](#9-error-handling)
10. [Testing Strategy](#10-testing-strategy)
11. [Migration and Deployment Considerations](#11-migration-and-deployment-considerations)

---

## 1. Current Architecture Overview

### Existing CoreAPI Structure

```
src/dotnet/CoreAPI/
├── Controllers/
│   ├── CompletionsController.cs      # Current completion endpoints
│   ├── CompletionsStatusController.cs
│   ├── SessionsController.cs
│   ├── FilesController.cs
│   └── ...
├── Program.cs                         # Service configuration
└── CoreAPI.csproj
```

### Current Endpoint Pattern

| Current Endpoint | Method | Purpose |
|-----------------|--------|---------|
| `/instances/{instanceId}/completions` | POST | Synchronous completion |
| `/instances/{instanceId}/async-completions` | POST | Async completion |
| `/instances/{instanceId}/completions/agents` | GET | Get available agents |

### Current Request/Response Models

**CompletionRequest** (FoundationaLLM native):
```json
{
  "operation_id": "string",
  "session_id": "string",
  "user_prompt": "string",
  "agent_name": "string",
  "attachments": ["objectId1", "objectId2"],
  "message_history": [...],
  "settings": {...}
}
```

### Current Authentication
- Microsoft Entra ID JWT Bearer tokens
- Agent Access Tokens (custom FoundationaLLM tokens)

---

## 2. Scope and Compatibility Layers

### Compatibility Levels

| Level | Description | Priority |
|-------|-------------|----------|
| **Transport** | HTTPS, RESTful JSON, UTF-8 | ✅ Required |
| **Authentication** | Bearer token support | ✅ Required |
| **Request Schema** | OpenAI request format acceptance | ✅ Required |
| **Response Schema** | OpenAI response format output | ✅ Required |
| **Behavioral** | Streaming, tool calls, error shapes | ✅ Required |

### Target Endpoints (Prioritized)

#### Phase 1: Core Chat Completions (MVP)
| Endpoint | Priority | Notes |
|----------|----------|-------|
| `POST /v1/chat/completions` | P0 | Primary chat interface |
| `GET /v1/models` | P0 | Model discovery |
| `GET /v1/models/{id}` | P0 | Model metadata |

#### Phase 2: Extended Functionality
| Endpoint | Priority | Notes |
|----------|----------|-------|
| `POST /v1/responses` | P1 | Modern unified interface |
| `POST /v1/embeddings` | P2 | Vector generation |

#### Phase 3: Additional Features
| Endpoint | Priority | Notes |
|----------|----------|-------|
| `GET /v1/files` | P3 | File listing |
| `POST /v1/files` | P3 | File upload |
| `DELETE /v1/files/{id}` | P3 | File deletion |
| `POST /v1/audio/transcriptions` | P3 | Speech-to-text |
| `POST /v1/audio/speech` | P3 | Text-to-speech |

---

## 3. Implementation Phases

### Phase 1: Foundation (MVP)

#### 1.1 Models and Core Infrastructure
- [ ] Create OpenAI-compatible model classes
- [ ] Implement `OpenAIAuthenticationHandler` for Bearer tokens
- [ ] Create base controller infrastructure
- [ ] Implement `/v1/models` and `/v1/models/{id}` endpoints

#### 1.2 Chat Completions (Non-Streaming)
- [ ] Implement `POST /v1/chat/completions` endpoint
- [ ] Create request/response translation services
- [ ] Map FoundationaLLM agents to OpenAI models
- [ ] Handle basic chat completion flow

#### 1.3 Streaming Support
- [ ] Implement SSE (Server-Sent Events) infrastructure
- [ ] Add streaming support to chat completions
- [ ] Implement proper chunk formatting
- [ ] Add `[DONE]` termination handling

### Phase 2: Tool Calling and Advanced Features

#### 2.1 Tool/Function Calling
- [ ] Implement tool definition schema support
- [ ] Add tool call response handling
- [ ] Implement tool result message processing
- [ ] Handle parallel tool calls

#### 2.2 Error Handling and Rate Limiting
- [ ] Implement OpenAI-compatible error responses
- [ ] Add rate limiting headers
- [ ] Implement quota integration
- [ ] Add comprehensive logging

### Phase 3: Responses API and Extensibility

#### 3.1 Responses API
- [ ] Implement `POST /v1/responses` endpoint
- [ ] Add multimodal input support
- [ ] Implement structured output (JSON schema)

#### 3.2 Testing and Documentation
- [ ] Comprehensive integration tests
- [ ] SDK compatibility tests (OpenAI Python SDK)
- [ ] Performance testing
- [ ] Documentation updates

---

## 4. Detailed Technical Specifications

### 4.1 New File Structure

```
src/dotnet/
├── Common/
│   └── Models/
│       └── OpenAI/                              # NEW
│           ├── Requests/
│           │   ├── OpenAIChatCompletionRequest.cs
│           │   ├── OpenAIMessage.cs
│           │   ├── OpenAITool.cs
│           │   ├── OpenAIToolChoice.cs
│           │   ├── OpenAIResponseFormat.cs
│           │   └── OpenAIResponseRequest.cs
│           ├── Responses/
│           │   ├── OpenAIChatCompletionResponse.cs
│           │   ├── OpenAIChatCompletionChoice.cs
│           │   ├── OpenAIChatCompletionChunk.cs
│           │   ├── OpenAIUsage.cs
│           │   ├── OpenAIToolCall.cs
│           │   ├── OpenAIModel.cs
│           │   ├── OpenAIModelList.cs
│           │   ├── OpenAIResponseObject.cs
│           │   └── OpenAIError.cs
│           └── Common/
│               ├── OpenAIFinishReason.cs
│               └── OpenAIMessageRole.cs
├── Core/
│   ├── Interfaces/
│   │   └── IOpenAICompatibilityService.cs       # NEW
│   └── Services/
│       └── OpenAICompatibilityService.cs        # NEW
└── CoreAPI/
    ├── Controllers/
    │   └── v1/                                  # NEW
    │       ├── ChatCompletionsController.cs
    │       ├── ModelsController.cs
    │       └── ResponsesController.cs
    ├── Authentication/                          # NEW
    │   └── OpenAIBearerTokenHandler.cs
    ├── Middleware/                              # NEW
    │   └── OpenAICompatibilityMiddleware.cs
    └── Services/                                # NEW
        ├── IOpenAIRequestTranslator.cs
        ├── OpenAIRequestTranslator.cs
        ├── IOpenAIResponseTranslator.cs
        ├── OpenAIResponseTranslator.cs
        └── StreamingResponseWriter.cs
```

### 4.2 Core Request Models

```csharp
// OpenAIChatCompletionRequest.cs
namespace FoundationaLLM.Common.Models.OpenAI.Requests;

public class OpenAIChatCompletionRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required List<OpenAIMessage> Messages { get; set; }

    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }

    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }

    [JsonPropertyName("stop")]
    public object? Stop { get; set; } // string or string[]

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("tools")]
    public List<OpenAITool>? Tools { get; set; }

    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; } // string or OpenAIToolChoice

    [JsonPropertyName("response_format")]
    public OpenAIResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("n")]
    public int? N { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }
}

// OpenAIMessage.cs
public class OpenAIMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; set; } // system, user, assistant, tool

    [JsonPropertyName("content")]
    public object? Content { get; set; } // string or array of content parts

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("tool_calls")]
    public List<OpenAIToolCall>? ToolCalls { get; set; }

    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }
}
```

### 4.3 Core Response Models

```csharp
// OpenAIChatCompletionResponse.cs
namespace FoundationaLLM.Common.Models.OpenAI.Responses;

public class OpenAIChatCompletionResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; } = "chat.completion";

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("choices")]
    public required List<OpenAIChatCompletionChoice> Choices { get; set; }

    [JsonPropertyName("usage")]
    public required OpenAIUsage Usage { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
}

// OpenAIChatCompletionChoice.cs
public class OpenAIChatCompletionChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public OpenAIMessage? Message { get; set; }

    [JsonPropertyName("delta")]
    public OpenAIMessage? Delta { get; set; } // For streaming

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; } // stop, length, tool_calls, content_filter

    [JsonPropertyName("logprobs")]
    public object? Logprobs { get; set; }
}

// OpenAIUsage.cs
public class OpenAIUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
```

### 4.4 Controller Implementation

```csharp
// ChatCompletionsController.cs
namespace FoundationaLLM.Core.API.Controllers.v1;

[ApiController]
[Route("v1")]
[Authorize(AuthenticationSchemes = OpenAIBearerDefaults.AuthenticationScheme)]
public class ChatCompletionsController : ControllerBase
{
    private readonly IOpenAICompatibilityService _openAIService;
    private readonly ILogger<ChatCompletionsController> _logger;

    public ChatCompletionsController(
        IOpenAICompatibilityService openAIService,
        ILogger<ChatCompletionsController> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a chat completion - OpenAI compatible endpoint
    /// </summary>
    [HttpPost("chat/completions")]
    [Produces("application/json", "text/event-stream")]
    public async Task<IActionResult> CreateChatCompletion(
        [FromBody] OpenAIChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Stream)
        {
            return await HandleStreamingCompletion(request, cancellationToken);
        }

        var response = await _openAIService.CreateChatCompletionAsync(request, cancellationToken);
        return Ok(response);
    }

    private async Task<IActionResult> HandleStreamingCompletion(
        OpenAIChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        await foreach (var chunk in _openAIService.CreateChatCompletionStreamAsync(request, cancellationToken))
        {
            var json = JsonSerializer.Serialize(chunk);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);

        return new EmptyResult();
    }
}
```

---

## 5. File Structure and Organization

### New Files to Create

| File Path | Purpose |
|-----------|---------|
| `Common/Models/OpenAI/Requests/OpenAIChatCompletionRequest.cs` | Chat completion request model |
| `Common/Models/OpenAI/Requests/OpenAIMessage.cs` | Message model with role/content |
| `Common/Models/OpenAI/Requests/OpenAITool.cs` | Tool/function definition |
| `Common/Models/OpenAI/Requests/OpenAIToolChoice.cs` | Tool choice configuration |
| `Common/Models/OpenAI/Requests/OpenAIResponseFormat.cs` | Response format (JSON schema) |
| `Common/Models/OpenAI/Responses/OpenAIChatCompletionResponse.cs` | Chat completion response |
| `Common/Models/OpenAI/Responses/OpenAIChatCompletionChoice.cs` | Completion choice model |
| `Common/Models/OpenAI/Responses/OpenAIChatCompletionChunk.cs` | Streaming chunk model |
| `Common/Models/OpenAI/Responses/OpenAIUsage.cs` | Token usage model |
| `Common/Models/OpenAI/Responses/OpenAIToolCall.cs` | Tool call response |
| `Common/Models/OpenAI/Responses/OpenAIModel.cs` | Model information |
| `Common/Models/OpenAI/Responses/OpenAIModelList.cs` | Model list response |
| `Common/Models/OpenAI/Responses/OpenAIError.cs` | Error response model |
| `Core/Interfaces/IOpenAICompatibilityService.cs` | Service interface |
| `Core/Services/OpenAICompatibilityService.cs` | Main translation service |
| `CoreAPI/Controllers/v1/ChatCompletionsController.cs` | Chat completions controller |
| `CoreAPI/Controllers/v1/ModelsController.cs` | Models controller |
| `CoreAPI/Authentication/OpenAIBearerTokenHandler.cs` | Bearer token auth |
| `CoreAPI/Authentication/OpenAIBearerDefaults.cs` | Auth defaults |
| `CoreAPI/Authentication/OpenAIBearerOptions.cs` | Auth options |

### Files to Modify

| File Path | Changes |
|-----------|---------|
| `CoreAPI/Program.cs` | Add OpenAI authentication, middleware, services |
| `Common/Constants/HttpHeaders.cs` | Add OpenAI header constants |
| `Common/Services/DependencyInjection.cs` | Add OpenAI service registration methods |

---

## 6. Data Model Mappings

### Request Translation: OpenAI → FoundationaLLM

```
OpenAI Request                    FoundationaLLM Request
─────────────────────────────────────────────────────────
model                      →      agent_name (via mapping)
messages[].role=system     →      (system prompt in agent config)
messages[].role=user       →      user_prompt (latest) + message_history
messages[].role=assistant  →      message_history
messages[].role=tool       →      (tool response handling)
temperature                →      settings.temperature (pass-through)
max_tokens                 →      settings.max_tokens (pass-through)
stream                     →      (handled at controller level)
tools                      →      (agent tool configuration)
tool_choice                →      (tool selection handling)
```

### Model → Agent Mapping Strategy

```csharp
public class ModelAgentMapping
{
    // Option 1: Direct mapping via configuration
    // "gpt-4" → "knowledge-management-agent"
    
    // Option 2: Model ID format includes agent reference
    // "foundationallm:my-agent" → "my-agent"
    
    // Option 3: Default agent with model hints
    // "gpt-4" → default agent with model preference hint
}
```

### Response Translation: FoundationaLLM → OpenAI

```
FoundationaLLM Response           OpenAI Response
─────────────────────────────────────────────────────────
operation_id              →       id (with prefix: "chatcmpl-")
completion                →       choices[0].message.content
prompt_tokens             →       usage.prompt_tokens
completion_tokens         →       usage.completion_tokens
agent_name                →       model (reverse mapping)
content (array)           →       choices[0].message.content
content_artifacts         →       (embedded in content or metadata)
errors                    →       error object (if present)
```

---

## 7. Authentication Strategy

### Supported Authentication Methods

#### Method 1: OpenAI-Style Bearer Token (Primary)
```http
Authorization: Bearer sk-foundationallm-xxx
```

Maps to existing FoundationaLLM Agent Access Token system:
- Parse token prefix to identify authentication type
- Validate against Agent Resource Provider
- Extract user identity from token claims

#### Method 2: Entra ID Bearer Token (Existing)
```http
Authorization: Bearer eyJ0eXAi...
```

Existing Microsoft Entra ID authentication continues to work.

### Authentication Handler Implementation

```csharp
// OpenAIBearerTokenHandler.cs
public class OpenAIBearerTokenHandler : AuthenticationHandler<OpenAIBearerOptions>
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = headerValue.Substring(7);
        
        // Determine token type and validate
        if (IsFoundationaLLMToken(token))
        {
            return await ValidateFoundationaLLMToken(token);
        }
        else if (IsEntraIDToken(token))
        {
            return AuthenticateResult.NoResult(); // Fall through to JWT handler
        }

        return AuthenticateResult.Fail("Invalid token");
    }
}
```

### Optional Headers Support

| Header | Purpose | Handling |
|--------|---------|----------|
| `OpenAI-Organization` | Organization context | Map to FoundationaLLM instance |
| `OpenAI-Project` | Project context | Optional, can be ignored |
| `X-Request-Id` | Request tracking | Pass through for logging |

---

## 8. Streaming Implementation

### SSE (Server-Sent Events) Protocol

```
HTTP/1.1 200 OK
Content-Type: text/event-stream
Cache-Control: no-cache
Connection: keep-alive

data: {"id":"chatcmpl-xxx","object":"chat.completion.chunk","created":1700000000,"model":"gpt-4","choices":[{"index":0,"delta":{"role":"assistant"},"finish_reason":null}]}

data: {"id":"chatcmpl-xxx","object":"chat.completion.chunk","created":1700000000,"model":"gpt-4","choices":[{"index":0,"delta":{"content":"Hello"},"finish_reason":null}]}

data: {"id":"chatcmpl-xxx","object":"chat.completion.chunk","created":1700000000,"model":"gpt-4","choices":[{"index":0,"delta":{"content":" world"},"finish_reason":null}]}

data: {"id":"chatcmpl-xxx","object":"chat.completion.chunk","created":1700000000,"model":"gpt-4","choices":[{"index":0,"delta":{},"finish_reason":"stop"}]}

data: [DONE]
```

### Streaming Service Implementation

```csharp
public interface IOpenAICompatibilityService
{
    Task<OpenAIChatCompletionResponse> CreateChatCompletionAsync(
        OpenAIChatCompletionRequest request,
        CancellationToken cancellationToken);

    IAsyncEnumerable<OpenAIChatCompletionChunk> CreateChatCompletionStreamAsync(
        OpenAIChatCompletionRequest request,
        CancellationToken cancellationToken);
}
```

### Integration with Existing Async Completions

The existing `StartCompletionOperation` and `GetCompletionOperationStatus` can be leveraged for streaming by:

1. Starting an async operation
2. Polling for status updates
3. Streaming partial results as SSE chunks
4. Sending `[DONE]` on completion

---

## 9. Error Handling

### Error Response Format

```json
{
  "error": {
    "message": "Human readable message",
    "type": "invalid_request_error",
    "param": "model",
    "code": "model_not_found"
  }
}
```

### Error Types Mapping

| FoundationaLLM Error | OpenAI Error Type | HTTP Status |
|---------------------|-------------------|-------------|
| `ResourceProviderException` (403) | `permission_denied` | 403 |
| `ResourceProviderException` (404) | `not_found` | 404 |
| `CoreServiceException` (400) | `invalid_request_error` | 400 |
| `QuotaExceededException` | `rate_limit_exceeded` | 429 |
| `ArgumentException` | `invalid_request_error` | 400 |
| General Exception | `internal_error` | 500 |

### Error Middleware

```csharp
public class OpenAIErrorMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var error = MapToOpenAIError(ex);
        context.Response.StatusCode = error.StatusCode;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsJsonAsync(new { error = error.Body });
    }
}
```

---

## 10. Testing Strategy

### Unit Tests

```
tests/dotnet/
├── Core.Tests/
│   └── Services/
│       ├── OpenAICompatibilityServiceTests.cs
│       └── OpenAIRequestTranslatorTests.cs
└── CoreAPI.Tests/
    └── Controllers/
        └── v1/
            ├── ChatCompletionsControllerTests.cs
            └── ModelsControllerTests.cs
```

### Integration Tests

```csharp
[Fact]
public async Task ChatCompletion_WithOpenAISDK_ReturnsValidResponse()
{
    // Arrange
    var client = new OpenAIClient(
        new Uri("https://localhost:5000/v1"),
        new AzureKeyCredential("sk-test-key"));

    // Act
    var response = await client.GetChatClient("test-agent")
        .CompleteChatAsync([
            new UserChatMessage("Hello")
        ]);

    // Assert
    Assert.NotNull(response);
    Assert.NotEmpty(response.Value.Content[0].Text);
}
```

### Compatibility Tests

| Test | Description |
|------|-------------|
| OpenAI Python SDK | Test with `openai` Python package |
| LiteLLM | Test with LiteLLM proxy configuration |
| LangChain | Test with LangChain ChatOpenAI |
| Curl | Basic curl request validation |

### Litmus Test

```python
from openai import OpenAI

client = OpenAI(base_url="https://api.foundationallm.com/v1")

resp = client.chat.completions.create(
    model="your-agent",
    messages=[{"role": "user", "content": "Hello"}]
)

print(resp.choices[0].message.content)
```

---

## 11. Migration and Deployment Considerations

### Backward Compatibility

- Existing `/instances/{instanceId}/completions` endpoints remain unchanged
- New `/v1/*` endpoints operate in parallel
- Gradual migration path for existing clients

### Configuration

Add to `appsettings.json`:
```json
{
  "FoundationaLLM": {
    "OpenAICompatibility": {
      "Enabled": true,
      "DefaultInstanceId": "default-instance",
      "ModelAgentMappings": {
        "gpt-4": "knowledge-management-agent",
        "gpt-3.5-turbo": "basic-chat-agent"
      },
      "RateLimiting": {
        "RequestsPerMinute": 60,
        "TokensPerMinute": 100000
      }
    }
  }
}
```

### Rate Limiting Headers

```http
x-ratelimit-limit-requests: 60
x-ratelimit-remaining-requests: 45
x-ratelimit-reset-requests: 30s
x-ratelimit-limit-tokens: 100000
x-ratelimit-remaining-tokens: 85000
x-ratelimit-reset-tokens: 1m
```

### Monitoring and Observability

- OpenTelemetry integration for `/v1/*` endpoints
- Custom metrics for OpenAI compatibility layer
- Request/response logging for debugging
- Token usage tracking

---

## Appendix A: Complete Request/Response Examples

### Chat Completion Request

```json
POST /v1/chat/completions HTTP/1.1
Host: api.foundationallm.com
Authorization: Bearer sk-foundationallm-xxx
Content-Type: application/json

{
  "model": "knowledge-management-agent",
  "messages": [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "What is the weather today?"}
  ],
  "temperature": 0.7,
  "max_tokens": 150,
  "stream": false
}
```

### Chat Completion Response

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": "chatcmpl-abc123",
  "object": "chat.completion",
  "created": 1700000000,
  "model": "knowledge-management-agent",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "I don't have access to real-time weather data..."
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 25,
    "completion_tokens": 42,
    "total_tokens": 67
  }
}
```

### Tool Calling Request

```json
{
  "model": "tool-enabled-agent",
  "messages": [
    {"role": "user", "content": "Get the current stock price of AAPL"}
  ],
  "tools": [
    {
      "type": "function",
      "function": {
        "name": "get_stock_price",
        "description": "Get the current stock price",
        "parameters": {
          "type": "object",
          "properties": {
            "symbol": {
              "type": "string",
              "description": "The stock symbol"
            }
          },
          "required": ["symbol"]
        }
      }
    }
  ]
}
```

### Tool Calling Response

```json
{
  "id": "chatcmpl-tool123",
  "object": "chat.completion",
  "created": 1700000000,
  "model": "tool-enabled-agent",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": null,
        "tool_calls": [
          {
            "id": "call_abc123",
            "type": "function",
            "function": {
              "name": "get_stock_price",
              "arguments": "{\"symbol\": \"AAPL\"}"
            }
          }
        ]
      },
      "finish_reason": "tool_calls"
    }
  ],
  "usage": {
    "prompt_tokens": 50,
    "completion_tokens": 20,
    "total_tokens": 70
  }
}
```

---

## Appendix B: Implementation Checklist

### Phase 1 Checklist

- [ ] Create `Common/Models/OpenAI/` folder structure
- [ ] Implement all request models
- [ ] Implement all response models
- [ ] Create `IOpenAICompatibilityService` interface
- [ ] Implement `OpenAICompatibilityService`
- [ ] Create `OpenAIBearerTokenHandler`
- [ ] Create `ChatCompletionsController`
- [ ] Create `ModelsController`
- [ ] Update `Program.cs` with new services
- [ ] Add configuration settings
- [ ] Write unit tests
- [ ] Write integration tests

### Phase 2 Checklist

- [ ] Implement streaming support
- [ ] Add tool calling support
- [ ] Implement error middleware
- [ ] Add rate limiting headers
- [ ] Integrate with quota service
- [ ] Add OpenTelemetry metrics

### Phase 3 Checklist

- [ ] Implement Responses API
- [ ] Add file endpoints (optional)
- [ ] SDK compatibility testing
- [ ] Performance testing
- [ ] Documentation updates
- [ ] Deployment procedures

---

## Appendix C: Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Breaking existing clients | High | Parallel endpoints, feature flags |
| Performance overhead | Medium | Efficient translation, caching |
| Security vulnerabilities | High | Token validation, input sanitization |
| Incomplete compatibility | Medium | Prioritized feature implementation |
| Streaming complexity | Medium | Thorough testing, fallback to non-streaming |

---

*Document Version: 1.1*
*Last Updated: December 2024*
*Author: FoundationaLLM Development Team*
