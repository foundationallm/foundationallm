# Custom Domains

Configure custom domain names for FoundationaLLM portals and APIs to match your organization's branding.

## Overview

Both Azure Container Apps (ACA) and Azure Kubernetes Service (AKS) deployments support custom domains with SSL certificates.

| Deployment Type | Certificate Management |
|-----------------|----------------------|
| Quick Start (ACA) | Azure managed or custom |
| Standard (AKS) | Custom certificates required |

## Azure Container Apps (Quick Start)

### Adding a Custom Domain

1. **Open Azure Portal**
   - Navigate to your subscription and resource group

2. **Select Container App**
   - Choose the ACA instance (e.g., `chat` or `management`)

3. **Configure Domain**
   - Select **Settings** > **Custom domains**
   - Click **Add custom domain**
   - Enter your domain name

4. **Configure DNS**
   - Note the DNS records displayed in the dialog:
     - **CNAME** record pointing to your Container App
     - **TXT** record for domain verification

5. **Validate Domain**
   - Add the required DNS records to your DNS provider
   - Return to Azure Portal and click **Validate**

6. **Add Certificate**
   - Choose **Managed certificate** (free, auto-renewed) OR
   - Upload a custom certificate
   - Click **Add**

### Certificate Options

| Option | Description | Use Case |
|--------|-------------|----------|
| **Managed Certificate** | Free Azure-managed SSL | Most deployments |
| **Custom Certificate** | Upload your own PFX | Enterprise requirements |

> **Note:** Managed certificates may take a few minutes to provision.

For detailed instructions, see [Custom domain names and certificates in Azure Container Apps](https://learn.microsoft.com/azure/container-apps/custom-domains-certificates).

## Azure Kubernetes Service (Standard)

### Pre-Deployment SSL Setup

For Standard deployments, SSL certificates must be provisioned before deployment.

1. **Obtain Certificates**
   - Acquire SSL certificates for each domain
   - Export to PFX format with private key

2. **Place Certificate Files**
   ```
   deploy/standard/certs/
   ├── api.example.com.pfx
   ├── management-api.example.com.pfx
   ├── chat.example.com.pfx
   └── management.example.com.pfx
   ```

3. **Configure Deployment Manifest**
   - Update `Deployment-Manifest.json` with hostnames

4. **Deploy**
   - Certificates are automatically configured during deployment

### Certificate Requirements

| Service | Hostname Example | File Name |
|---------|------------------|-----------|
| Core API | `api.example.com` | `api.example.com.pfx` |
| Management API | `management-api.example.com` | `management-api.example.com.pfx` |
| Chat Portal | `chat.example.com` | `chat.example.com.pfx` |
| Management Portal | `management.example.com` | `management.example.com.pfx` |

For detailed instructions, see [Set up a custom domain name and SSL certificate with the application routing add-on](https://learn.microsoft.com/azure/aks/app-routing-dns-ssl).

## Update Entra ID Redirect URIs

After configuring custom domains, update the App Registration redirect URIs.

### Chat Portal (User Portal)

1. Navigate to **Azure Portal** > **Microsoft Entra ID**
2. Select **App registrations**
3. Search for and select the Chat UI app registration
4. Select **Manage** > **Authentication**
5. In **Single-page application Redirect URIs**, add:
   ```
   https://your-custom-domain.com/signin-oidc
   ```
6. Click **Save**

### Management Portal

1. Navigate to **App registrations**
2. Search for and select the Management UI app registration
3. Select **Manage** > **Authentication**
4. In **Single-page application Redirect URIs**, add:
   ```
   https://your-custom-domain.com/management/signin-oidc
   ```
5. Click **Save**

### Using the Update Script

For Quick Start deployments, run:

```powershell
cd deploy/quick-start
../common/scripts/Update-OAuthCallbackUris.ps1
```

## DNS Configuration

### Required DNS Records

| Record Type | Name | Value |
|-------------|------|-------|
| **CNAME** | `chat` | `<aca-fqdn>` or `<aks-ingress-ip>` |
| **CNAME** | `management` | `<aca-fqdn>` or `<aks-ingress-ip>` |
| **CNAME** | `api` | `<aca-fqdn>` or `<aks-ingress-ip>` |
| **A** (AKS only) | Various | Ingress IP address |

### Private DNS (Standard Deployment)

For Standard deployments with private networking:

1. Deployment generates `hosts` file in `deploy/standard/config/`
2. Add entries to:
   - Local `hosts` file for testing, OR
   - Organization's private DNS server

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Certificate not trusted | Ensure certificate chain is complete |
| DNS validation failed | Check DNS propagation (allow 24-48 hours) |
| Redirect loop | Verify redirect URIs in App Registration |
| Mixed content warnings | Ensure all resources use HTTPS |

## Best Practices

1. **Use Managed Certificates** (ACA) when possible for automatic renewal
2. **Plan DNS Changes** in advance - propagation takes time
3. **Test in Staging** before updating production
4. **Monitor Certificate Expiry** for custom certificates
5. **Document Custom Domains** for team reference

## Related Topics

- [Quick Start Deployment](deployment-quick-start.md)
- [Standard Deployment](deployment-standard.md)
- [Authentication Setup](security-permissions/authentication-authorization/index.md)
- [Deployment Manifest](standard-manifest.md)
