# Standard Deployment with AKS

The Standard deployment uses Azure Kubernetes Service (AKS) for production environments requiring advanced orchestration, scalability, and enterprise-grade security features.

## Overview

| Aspect | Details |
|--------|---------|
| **Infrastructure** | Azure Kubernetes Service (AKS) |
| **Deployment Time** | ~2 hours |
| **Scalability** | Advanced Kubernetes orchestration |
| **Networking** | Private endpoints, VNet peering |
| **SSL** | Custom certificates required |

## Architecture

The Standard deployment creates:

- **Two AKS clusters**: Frontend (portals) and Backend (APIs)
- **Private networking**: VNet with subnets, private endpoints
- **Multiple resource groups**: Organized by workload type
- **Full Azure monitoring**: Log Analytics, Application Insights

## Prerequisites

### Azure Requirements

| Requirement | Details |
|-------------|---------|
| **Azure Subscription** | Dedicated subscription recommended |
| **Azure OpenAI Access** | [Request access](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUNTZBNzRKNlVQSFhZMU9aV09EVzYxWFdORCQlQCN0PWcu) |
| **VM Quota** | ~64 vCPUs minimum (varies by node SKU) |

### Service Principal Requirements

| Role | Scope |
|------|-------|
| `Directory Reader` | Azure Tenant |
| `Owner` | Target resource groups (or subscription if RGs don't exist) |
| `DNS Zone Contributor` | DNS resource group |
| `Network Contributor` | Hub VNet resource |

### Entra ID Requirements

| Requirement | Details |
|-------------|---------|
| **AD Groups** | `FLLM-Admins`, `FLLM-Users` (pre-created) |
| **App Registrations** | 6 registrations (see [Authentication Setup](security-permissions/authentication-authorization/index.md)) |

### Required Tools

| Tool | Purpose |
|------|---------|
| Azure CLI v2.51.0+ | Azure management |
| PowerShell 7.4.1+ | Deployment scripts |
| Git | Repository access |
| Helm | Kubernetes deployments |
| kubectl | Cluster management |
| kubelogin | AKS authentication |

## Pre-Deployment Information

Gather the following before starting:

### General Settings

| Setting | Description | Example |
|---------|-------------|---------|
| Project ID | 3-8 character identifier | `fllm` |
| Location | Azure region | `eastus2` |
| Instance ID | Unique GUID | `5d40d2ee-...` |
| Admin Group | AD Group Object ID | `995a549b-...` |

### Network Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| VNet CIDR | `10.220.128.0/20` | Must be /20 netmask |
| AKS Service CIDR | `10.100.0.0/16` | Kubernetes services |
| Allowed External CIDRs | `192.168.100.0/24` | Access whitelist |

### AKS Node Pools

| Pool | Default SKU | Nodes |
|------|-------------|-------|
| Backend User | `Standard_D8_v5` | 1-3 |
| Backend System | `Standard_D2_v5` | 1-3 |
| Frontend User | `Standard_D2_v5` | 1-3 |
| Frontend System | `Standard_D2_v5` | 1-3 |

### SSL Certificates

Provision certificates in PFX format for:

| Service | Hostname Example |
|---------|------------------|
| Core API | `api.example.com` |
| Management API | `management-api.example.com` |
| Chat UI | `chat.example.com` |
| Management UI | `management.example.com` |

## Deployment Steps

### Step 1: Clone Repository

```powershell
git clone https://github.com/foundationallm/foundationallm.git
cd foundationallm
git checkout release/0.9.6
```

### Step 2: Configure Environment

```powershell
azd env new --location <azure-region> --subscription <subscription-id>
```

### Step 3: Prepare SSL Certificates

Place PFX files in `deploy/standard/certs/`:

```
deploy/standard/certs/
├── api.example.com.pfx
├── management-api.example.com.pfx
├── chat.example.com.pfx
└── management.example.com.pfx
```

### Step 4: Create Deployment Manifest

Copy and configure the manifest:

```powershell
cp deploy/standard/Deployment-Manifest.template.json deploy/standard/Deployment-Manifest.json
```

Edit `Deployment-Manifest.json` with your settings. See [Deployment Manifest Setup](standard-manifest.md) for details.

### Step 5: Provision Infrastructure

```powershell
cd deploy/standard
azd provision
```

The script generates `hosts` file in `config/` folder with private endpoint IPs.

### Step 6: Configure Network Access

Ensure network connectivity to deployed resources:
- Update local hosts file, OR
- Configure organization DNS

### Step 7: Deploy Applications

```powershell
azd deploy
```

This deploys to both clusters and generates `hosts.ingress` with service endpoints.

### Step 8: Post-Deployment Configuration

**Configure MS Graph Permissions:**

```powershell
../common/scripts/Set-FllmGraphRoles.ps1 -resourceGroupName <app-resource-group>
```

**Update OAuth Callback URIs:**

```powershell
../common/scripts/Update-OAuthCallbackUris.ps1
```

### Step 9: Verify Deployment

1. Access the Chat UI URL
2. Log in with an authorized user
3. Send a test message: "Who are you?"
4. Verify the default agent responds

## Resource Groups

| Name Pattern | Purpose |
|--------------|---------|
| `rg-{project}-{env}-{region}-app` | AKS clusters |
| `rg-{project}-{env}-{region}-auth` | Authorization storage |
| `rg-{project}-{env}-{region}-data` | Customer data |
| `rg-{project}-{env}-{region}-dns` | Private DNS |
| `rg-{project}-{env}-{region}-net` | Networking |
| `rg-{project}-{env}-{region}-oai` | Azure OpenAI |
| `rg-{project}-{env}-{region}-ops` | Operations |
| `rg-{project}-{env}-{region}-storage` | FLLM storage |
| `rg-{project}-{env}-{region}-vec` | Vector databases |

## Updating Deployments

To update container images:

```powershell
./Deploy-Images-Aks.ps1 `
    -aksName "<aks-name>" `
    -resourceGroup "<resource-group>" `
    -tag "<version>"
```

See [Updating Container Versions](../how-to-guides/updating-container-versions.md) for details.

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Quota exceeded | Request quota increase for selected SKUs |
| Network access denied | Verify hosts file or DNS configuration |
| Certificate errors | Verify PFX format and placement |
| Pod crashes | Check pod logs with `kubectl logs` |

## Related Topics

- [Quick Start Deployment](deployment-quick-start.md)
- [Deployment Manifest Setup](standard-manifest.md)
- [Authentication Setup](security-permissions/authentication-authorization/index.md)
- [Network Security Groups](../security-permissions/network-security-groups.md)
- [Updating Container Versions](../how-to-guides/updating-container-versions.md)
