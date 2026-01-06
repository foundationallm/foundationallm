# Deployment

FoundationaLLM provides multiple deployment options to suit different environments and requirements.

## Deployment Options

| Deployment Type | Infrastructure | Best For |
|-----------------|---------------|----------|
| [Quick Start](deployment-quick-start.md) | Azure Container Apps (ACA) | Development, POC, small deployments |
| [Standard](deployment-standard.md) | Azure Kubernetes Service (AKS) | Production, enterprise scale |

## Quick Comparison

| Feature | Quick Start (ACA) | Standard (AKS) |
|---------|-------------------|----------------|
| **Complexity** | Low | High |
| **Setup Time** | ~30 minutes | ~2 hours |
| **Scalability** | Auto-scaling | Advanced orchestration |
| **Networking** | Public endpoints | Private networking, VPN |
| **SSL Certificates** | Managed | Custom required |
| **Cost** | Lower | Higher |
| **Production Ready** | Dev/Test | Yes |

## Prerequisites

Both deployment types require:

| Requirement | Description |
|-------------|-------------|
| **Azure Subscription** | With appropriate quotas |
| **Azure OpenAI Access** | [Request access](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUNTZBNzRKNlVQSFhZMU9aV09EVzYxWFdORCQlQCN0PWcu) |
| **Entra ID App Registrations** | 6 app registrations required |
| **Owner Role** | On subscription and app registrations |

## Post-Deployment Configuration

After deployment completes, you must:

1. **Configure Authentication** - Complete Entra ID app registration setup
2. **Set MS Graph Permissions** - Run the Graph roles script
3. **Update Redirect URIs** - Configure OAuth callbacks
4. **Verify Access** - Test portal and API access

See [Authentication Setup](security-permissions/authentication-authorization/index.md) for detailed instructions.

## Deployment Tools

| Tool | Version | Purpose |
|------|---------|---------|
| Azure CLI | v2.51.0+ | Azure resource management |
| Azure Developer CLI (azd) | v1.6.1+ | Infrastructure provisioning |
| PowerShell | 7.4.1+ | Deployment scripts |
| Git | Latest | Repository cloning |
| AzCopy | Latest | File synchronization |

### Additional Tools for Standard Deployment

| Tool | Purpose |
|------|---------|
| kubectl | Kubernetes management |
| kubelogin | AKS authentication |
| Helm | Kubernetes package management |

## Related Topics

- [Quick Start Deployment](deployment-quick-start.md)
- [Standard Deployment](deployment-standard.md)
- [App Configuration Values](app-configuration-values.md)
- [Authentication Setup](security-permissions/authentication-authorization/index.md)
- [Azure Resource Providers](azure-resource-providers-requirements.md)
