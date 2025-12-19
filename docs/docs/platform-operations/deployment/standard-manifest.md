# Deployment Manifest Setup

The Deployment Manifest is a JSON file that configures all aspects of a Standard (AKS) deployment.

## Overview

The manifest template is located at:
```
deploy/standard/Deployment-Manifest.template.json
```

Copy to create your deployment configuration:
```powershell
cp Deployment-Manifest.template.json Deployment-Manifest.json
```

> **Tip:** Create separate manifests for different environments (dev, test, prod).

## General Settings

Root-level properties defining the deployment:

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `adminObjectId` | GUID | AD Group Object ID for admins | `995a549b-067e-...` |
| `baseDomain` | String | Base domain for services | `example.com` |
| `createVpnGateway` | Boolean | Create VPN Gateway | `true` |
| `environment` | String | Environment identifier | `dev`, `test`, `prod` |
| `instanceId` | GUID | Unique deployment instance ID | `5d40d2ee-aeb5-...` |
| `k8sNamespace` | String | Kubernetes namespace | `fllm` |
| `letsEncryptEmail` | Email | Let's Encrypt notifications | `admin@example.com` |
| `location` | String | Azure region | `eastus2` |
| `networkName` | String | Pre-provisioned network name | `fllm-network` |
| `project` | String | Project identifier (3-8 chars) | `fllm` |
| `subscription` | GUID | Azure subscription ID | `ad82622e-458a-...` |

### Notes

- **`createVpnGateway`**: Set to `false` if using existing VPN or ExpressRoute
- **`instanceId`**: Generate a unique GUID; used by authorization system
- **`letsEncryptEmail`**: Only needed if using Let's Encrypt certificates
- **`location`**: Choose a region supporting required Azure OpenAI models

### Supported Models by Region

The template auto-configures available models:
- gpt-35-turbo (0613, 1106)
- gpt-4 (1106-Preview)
- gpt-4o (2024-05-13)
- text-embedding-ada-002 (2)
- text-embedding-3-large/small

See [Azure OpenAI model availability](https://learn.microsoft.com/azure/ai-services/openai/concepts/models#model-summary-table-and-region-availability).

## Entra Client IDs

Application registration client IDs:

| Property | Description | Reference |
|----------|-------------|-----------|
| `authorization` | Authorization API | [Authorization Setup](security-permissions/authentication-authorization/pre-deployment/authorization-setup.md) |
| `chat` | Chat Portal | [Core Auth Setup](security-permissions/authentication-authorization/pre-deployment/core-authentication-setup.md) |
| `core` | Core API | [Core Auth Setup](security-permissions/authentication-authorization/pre-deployment/core-authentication-setup.md) |
| `managementapi` | Management API | [Management Auth Setup](security-permissions/authentication-authorization/pre-deployment/management-authentication-setup.md) |
| `managementui` | Management Portal | [Management Auth Setup](security-permissions/authentication-authorization/pre-deployment/management-authentication-setup.md) |
| `vectorizationapi` | Vectorization API | Authentication Setup |

## Entra Client Secrets

Secrets for authorization:

| Property | Description |
|----------|-------------|
| `authorization` | Client secret for Authorization app registration |

> **Security:** Store secrets securely. Consider using Azure Key Vault references.

## Entra Instances

Cloud endpoints for authentication:

| Property | Value |
|----------|-------|
| `authorization` | `https://login.microsoftonline.com/` |

For sovereign clouds, use the appropriate login URL.

## Entra Scopes

API scopes for each service:

| Property | Example | Description |
|----------|---------|-------------|
| `authorization` | `api://FoundationaLLM-Authorization-Auth` | Authorization API scope |
| `chat` | `api://FoundationaLLM-Auth/Data.Read` | Chat Portal scope |
| `core` | `Data.Read` | Core API scope |
| `managementapi` | `Data.Manage` | Management API scope |
| `managementui` | `api://FoundationaLLM-Management-Auth/Data.Manage` | Management Portal scope |
| `vectorizationapi` | `Data.Manage` | Vectorization API scope |

## Ingress Configuration

### API Ingress (Backend Cluster)

```json
{
  "apiIngress": {
    "coreapi": {
      "host": "api.fllm.example.com",
      "path": "/core/",
      "pathType": "ImplementationSpecific",
      "serviceName": "core-api",
      "sslCert": "coreapi"
    },
    "managementapi": {
      "host": "management-api.fllm.example.com",
      "path": "/management/",
      "pathType": "ImplementationSpecific",
      "serviceName": "management-api",
      "sslCert": "managementapi"
    },
    "vectorizationapi": {
      "host": "vectorization-api.fllm.example.com",
      "path": "/vectorization/",
      "pathType": "ImplementationSpecific",
      "serviceName": "vectorization-api",
      "sslCert": "vectorizationapi"
    }
  }
}
```

### Frontend Ingress (Frontend Cluster)

```json
{
  "frontendIngress": {
    "chatui": {
      "host": "chat.fllm.example.com",
      "path": "/",
      "pathType": "ImplementationSpecific",
      "serviceName": "chat-ui",
      "sslCert": "chatui"
    },
    "managementui": {
      "host": "management.fllm.example.com",
      "path": "/",
      "pathType": "ImplementationSpecific",
      "serviceName": "management-ui",
      "sslCert": "managementui"
    }
  }
}
```

## Resource Groups

Define resource group names for each workload:

| Property | Purpose | Default Pattern |
|----------|---------|-----------------|
| `app` | AKS clusters | `rg-{env}-{region}-app-{project}` |
| `auth` | Authorization storage | `rg-{env}-{region}-auth-{project}` |
| `data` | Customer data | `rg-{env}-{region}-data-{project}` |
| `dns` | Private DNS | `rg-{env}-{region}-dns-{project}` |
| `jbx` | Jumpbox | `rg-{env}-{region}-jbx-{project}` |
| `net` | Networking | `rg-{env}-{region}-net-{project}` |
| `oai` | Azure OpenAI | `rg-{env}-{region}-oai-{project}` |
| `ops` | Operations | `rg-{env}-{region}-ops-{project}` |
| `storage` | FLLM storage | `rg-{env}-{region}-storage-{project}` |
| `vec` | Vector databases | `rg-{env}-{region}-vec-{project}` |

## External Resource Groups

Pre-existing resource groups:

| Property | Purpose |
|----------|---------|
| `dns` | Pre-provisioned Private DNS zones |

> **Note:** Remove corresponding entry from `resourceGroups` if using external resources.

## Example Manifest

```json
{
  "adminObjectId": "995a549b-067e-4fe3-9f90-98d78b9ed086",
  "baseDomain": "example.com",
  "createVpnGateway": false,
  "environment": "dev",
  "instanceId": "5d40d2ee-aeb5-4391-95a0-1fd9045d7720",
  "k8sNamespace": "fllm",
  "letsEncryptEmail": "admin@example.com",
  "location": "eastus2",
  "networkName": "fllm-network",
  "project": "ai",
  "subscription": "ad82622e-458a-4a48-8023-6b18eed1cf79",
  
  "entraClientIds": {
    "authorization": "...",
    "chat": "...",
    "core": "...",
    "managementapi": "...",
    "managementui": "...",
    "vectorizationapi": "..."
  },
  
  "entraClientSecrets": {
    "authorization": "..."
  },
  
  "entraInstances": {
    "authorization": "https://login.microsoftonline.com/"
  },
  
  "entraScopes": {
    "authorization": "api://FoundationaLLM-Authorization-Auth",
    "chat": "api://FoundationaLLM-Auth/Data.Read",
    "core": "Data.Read",
    "managementapi": "Data.Manage",
    "managementui": "api://FoundationaLLM-Management-Auth/Data.Manage",
    "vectorizationapi": "Data.Manage"
  },
  
  "resourceGroups": {
    "app": "rg-ai-dev-eastus2-app",
    "auth": "rg-ai-dev-eastus2-auth",
    "data": "rg-ai-dev-eastus2-data",
    "dns": "rg-ai-dev-eastus2-dns",
    "jbx": "rg-ai-dev-eastus2-jbx",
    "net": "rg-ai-dev-eastus2-net",
    "oai": "rg-ai-dev-eastus2-oai",
    "ops": "rg-ai-dev-eastus2-ops",
    "storage": "rg-ai-dev-eastus2-storage",
    "vec": "rg-ai-dev-eastus2-vec"
  }
}
```

## Validation

Before deployment, verify:

1. All GUIDs are valid
2. Domain names are correct
3. SSL certificates are in place
4. Entra app registrations exist
5. Resource group names follow conventions

## Related Topics

- [Standard Deployment](deployment-standard.md)
- [Authentication Setup](security-permissions/authentication-authorization/index.md)
- [Custom Domains](custom-domains.md)
