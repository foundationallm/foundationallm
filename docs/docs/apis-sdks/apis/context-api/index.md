# Context API

The Context API is an internal FoundationaLLM service that provides context management capabilities including file handling, code session management, knowledge resources, and content safety features.

## Overview

The Context API enables:

- File upload and management for conversations and agents
- Code session management for dynamic code execution
- Knowledge resource management (knowledge units, knowledge sources)
- Content safety scanning for prompt injection detection

## Authentication

The Context API uses API Key authentication. Include the API key in the `X-API-KEY` header:

```http
X-API-KEY: <your-api-key>
```

## Base URL

The Context API is typically deployed as an internal service within the Kubernetes cluster:

```
http://context-api.{namespace}.svc.cluster.local
```

For local development:
```
https://localhost:6004
```

## API Versioning

Include the API version as a query parameter:

```http
?api-version=2025-03-20
```

## Available Endpoints

| Category | Endpoint | Description |
|----------|----------|-------------|
| [Content Safety](content-safety.md) | `POST /contentSafety/shield` | Scan content for prompt injection attacks |
| Files | `POST /conversations/{id}/files` | Upload file to conversation |
| Files | `POST /agents/{name}/files` | Upload file to agent |
| Files | `GET /files/{id}` | Download file |
| Files | `GET /fileRecords/{id}` | Get file metadata |
| Files | `DELETE /fileRecords/{id}` | Delete file |
| Code Sessions | `POST /codeSessions` | Create code execution session |
| Knowledge | `GET /{resourceType}/{id}` | Get knowledge resource |
| Knowledge | `POST /{resourceType}/list` | List knowledge resources |

## Related Topics

- [Content Safety API](content-safety.md)
- [Data Pipelines](../management-api/data-pipelines.md)
