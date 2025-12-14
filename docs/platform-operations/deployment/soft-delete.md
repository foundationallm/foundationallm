# Soft Delete in Azure Resources

Several Azure resources used by FoundationaLLM implement soft delete, which retains deleted resources for a period before permanent deletion.

## Overview

Soft delete protects against accidental deletion but requires consideration during:
- Redeployment with same resource names
- Subscription cleanup
- Quota management

## Resources with Soft Delete

| Resource Type | Default Retention | Impact |
|--------------|-------------------|--------|
| Azure OpenAI | 48 hours | Quota remains allocated |
| Azure Key Vault | 7-90 days | Name unavailable |
| Azure AI Search | 7 days | Name unavailable |
| Azure Content Safety | 48 hours | Quota remains allocated |

## Implications

### Resource Naming

When redeploying after deletion:
- Soft-deleted resources block reuse of the same name
- Either purge the resource OR use a different name

### Quota Management

Soft-deleted Azure OpenAI resources:
- Continue consuming quota allocation
- May prevent new deployments in the same region
- Must be purged to release quota

## Purging Resources

### Using azd down

The recommended method for complete cleanup:

```powershell
azd down --purge
```

The `--purge` flag:
- Deletes all deployed resources
- Purges soft-deleted resources
- Releases all quotas and name reservations

### Manual Purging

#### Azure OpenAI

```bash
# List deleted resources
az cognitiveservices account list-deleted

# Purge specific resource
az cognitiveservices account purge \
  --location <region> \
  --resource-group <resource-group> \
  --name <account-name>
```

#### Key Vault

**Via Portal:**
1. Navigate to **Key Vaults**
2. Select **Manage deleted vaults**
3. Select the vault
4. Click **Purge**

**Via CLI:**
```bash
# List deleted vaults
az keyvault list-deleted

# Purge specific vault
az keyvault purge --name <vault-name>
```

#### Azure AI Search

**Via Portal:**
1. Navigate to **Azure AI Search**
2. The deleted service will show in soft-deleted state
3. Select and purge

**Via CLI:**
```bash
# Currently requires portal or REST API
```

#### Azure Content Safety

```bash
# Similar to Azure OpenAI
az cognitiveservices account purge \
  --location <region> \
  --resource-group <resource-group> \
  --name <account-name>
```

## Best Practices

### Before Redeployment

1. **Check for Soft-Deleted Resources**
   ```bash
   az keyvault list-deleted
   az cognitiveservices account list-deleted
   ```

2. **Purge if Reusing Names**
   ```bash
   azd down --purge
   ```

3. **Or Use Different Names**
   - Update resource names in deployment manifest/parameters

### Regular Cleanup

1. **Audit Soft-Deleted Resources**
   - Periodically review deleted resources
   - Purge unnecessary ones to release quota

2. **Document Naming Conventions**
   - Track resource names used
   - Plan for naming collisions

### Quota Planning

1. **Monitor Quota Usage**
   - Track Azure OpenAI TPM/RPM limits
   - Account for soft-deleted resources

2. **Plan Regional Distribution**
   - Spread deployments across regions if hitting limits

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| "Name already exists" | Soft-deleted resource | Purge or use different name |
| Quota exceeded | Soft-deleted resources consuming quota | Purge resources |
| Cannot recreate Key Vault | Soft-delete retention | Wait for retention period or purge |
| Deployment fails | Previous soft-deleted resources | Run `azd down --purge` first |

## Disabling Soft Delete

> **Warning:** Disabling soft delete removes protection against accidental deletion.

### Key Vault

Soft delete is mandatory for Key Vault and cannot be disabled.

### Other Resources

Some resources allow disabling soft delete during creation, but this is not recommended for production environments.

## Related Topics

- [Quick Start Deployment](deployment-quick-start.md)
- [Standard Deployment](deployment-standard.md)
- [Troubleshooting](../monitoring-troubleshooting/troubleshooting.md)
