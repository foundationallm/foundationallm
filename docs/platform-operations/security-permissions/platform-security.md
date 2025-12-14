# Platform Security Features & Best Practices

This guide covers security features and best practices for FoundationaLLM deployments.

## Security Overview

FoundationaLLM implements security at multiple layers:

| Layer | Implementation |
|-------|----------------|
| **Identity** | Microsoft Entra ID |
| **Network** | VNets, NSGs, Private Endpoints |
| **Data** | Encryption at rest/transit |
| **Application** | RBAC, Content Safety |
| **Operations** | Monitoring, Audit Logs |

## Identity & Access Management

### Microsoft Entra ID Integration

FoundationaLLM uses Entra ID for:
- User authentication
- Service authentication (Managed Identities)
- API authorization
- Group-based access control

### Best Practices

| Practice | Recommendation |
|----------|----------------|
| **MFA** | Enable for all users |
| **Conditional Access** | Configure based on risk profile |
| **Privileged Identity Management** | Use for admin accounts |
| **App Registration Security** | Limit secret expiration, use certificates |

### Managed Identities

All FoundationaLLM services use managed identities for:
- Azure resource access (Key Vault, Storage, Cosmos DB)
- Inter-service communication
- MS Graph API access

Benefits:
- No credential management
- Automatic rotation
- Auditable access

## Network Security

### Standard Deployment Architecture

| Component | Network Configuration |
|-----------|----------------------|
| **VNets** | Isolated network segments |
| **Subnets** | Service-specific isolation |
| **NSGs** | Traffic filtering |
| **Private Endpoints** | Private access to PaaS services |
| **VNet Peering** | Hub connectivity |

See [Network Security Groups](network-security-groups.md) for detailed NSG rules.

### Quick Start Deployment

- Uses Azure Container Apps managed networking
- Public endpoints with Azure-managed security
- Suitable for development/POC

### Private Connectivity Options

| Option | Use Case |
|--------|----------|
| **VPN Gateway** | Site-to-site connectivity |
| **ExpressRoute** | Dedicated private connection |
| **Private Endpoints** | Private PaaS access |

## Data Encryption

### Encryption at Rest

| Service | Encryption |
|---------|------------|
| Azure Storage | AES-256, Microsoft-managed keys |
| Cosmos DB | AES-256, Microsoft-managed keys |
| Key Vault | HSM-backed encryption |
| Azure OpenAI | Microsoft-managed keys |

### Customer-Managed Keys (CMK)

Enable CMK for enhanced control:

```bash
# Enable CMK for Storage Account
az storage account update \
  --name <storage-account> \
  --resource-group <resource-group> \
  --encryption-key-source Microsoft.Keyvault \
  --encryption-key-vault <keyvault-uri> \
  --encryption-key-name <key-name>
```

> **Note:** CMK requires additional Key Vault configuration and is not enabled by default.

### Encryption in Transit

- All services use TLS 1.2+
- Internal communication uses mTLS (AKS)
- API endpoints enforce HTTPS

## Threat Detection & Monitoring

### Default Monitoring

Standard deployment enables:
- Azure Diagnostics on all resources
- Log Analytics workspace
- Application Insights

### Recommended Enhancements

| Feature | Benefit |
|---------|---------|
| **Microsoft Defender for Cloud** | Threat detection, security posture |
| **Azure Sentinel** | SIEM, threat hunting |
| **Azure Monitor Alerts** | Real-time alerting |

### Enable Defender for Cloud

```bash
az security pricing create \
  --name VirtualMachines \
  --tier Standard

az security pricing create \
  --name StorageAccounts \
  --tier Standard
```

### Security Alerts

Configure alerts for:
- Failed authentication attempts
- Unusual API access patterns
- Resource configuration changes
- Key Vault access anomalies

## Gatekeeper & Content Safety

### Content Filtering

FoundationaLLM includes content safety integrations:

| Feature | Purpose |
|---------|---------|
| **Azure Content Safety** | Harmful content detection |
| **Microsoft Presidio** | PII detection/redaction |
| **Lakera Guard** | Prompt injection protection |
| **Enkrypt Guardrails** | Additional safety controls |

### Configuration

Enable/disable in App Configuration:

| Key | Default |
|-----|---------|
| `FoundationaLLM:APIs:CoreAPI:BypassGatekeeper` | `true` |
| `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableAzureContentSafety` | `true` |
| `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableMicrosoftPresidio` | `true` |

## Patch Management

### Container Updates

- Monitor [GitHub Releases](https://github.com/foundationallm/foundationallm/releases)
- Apply security updates promptly
- Test in staging before production

See [Updating Container Versions](../how-to-guides/updating-container-versions.md).

### Azure Resource Updates

- Enable Azure auto-updates where applicable
- Review Azure Security Center recommendations
- Apply Azure platform updates

## Compliance Considerations

### Data Residency

- Deploy in regions meeting data residency requirements
- Configure Cosmos DB and Storage geo-replication appropriately
- Review Azure OpenAI data processing locations

### Audit Logging

Enabled by default:
- Key Vault audit logs
- Cosmos DB diagnostic logs
- API request logs
- Authentication events

### Data Retention

Configure retention based on compliance needs:
- Default: 30 days in Log Analytics
- Extend or archive for compliance
- Export to storage for long-term retention

## Security Checklist

### Pre-Deployment

- [ ] Review and accept Azure OpenAI terms
- [ ] Plan network architecture
- [ ] Identify admin users/groups
- [ ] Prepare SSL certificates (Standard)
- [ ] Review compliance requirements

### Post-Deployment

- [ ] Verify Entra ID configuration
- [ ] Configure RBAC assignments
- [ ] Enable MFA for all users
- [ ] Configure monitoring alerts
- [ ] Review NSG rules
- [ ] Test authentication flow

### Ongoing

- [ ] Monitor security alerts
- [ ] Review access logs
- [ ] Update container images
- [ ] Rotate secrets/certificates
- [ ] Conduct security reviews

## Related Topics

- [Network Security Groups](network-security-groups.md)
- [Role-Based Access Control](role-based-access-control/index.md)
- [Authentication Setup](authentication-authorization/index.md)
- [Vulnerability Management](vulnerabilities.md)
- [Graph API Permissions](graph-api-permissions.md)
