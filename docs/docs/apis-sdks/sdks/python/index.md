# Python SDK

The FoundationaLLM Python SDK provides integration capabilities for Python applications.

## Overview

The `foundationallm` Python package provides:

- Internal orchestration components (LangChain integration)
- Telemetry and logging utilities
- Configuration management
- Integration with FoundationaLLM platform services

## Installation

```bash
pip install foundationallm
```

Or from source:

```bash
cd src/python/PythonSDK
pip install -e .
```

## Package Structure

```
foundationallm/
├── config/           # Configuration management
├── hubs/             # Hub integrations (Agent, Prompt, Data Source)
├── langchain/        # LangChain orchestration components
│   ├── agents/       # Agent implementations
│   ├── data_sources/ # Data source connectors
│   ├── message_history/ # Conversation history
│   └── orchestration/ # Orchestration managers
├── models/           # Data models
├── plugins/          # Plugin system
├── storage/          # Storage integrations
├── telemetry/        # OpenTelemetry integration
└── utils/            # Utility functions
```

## Configuration

### Environment Variables

| Variable | Description |
|----------|-------------|
| `FOUNDATIONALLM_APP_CONFIG_CONNECTION_STRING` | Azure App Configuration connection string |
| `FOUNDATIONALLM_INSTANCE_ID` | FoundationaLLM instance identifier |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Application Insights telemetry |

### Configuration Manager

```python
from foundationallm.config import Configuration

# Initialize from environment
config = Configuration()

# Access configuration values
api_url = config.get_value("FoundationaLLM:APIs:CoreAPI:APIUrl")
instance_id = config.get_value("FoundationaLLM:Instance:Id")
```

## LangChain Integration

The SDK provides LangChain components for building orchestration workflows.

### Orchestration Manager

```python
from foundationallm.langchain.orchestration import OrchestrationManager

# Initialize orchestration
orchestration = OrchestrationManager(
    config=config,
    agent_config=agent_configuration
)

# Process a completion request
result = await orchestration.run(
    user_prompt="What is FoundationaLLM?",
    message_history=[]
)
```

### Message History

```python
from foundationallm.langchain.message_history import MessageHistoryManager

# Manage conversation history
history_manager = MessageHistoryManager(
    session_id="session-guid",
    max_messages=10
)

# Add messages
history_manager.add_user_message("What is AI?")
history_manager.add_assistant_message("AI stands for Artificial Intelligence...")

# Get history for context
messages = history_manager.get_messages()
```

## Telemetry

The SDK integrates with OpenTelemetry for distributed tracing:

```python
from foundationallm.telemetry import Telemetry

# Initialize telemetry
telemetry = Telemetry(
    service_name="my-service",
    connection_string=app_insights_connection
)

# Create spans
with telemetry.create_span("process_request") as span:
    span.set_attribute("user_id", user_id)
    # Process request
```

## Plugin Development

Create custom plugins for FoundationaLLM:

### Plugin Structure

```
my_plugin/
├── __init__.py
├── plugin.py
├── requirements.txt
└── plugin.json
```

### Plugin Manifest (plugin.json)

```json
{
  "name": "my-custom-plugin",
  "version": "1.0.0",
  "type": "AgentTool",
  "description": "Custom tool plugin",
  "entry_point": "plugin:MyCustomTool"
}
```

### Plugin Implementation

```python
from foundationallm.plugins import PluginBase

class MyCustomTool(PluginBase):
    """Custom tool implementation."""
    
    def __init__(self, config):
        super().__init__(config)
        
    async def execute(self, parameters):
        """Execute the tool."""
        # Implementation
        return result
```

## REST API Client

For direct API calls, use standard HTTP libraries:

```python
import httpx
from azure.identity import DefaultAzureCredential

# Get authentication token
credential = DefaultAzureCredential()
token = credential.get_token("api://your-api-client-id/.default")

# Call Core API
async with httpx.AsyncClient() as client:
    response = await client.post(
        f"{core_api_url}/instances/{instance_id}/completions",
        headers={
            "Authorization": f"Bearer {token.token}",
            "Content-Type": "application/json"
        },
        json={
            "user_prompt": "Hello, what can you do?",
            "agent_name": "default-agent"
        }
    )
    result = response.json()
    print(result["completion"])
```

## Usage in FoundationaLLM Services

The Python SDK powers several FoundationaLLM services:

| Service | Description |
|---------|-------------|
| **LangChainAPI** | LangChain-based orchestration service |
| **GatekeeperIntegrationAPI** | Content safety integrations |
| **Data Pipeline Workers** | Pipeline processing components |

## Development

### Setup Development Environment

```bash
# Create virtual environment
python -m venv venv
source venv/bin/activate  # Linux/macOS
# or: venv\Scripts\activate  # Windows

# Install dependencies
pip install -r requirements.txt

# Install package in development mode
pip install -e .
```

### Running Tests

```bash
pytest tests/
```

## Requirements

- Python 3.11+
- See `requirements.txt` for dependencies

## Related Topics

- [Core API Reference](../../apis/core-api/api-reference.md)
- [Management API Reference](../../apis/management-api/api-reference.md)
- [.NET SDK](../dotnet/index.md)
- [Plugin Development](../../../management-portal/reference/concepts/plugins-packages.md)
