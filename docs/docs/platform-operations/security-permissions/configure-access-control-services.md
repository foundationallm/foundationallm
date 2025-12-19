# Configure Access Control for Azure Services

FoundationaLLM uses Azure RBAC to control access to underlying Azure services. This guide covers granting access to App Configuration and Key Vault.

## Overview

FoundationaLLM follows least-privilege principles:
- Default: No user access
- Access must be explicitly granted
- Service accounts pre-configured by deployment

## Prerequisites

- FoundationaLLM deployed and running
- **Contributor** role on the resource group or subscription
- Azure Portal access

## Azure App Configuration

Azure App Configuration stores application settings and feature flags.

### Available Roles

| Role | Permissions | Use Case |
|------|-------------|----------|
| `App Configuration Data Reader` | Read settings and feature flags | Services, local development |
| `App Configuration Data Owner` | Read and write settings | Administrators |

### Grant Access

#### Via Azure Portal

1. Navigate to [Azure Portal](https://portal.azure.com/)
2. Go to your deployment resource group
   
   > **Note:** For ACA/AKS deployments, use the resource group **without** `ME_` or `MC_` prefix.

3. Select the **App Configuration** resource (name ends with `-appconfig`)
4. Select **Access control (IAM)** in left menu
5. Click **+ Add** > **Add role assignment**
6. Select role:
   - **App Configuration Data Reader** (for read access)
   - **App Configuration Data Owner** (for full access)
7. Click **Next**
8. Click **+ Select members**
9. Search for and select the user/group
10. Click **Select**
11. Click **Review + assign**

#### Via Azure CLI

```bash
# Get resource ID
APP_CONFIG_ID=$(az appconfig show \
  --name <app-config-name> \
  --resource-group <resource-group> \
  --query id -o tsv)

# Assign Reader role
az role assignment create \
  --assignee <user-or-group-id> \
  --role "App Configuration Data Reader" \
  --scope $APP_CONFIG_ID

# Or assign Owner role
az role assignment create \
  --assignee <user-or-group-id> \
  --role "App Configuration Data Owner" \
  --scope $APP_CONFIG_ID
```

## Azure Key Vault

Azure Key Vault stores secrets, certificates, and keys.

### Available Roles

| Role | Permissions | Use Case |
|------|-------------|----------|
| `Key Vault Secrets User` | Read secrets | Services, local development |
| `Key Vault Secrets Officer` | Read and write secrets | Administrators |
| `Key Vault Administrator` | Full management | Key Vault admins |

### Grant Access

#### Via Azure Portal

1. Navigate to your deployment resource group
2. Select the **Key Vault** resource (name ends with `-kv`)
3. Select **Access control (IAM)**
4. Click **+ Add** > **Add role assignment**
5. Select role:
   - **Key Vault Secrets User** (for read access)
   - **Key Vault Secrets Officer** (for management)
6. Click **Next**
7. Click **+ Select members**
8. Search for and select the user/group
9. Click **Select**
10. Click **Review + assign**

#### Via Azure CLI

```bash
# Get resource ID
KEYVAULT_ID=$(az keyvault show \
  --name <keyvault-name> \
  --resource-group <resource-group> \
  --query id -o tsv)

# Assign Secrets User role
az role assignment create \
  --assignee <user-or-group-id> \
  --role "Key Vault Secrets User" \
  --scope $KEYVAULT_ID

# Or assign Secrets Officer role
az role assignment create \
  --assignee <user-or-group-id> \
  --role "Key Vault Secrets Officer" \
  --scope $KEYVAULT_ID
```

## Other Azure Resources

### Storage Accounts

| Role | Permissions |
|------|-------------|
| `Storage Blob Data Reader` | Read blobs |
| `Storage Blob Data Contributor` | Read/write blobs |
| `Storage Blob Data Owner` | Full blob access |

### Cosmos DB

| Role | Permissions |
|------|-------------|
| `Cosmos DB Account Reader` | Read metadata |
| `Cosmos DB Operator` | Manage accounts |
| `DocumentDB Account Contributor` | Full access |

### Azure AI Search

| Role | Permissions |
|------|-------------|
| `Search Index Data Reader` | Read index data |
| `Search Index Data Contributor` | Read/write index data |
| `Search Service Contributor` | Manage service |

## Best Practices

| Practice | Description |
|----------|-------------|
| **Least Privilege** | Grant minimum required permissions |
| **Use Groups** | Assign roles to groups, not individuals |
| **Regular Review** | Audit role assignments periodically |
| **Just-in-Time** | Consider PIM for sensitive roles |
| **Document** | Track who has access and why |

## Troubleshooting

### Access Denied Errors

1. Verify role assignment exists
2. Check correct resource scope
3. Allow up to 30 minutes for propagation
4. Verify user/group object ID

### Check Existing Assignments

```bash
# List role assignments
az role assignment list \
  --resource-group <resource-group> \
  --output table

# Check specific resource
az role assignment list \
  --scope <resource-id> \
  --output table
```

## Related Topics

- [Platform Security](platform-security.md)
- [Role-Based Access Control](role-based-access-control/index.md)
- [App Configuration Values](../deployment/app-configuration-values.md)
