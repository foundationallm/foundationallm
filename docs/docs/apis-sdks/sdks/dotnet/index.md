# .NET SDK

The FoundationaLLM .NET SDK provides client libraries for integrating with FoundationaLLM APIs in .NET applications.

## Overview

The SDK includes two NuGet packages:

| Package | Description | NuGet |
|---------|-------------|-------|
| **FoundationaLLM.Client.Core** | Core API client for completions, sessions, agents | [NuGet](https://www.nuget.org/packages/FoundationaLLM.Client.Core) |
| **FoundationaLLM.Client.Management** | Management API client for resource management | [NuGet](https://www.nuget.org/packages/FoundationaLLM.Client.Management) |

## Installation

```bash
# Core API client
dotnet add package FoundationaLLM.Client.Core

# Management API client
dotnet add package FoundationaLLM.Client.Management
```

## Client Architecture

Each package provides two client classes:

| Client Type | Description |
|-------------|-------------|
| **RESTClient** | Low-level client with direct access to all API endpoints |
| **Client** | High-level client with simplified, user-friendly methods |

> Choose based on your needs: RESTClient for complete API access, Client for simpler common operations.

---

## Core Client

### Quick Start

```csharp
using FoundationaLLM.Client.Core;
using Azure.Identity;

// Configuration
var coreApiUrl = "https://your-core-api.azurecontainerapps.io";
var instanceId = "your-instance-guid";
var credential = new AzureCliCredential(); // or ManagedIdentityCredential for production

// Create client
var coreClient = new CoreClient(coreApiUrl, credential, instanceId);

// Get available agents
var agents = await coreClient.GetAgentsAsync();

// Request a completion
var response = await coreClient.GetCompletionAsync(
    userPrompt: "What can you help me with?",
    agentName: "default-agent"
);
Console.WriteLine(response.Completion);
```

### Using Dependency Injection

**1. Configuration file (appsettings.json):**

```json
{
  "FoundationaLLM": {
    "APIEndpoints": {
      "CoreAPI": {
        "Essentials": {
          "APIUrl": "https://your-core-api.azurecontainerapps.io"
        }
      }
    },
    "Instance": {
      "Id": "your-instance-guid"
    }
  }
}
```

**2. Service registration:**

```csharp
using FoundationaLLM.Client.Core;
using FoundationaLLM.Common.Constants.Configuration;

var builder = WebApplication.CreateBuilder(args);
var credential = new DefaultAzureCredential();

// Register Core clients
builder.Services.AddCoreClient(
    builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Essentials_APIUrl]!,
    credential,
    builder.Configuration[AppConfigurationKeys.FoundationaLLM_Instance_Id]!
);
```

**3. Inject and use:**

```csharp
public class ChatService
{
    private readonly ICoreClient _coreClient;
    
    public ChatService(ICoreClient coreClient)
    {
        _coreClient = coreClient;
    }
    
    public async Task<string> GetResponseAsync(string question)
    {
        var response = await _coreClient.GetCompletionAsync(question);
        return response.Completion;
    }
}
```

### Core Client Methods

| Method | Description |
|--------|-------------|
| `GetAgentsAsync()` | List available agents |
| `GetCompletionAsync(prompt, agent)` | Request a completion |
| `GetChatCompletionAsync(prompt, sessionId, agent)` | Chat completion with session context |

### Core REST Client Methods

| Client | Method | Description |
|--------|--------|-------------|
| `Sessions` | `GetSessionsAsync()` | List all sessions |
| `Sessions` | `CreateSessionAsync(name)` | Create new session |
| `Sessions` | `GetMessagesAsync(sessionId)` | Get session messages |
| `Sessions` | `DeleteSessionAsync(sessionId)` | Delete session |
| `Completions` | `GetCompletionAsync(request)` | Request completion |
| `Completions` | `StartCompletionOperationAsync(request)` | Async completion |
| `Status` | `GetServiceStatusAsync()` | Check API status |
| `Branding` | `GetBrandingAsync()` | Get branding config |

---

## Management Client

### Quick Start

```csharp
using FoundationaLLM.Client.Management;
using Azure.Identity;

// Configuration
var managementApiUrl = "https://your-management-api.azurecontainerapps.io";
var instanceId = "your-instance-guid";
var credential = new AzureCliCredential();

// Create client
var managementClient = new ManagementClient(managementApiUrl, credential, instanceId);

// List agents
var agents = await managementClient.Agents.GetAgentsAsync();

// Create a prompt
await managementClient.Prompts.CreatePromptAsync(new PromptRequest
{
    Name = "my-prompt",
    DisplayName = "My Prompt",
    Category = "AgentWorkflow",
    Prefix = "You are a helpful assistant..."
});

// Delete and purge a data source
await managementClient.DataSources.DeleteDataSourceAsync("old-source");
await managementClient.DataSources.PurgeDataSourceAsync("old-source");
```

### Using Dependency Injection

**1. Configuration file (appsettings.json):**

```json
{
  "FoundationaLLM": {
    "APIEndpoints": {
      "ManagementAPI": {
        "Essentials": {
          "APIUrl": "https://your-management-api.azurecontainerapps.io"
        }
      }
    },
    "Instance": {
      "Id": "your-instance-guid"
    }
  }
}
```

**2. Service registration:**

```csharp
using FoundationaLLM.Client.Management;

builder.Services.AddManagementClient(
    builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIEndpoints_ManagementAPI_Essentials_APIUrl]!,
    credential,
    builder.Configuration[AppConfigurationKeys.FoundationaLLM_Instance_Id]!
);
```

**3. Inject and use:**

```csharp
public class AgentService
{
    private readonly IManagementClient _managementClient;
    
    public AgentService(IManagementClient managementClient)
    {
        _managementClient = managementClient;
    }
    
    public async Task<IEnumerable<Agent>> GetAllAgentsAsync()
    {
        return await _managementClient.Agents.GetAgentsAsync();
    }
}
```

### Management Client Interfaces

| Interface | Purpose |
|-----------|---------|
| `IAgentManagementClient` | Manage agents |
| `IPromptManagementClient` | Manage prompts |
| `IDataSourceManagementClient` | Manage data sources |
| `IAIModelManagementClient` | Manage AI models |
| `IConfigurationManagementClient` | Manage configuration |
| `IVectorizationManagementClient` | Manage vectorization profiles |
| `IAttachmentManagementClient` | Manage attachments |

---

## Azure App Configuration Integration

Load configuration from Azure App Configuration:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .AddAzureAppConfiguration(options =>
    {
        options.Connect(Environment.GetEnvironmentVariable("FLLM_AppConfig_ConnectionString"));
        options.ConfigureKeyVault(kv => kv.SetCredential(credential));
        options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
        options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Essentials);
        options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_ManagementAPI_Essentials);
    })
    .Build();
```

---

## Authentication

### Development (Azure CLI)

```csharp
var credential = new AzureCliCredential();
```

### Production (Managed Identity)

```csharp
var credential = new ManagedIdentityCredential();
```

### Using DefaultAuthentication Helper

```csharp
using FoundationaLLM.Common.Authentication;

// Initialize for environment
DefaultAuthentication.Initialize(isProduction: false, serviceName: "MyApp");
var credential = DefaultAuthentication.AzureCredential;
```

---

## Configuration Options

```csharp
var options = new APIClientSettings
{
    Timeout = TimeSpan.FromSeconds(600) // Default: 900 seconds
};

var coreClient = new CoreClient(apiUrl, credential, instanceId, options);
```

---

## Error Handling

```csharp
try
{
    var response = await coreClient.GetCompletionAsync("What is FoundationaLLM?");
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
{
    // Handle authentication error
    Console.WriteLine("Authentication failed. Check your credentials.");
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
{
    // Handle rate limiting
    Console.WriteLine("Rate limit exceeded. Retry later.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

---

## Examples

The `Core.Examples` test project in the FoundationaLLM repository contains comprehensive examples demonstrating:

- Session management
- Completion requests
- Agent configuration
- Data source management
- Vectorization workflows

---

## Related Topics

- [Core API Reference](../../apis/core-api/api-reference.md)
- [Management API Reference](../../apis/management-api/api-reference.md)
- [Python SDK](../python/index.md)
