# Backups & Data Resiliency

This guide covers backup strategies and data resiliency options for FoundationaLLM platform components.

## Overview

Before implementing backup strategies, consider:

| Factor | Description |
|--------|-------------|
| **RTO** | Recovery Time Objective - acceptable downtime |
| **RPO** | Recovery Point Objective - acceptable data loss |
| **Compliance** | Regulatory requirements for data retention |
| **Cost** | Storage and compute costs for backups |

## Cosmos DB

Cosmos DB stores conversations, user sessions, and other application data.

### Automated Backups

| Feature | Standard Deployment Default |
|---------|----------------------------|
| **Backup Mode** | Continuous or Periodic |
| **Retention Period** | 30 days |
| **Backup Frequency** | Automatic |
| **Restore Granularity** | Point-in-time |

### Configuring Backup Policy

**Via Azure Portal:**
1. Navigate to your Cosmos DB account
2. Select **Settings** > **Backup & Restore**
3. Configure backup mode and retention

**Via Azure CLI:**
```bash
az cosmosdb update \
  --name <account-name> \
  --resource-group <resource-group> \
  --backup-policy-type Continuous
```

### Restoring from Backup

**Via Azure Portal:**
1. Navigate to Cosmos DB account
2. Select **Settings** > **Backup & Restore**
3. Click **Restore**
4. Select restore point and destination

**Via Azure CLI:**
```bash
az cosmosdb restore \
  --account-name <source-account> \
  --resource-group <resource-group> \
  --target-database-account-name <target-account> \
  --restore-timestamp "2024-01-01T00:00:00Z" \
  --location <region>
```

### Data Resiliency Options

| Feature | Description | Default |
|---------|-------------|---------|
| **Global Distribution** | Multi-region replication | Not enabled |
| **Consistency Levels** | Session, Strong, Eventual, etc. | Session |
| **Automatic Failover** | Regional failover | Not configured |

> **TODO:** Document procedure for enabling multi-region replication in Standard deployment.

## Storage Accounts

Storage accounts contain prompts, agents, data sources, and vectorized content.

### Built-in Protection

| Feature | Standard Deployment Default |
|---------|----------------------------|
| **Replication** | LRS (Locally Redundant) |
| **Blob Versioning** | Enabled |
| **Soft Delete (Blobs)** | 30 days |
| **Soft Delete (Containers)** | 30 days |

### Upgrading Replication

| Option | Description | Use Case |
|--------|-------------|----------|
| **LRS** | 3 copies in single datacenter | Development |
| **ZRS** | 3 copies across availability zones | Production |
| **GRS** | 6 copies across two regions | Disaster recovery |
| **RA-GRS** | GRS with read access to secondary | High availability |

**Change Replication:**
```bash
az storage account update \
  --name <account-name> \
  --resource-group <resource-group> \
  --sku Standard_GRS
```

### Azure Backup for Storage

Configure Azure Backup for blob data:

1. Navigate to **Recovery Services vault**
2. Select **+ Backup**
3. Choose **Azure Blob Storage**
4. Select storage account
5. Configure backup policy

### Manual Export

Export data for archival or migration:

```bash
# Export using AzCopy
azcopy copy "https://<account>.blob.core.windows.net/<container>/*" \
  "/local/backup/" \
  --recursive
```

## Key Vault

Key Vault stores secrets, certificates, and encryption keys.

### Built-in Protection

| Feature | Default |
|---------|---------|
| **Soft Delete** | Enabled (7-90 days) |
| **Purge Protection** | Enabled (7 days) |
| **Secret Versioning** | Automatic |

### Backing Up Secrets

**Via Azure Portal:**
1. Navigate to Key Vault
2. Select **Secrets**
3. Select a secret
4. Click **Download Backup**

**Via Azure CLI:**
```bash
# Backup single secret
az keyvault secret backup \
  --vault-name <vault-name> \
  --name <secret-name> \
  --file <backup-file>

# Restore secret
az keyvault secret restore \
  --vault-name <vault-name> \
  --file <backup-file>
```

### Backup All Secrets Script

```powershell
$vaultName = "<vault-name>"
$backupPath = "./keyvault-backup"

# Create backup directory
New-Item -ItemType Directory -Force -Path $backupPath

# Get all secrets
$secrets = az keyvault secret list --vault-name $vaultName --query "[].name" -o tsv

# Backup each secret
foreach ($secret in $secrets) {
    az keyvault secret backup `
        --vault-name $vaultName `
        --name $secret `
        --file "$backupPath/$secret.backup"
}
```

> **Note:** Key Vault does not support full vault backup. Back up individual items.

## App Configuration

App Configuration stores application settings and feature flags.

### Versioning

- Changes are automatically versioned
- Access history via **History** view in portal
- Rollback by restoring previous version

### Export Configuration

**Via Azure Portal:**
1. Navigate to App Configuration
2. Select **Import/export**
3. Select **Export**
4. Choose destination (file, App Service, another App Config)

**Via Azure CLI:**
```bash
# Export to file
az appconfig kv export \
  --name <app-config-name> \
  --destination file \
  --path ./config-backup.json \
  --format json
```

### Import Configuration

```bash
# Import from file
az appconfig kv import \
  --name <app-config-name> \
  --source file \
  --path ./config-backup.json \
  --format json
```

## Backup Schedule Recommendations

| Component | Frequency | Retention |
|-----------|-----------|-----------|
| Cosmos DB | Continuous | 30 days |
| Storage (critical) | Daily | 30 days |
| Key Vault secrets | Before changes | As needed |
| App Configuration | Before changes | As needed |

## Disaster Recovery

### Standard Deployment Considerations

| Component | DR Strategy |
|-----------|-------------|
| **AKS Clusters** | Redeploy from templates |
| **Cosmos DB** | Restore from backup |
| **Storage** | Restore from GRS secondary |
| **Key Vault** | Restore secrets from backup |
| **App Config** | Import from export |

### Recovery Procedure

1. **Assess** - Identify affected components
2. **Provision** - Deploy new infrastructure
3. **Restore** - Restore data from backups
4. **Configure** - Apply App Configuration
5. **Validate** - Test functionality
6. **Cutover** - Update DNS/routing

> **TODO:** Add detailed disaster recovery runbook for Standard deployment.

## Related Topics

- [Logs & Monitoring](../monitoring-troubleshooting/logs.md)
- [Troubleshooting](../monitoring-troubleshooting/troubleshooting.md)
- [Standard Deployment](../deployment/deployment-standard.md)
