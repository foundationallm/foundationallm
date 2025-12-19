# Branding Reference

Reference documentation for customizing the visual appearance of your FoundationaLLM deployment.

## Overview

FoundationaLLM provides comprehensive branding customization for both the Management Portal and Chat User Portal. You can customize logos, colors, text, and messages to match your organization's visual identity.

## Configuration Methods

| Method | Description | Best For |
|--------|-------------|----------|
| [Management Portal](using-management-portal.md) | Interactive UI-based configuration | Real-time changes, visual preview |
| [App Configuration](using-app-configuration.md) | Azure App Configuration values | Infrastructure-as-code, deployment automation |
| [REST API](using-rest-api.md) | Programmatic updates | Custom tooling, automation |

## Branding Elements

### Logo Configuration

| Setting | Key | Default |
|---------|-----|---------|
| Logo URL | `FoundationaLLM:Branding:LogoUrl` | `foundationallm-logo-white.svg` |
| Logo Text | `FoundationaLLM:Branding:LogoText` | `FoundationaLLM` |
| FavIcon URL | `FoundationaLLM:Branding:FavIconUrl` | `favicon.ico` |
| Agent Icon URL | `FoundationaLLM:Branding:AgentIconUrl` | (empty) |

### Text Configuration

| Setting | Key | Default |
|---------|-----|---------|
| Company Name | `FoundationaLLM:Branding:CompanyName` | `FoundationaLLM` |
| Page Title | `FoundationaLLM:Branding:PageTitle` | `FoundationaLLM User Portal` |
| Footer Text | `FoundationaLLM:Branding:FooterText` | `FoundationaLLM © All rights reserved.` |
| No Agents Message | `FoundationaLLM:Branding:NoAgentsMessage` | System default message |
| Default Welcome Message | `FoundationaLLM:Branding:DefaultAgentWelcomeMessage` | `Start the conversation using the text box below.` |

### Color Configuration

| Setting | Key | Default |
|---------|-----|---------|
| Primary Color | `FoundationaLLM:Branding:PrimaryColor` | `#131833` |
| Primary Text Color | `FoundationaLLM:Branding:PrimaryTextColor` | `#fff` |
| Secondary Color | `FoundationaLLM:Branding:SecondaryColor` | `#334581` |
| Secondary Text Color | `FoundationaLLM:Branding:SecondaryTextColor` | `#fff` |
| Accent Color | `FoundationaLLM:Branding:AccentColor` | `#fff` |
| Accent Text Color | `FoundationaLLM:Branding:AccentTextColor` | `#131833` |
| Background Color | `FoundationaLLM:Branding:BackgroundColor` | `#fff` |
| Primary Button Background | `FoundationaLLM:Branding:PrimaryButtonBackgroundColor` | `#5472d4` |
| Primary Button Text | `FoundationaLLM:Branding:PrimaryButtonTextColor` | `#fff` |
| Secondary Button Background | `FoundationaLLM:Branding:SecondaryButtonBackgroundColor` | `#70829a` |
| Secondary Button Text | `FoundationaLLM:Branding:SecondaryButtonTextColor` | `#fff` |

### Mode Configuration

| Setting | Key | Default |
|---------|-----|---------|
| Kiosk Mode | `FoundationaLLM:Branding:KioskMode` | `false` |

## Color Format Support

Colors can be specified in:
- **Hex**: `#RRGGBB` or `#RGB` (e.g., `#131833`, `#fff`)
- **RGB**: `rgb(R, G, B)` (e.g., `rgb(19, 24, 51)`)

## Accessibility Guidelines

### WCAG Contrast Requirements

| Standard | Minimum Ratio | Recommendation |
|----------|---------------|----------------|
| **AA** | 4.5:1 | Required for text |
| **AAA** | 7:1 | Recommended for enhanced accessibility |

### Testing Contrast

The Management Portal branding page includes contrast testing:
1. Enable "Show contrast information"
2. Review the ratio between background and text colors
3. Check AA and AAA compliance indicators

## Rich Text Fields

The following fields support HTML formatting:
- Footer Text
- No Agents Message
- Default Agent Welcome Message

Supported HTML elements:
- `<strong>`, `<b>` - Bold text
- `<em>`, `<i>` - Italic text
- `<a href="">` - Links
- `<ul>`, `<ol>`, `<li>` - Lists
- `<p>` - Paragraphs

## Related Topics

- [Branding Configuration (How-To)](../../how-to-guides/fllm-platform/branding.md)
- [Using Management Portal for Branding](using-management-portal.md)
- [Using App Configuration for Branding](using-app-configuration.md)
- [Using REST API for Branding](using-rest-api.md)
