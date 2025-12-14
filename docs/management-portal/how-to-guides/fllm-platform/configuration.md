# Platform Configuration

Learn how to configure platform-wide settings in the Management Portal.

## Overview

Platform configuration manages settings that affect the entire FoundationaLLM deployment.

## Accessing Configuration

1. Navigate to **FLLM Platform** in the sidebar
2. Click **Configuration**

## Configuration Categories

### API Settings
- Rate limiting
- Timeout values
- Cache settings

### Security Settings
- Default authentication requirements
- Session timeouts
- Security headers

### Feature Flags
- Enable/disable features
- Preview features
- Experimental capabilities

## Making Changes

1. Select the configuration category
2. Modify the values
3. Save changes
4. Some changes may require service restart

## Configuration Sources

Settings are stored in:
- Azure App Configuration
- Key Vault (for secrets)
- Database (for runtime settings)

## Related Topics

- [App Configuration Values](../../../platform-operations/deployment/app-configuration-values.md)
- [Deployment Configuration](../../../platform-operations/deployment/deployment-configuration.md)
