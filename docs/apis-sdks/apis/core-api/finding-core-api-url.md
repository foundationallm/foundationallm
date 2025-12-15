# Finding Your Core API URL

This guide explains how to locate the Core API URL for your FoundationaLLM deployment.

## Overview

The Core API URL depends on your deployment type:

| Deployment Type | URL Pattern |
|-----------------|-------------|
| Azure Container Apps (ACA) | Container app URL ending in `coreca` |
| Azure Kubernetes Service (AKS) | Cluster FQDN with `/core` path |

## Method 1: Azure App Configuration

The easiest way to find your Core API URL:

1. Open the [Azure Portal](https://portal.azure.com)
2. Navigate to your FoundationaLLM resource group
3. Open the **App Configuration** resource
4. Select **Configuration explorer**
5. Search for: `FoundationaLLM:APIs:CoreAPI:APIUrl`
6. Copy the value

## Method 2: Azure Container Apps (ACA) Deployment

For ACA deployments:

1. Navigate to your FoundationaLLM resource group in Azure Portal
2. Find the **Container App** resource with a name ending in `coreca`
   
   Example: `fllm001coreca`

3. In the Container App **Overview** pane, copy the **Application Url**

   Example: `https://fllmaca002coreca.graybush-c554b849.eastus.azurecontainerapps.io`

## Method 3: Azure Kubernetes Service (AKS) Deployment

For AKS deployments:

1. Navigate to your FoundationaLLM resource group in Azure Portal
2. Open the **Kubernetes Service** resource
3. Select **Properties** in the left menu
4. Copy the **HTTP application routing domain**
5. Append `/core` to the domain

   Example: If the domain is `https://1cf699fd0d89446eabf2.eastus.aksapp.io/`, 
   then the Core API URL is `https://1cf699fd0d89446eabf2.eastus.aksapp.io/core`

## Method 4: Azure CLI

Use the Azure CLI to retrieve the URL:

### For ACA:

```bash
# List container apps and find the core API
az containerapp list \
  --resource-group <your-resource-group> \
  --query "[?contains(name, 'coreca')].properties.configuration.ingress.fqdn" \
  -o tsv
```

### For AKS:

```bash
# Get the AKS cluster's HTTP routing domain
az aks show \
  --resource-group <your-resource-group> \
  --name <your-aks-cluster> \
  --query "addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName" \
  -o tsv
```

## Verification

Verify your Core API URL is correct by accessing the status endpoint:

```bash
curl https://{your-core-api-url}/status
```

Expected response:

```json
{
  "status": "ready",
  "name": "CoreAPI",
  "version": "x.x.x"
}
```

Or access the Swagger UI (ACA only):

```
https://{your-core-api-url}/swagger/
```

## URL Formats Summary

| Deployment | Format | Example |
|------------|--------|---------|
| ACA Quick Start | `https://{prefix}coreca.{env}.{region}.azurecontainerapps.io` | `https://fllmaca002coreca.graybush-c554b849.eastus.azurecontainerapps.io` |
| ACA Standard | `https://{prefix}coreca.{env}.{region}.azurecontainerapps.io` | `https://fllmprod-coreca.fllmenv.westus2.azurecontainerapps.io` |
| AKS | `https://{cluster-fqdn}/core` | `https://1cf699fd0d89446eabf2.eastus.aksapp.io/core` |

## Troubleshooting

### URL Returns 404

- Verify the URL path is correct (AKS requires `/core`)
- Check that the Container App or AKS ingress is running

### URL Returns 401/403

- The API is accessible but requires authentication
- This confirms the URL is correct

### Cannot Access URL

- Check network connectivity
- Verify the deployment is running
- For AKS, ensure the ingress controller is configured

## Related Topics

- [Core API Overview](index.md)
- [Directly Calling Core API](directly-calling-core-api.md)
- [Standard Deployment Local API Access](standard-deployment-local-api-access.md)
