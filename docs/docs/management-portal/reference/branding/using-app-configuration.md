# Using App Configuration for Branding

Configure branding settings through Azure App Configuration for infrastructure-as-code deployments.

## Overview

Azure App Configuration provides a centralized store for branding settings, enabling:
- Infrastructure-as-code management
- Deployment automation
- Environment-specific configurations
- Version control of settings

## Configuration Keys

All branding keys follow the pattern: `FoundationaLLM:Branding:{SettingName}`

### Text Settings

| Key | Type | Description |
|-----|------|-------------|
| `FoundationaLLM:Branding:CompanyName` | String | Organization name |
| `FoundationaLLM:Branding:PageTitle` | String | Browser tab title |
| `FoundationaLLM:Branding:LogoText` | String | Text if logo unavailable |
| `FoundationaLLM:Branding:LogoUrl` | String | Logo image path |
| `FoundationaLLM:Branding:FavIconUrl` | String | Favicon path |
| `FoundationaLLM:Branding:AgentIconUrl` | String | Default agent icon path |
| `FoundationaLLM:Branding:FooterText` | String | Footer HTML |
| `FoundationaLLM:Branding:NoAgentsMessage` | String | No agents message HTML |
| `FoundationaLLM:Branding:DefaultAgentWelcomeMessage` | String | Default welcome HTML |

### Color Settings

| Key | Type | Format |
|-----|------|--------|
| `FoundationaLLM:Branding:PrimaryColor` | String | Hex or RGB |
| `FoundationaLLM:Branding:PrimaryTextColor` | String | Hex or RGB |
| `FoundationaLLM:Branding:SecondaryColor` | String | Hex or RGB |
| `FoundationaLLM:Branding:SecondaryTextColor` | String | Hex or RGB |
| `FoundationaLLM:Branding:AccentColor` | String | Hex or RGB |
| `FoundationaLLM:Branding:AccentTextColor` | String | Hex or RGB |
| `FoundationaLLM:Branding:BackgroundColor` | String | Hex or RGB |
| `FoundationaLLM:Branding:PrimaryButtonBackgroundColor` | String | Hex or RGB |
| `FoundationaLLM:Branding:PrimaryButtonTextColor` | String | Hex or RGB |
| `FoundationaLLM:Branding:SecondaryButtonBackgroundColor` | String | Hex or RGB |
| `FoundationaLLM:Branding:SecondaryButtonTextColor` | String | Hex or RGB |

### Mode Settings

| Key | Type | Values |
|-----|------|--------|
| `FoundationaLLM:Branding:KioskMode` | String | `true` or `false` |

## Setting Values via Azure Portal

1. Navigate to your Azure App Configuration resource
2. Click **Configuration explorer**
3. Click **+ Create** > **Key-value**
4. Enter the key (e.g., `FoundationaLLM:Branding:CompanyName`)
5. Enter the value
6. Click **Apply**

## Setting Values via Azure CLI

```bash
# Set a text value
az appconfig kv set \
  --name <app-config-name> \
  --key "FoundationaLLM:Branding:CompanyName" \
  --value "Contoso"

# Set a color value
az appconfig kv set \
  --name <app-config-name> \
  --key "FoundationaLLM:Branding:PrimaryColor" \
  --value "#1a2b3c"
```

## Setting Values via Bicep/ARM

```bicep
resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: appConfigName
}

resource brandingCompanyName 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfig
  name: 'FoundationaLLM:Branding:CompanyName'
  properties: {
    value: 'Contoso'
  }
}

resource brandingPrimaryColor 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfig
  name: 'FoundationaLLM:Branding:PrimaryColor'
  properties: {
    value: '#1a2b3c'
  }
}
```

## Environment-Specific Configuration

Use labels for environment-specific settings:

```bash
# Development
az appconfig kv set \
  --name <app-config-name> \
  --key "FoundationaLLM:Branding:PageTitle" \
  --value "FoundationaLLM (Dev)" \
  --label "dev"

# Production
az appconfig kv set \
  --name <app-config-name> \
  --key "FoundationaLLM:Branding:PageTitle" \
  --value "FoundationaLLM" \
  --label "prod"
```

## Value Format Notes

### Color Values

Supported formats:
- `#RRGGBB` (6-digit hex)
- `#RGB` (3-digit hex, expanded to 6-digit)
- `rgb(R, G, B)` (RGB function)

### HTML Values

For rich text fields (Footer, Messages), use properly escaped HTML:
- Escape special characters if needed
- Ensure HTML is valid

## Cache Considerations

App Configuration values are cached:
- Portal applications refresh periodically
- Users may need to refresh browser to see changes
- Consider cache timing when deploying updates

## Related Topics

- [Branding Reference](index.md)
- [Using Management Portal for Branding](using-management-portal.md)
- [Using REST API for Branding](using-rest-api.md)
