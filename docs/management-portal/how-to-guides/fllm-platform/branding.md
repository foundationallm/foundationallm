# Branding Configuration

Learn how to customize the look and feel of your FoundationaLLM deployment.

## Overview

Branding allows you to personalize both the Management Portal and Chat User Portal to match your organization's visual identity. Customizable elements include logos, colors, text, and messages.

## Accessing Branding Settings

1. In the Management Portal sidebar, click **Branding** under the **FLLM Platform** section
2. The branding configuration page loads with all customizable options

## Branding Configuration Interface

The branding page is organized into sections:

1. **General Settings** - Logo, text, and messages
2. **Color Settings** - Color scheme configuration
3. **Action Buttons** - Reset, Set Default, and Save

### Contrast Information Toggle

Enable **Show contrast information** to see WCAG accessibility compliance indicators for your color combinations.

## General Branding Settings

| Setting | Description |
|---------|-------------|
| **Company Name** | Organization name displayed in the portal |
| **FavIcon URL** | Path to the browser tab icon |
| **Footer Text** | Text displayed in the portal footer (supports rich text) |
| **Kiosk Mode** | Toggle for kiosk/public display mode |
| **Logo Text** | Text to display if logo image is unavailable |
| **Logo URL** | Path to your organization's logo image |
| **Page Title** | Browser tab/window title |
| **Agent Icon URL** | Default icon for agents |
| **No Agents Message** | Message shown when no agents are available (supports rich text) |
| **Default Agent Welcome Message** | Default greeting for agents without custom messages (supports rich text) |

### Rich Text Fields

Footer Text, No Agents Message, and Default Agent Welcome Message support rich text formatting:
- Bold, italic, underline
- Links
- Lists
- Basic HTML

## Color Settings

Colors are grouped by their functional area:

### Color Groups

| Group | Background | Text |
|-------|------------|------|
| **Accent** | Accent Color | Accent Text Color |
| **Background** | Background Color | - |
| **Primary** | Primary Color | Primary Text Color |
| **Secondary** | Secondary Color | Secondary Text Color |
| **Primary Button** | Primary Button Background | Primary Button Text |
| **Secondary Button** | Secondary Button Background | Secondary Button Text |

### Color Input Methods

Colors can be entered as:
- **Hex**: `#RRGGBB` or `#RGB` (e.g., `#131833`)
- **RGB**: `rgb(R, G, B)` (e.g., `rgb(19, 24, 51)`)

Use the color picker for visual selection or enter values directly.

### WCAG Accessibility Compliance

When **Show contrast information** is enabled:

| Indicator | Description |
|-----------|-------------|
| **Contrast Ratio** | Numerical ratio (e.g., "4.5:1") |
| **AA** | Pass/Fail for WCAG AA standard (4.5:1 minimum) |
| **AAA** | Pass/Fail for WCAG AAA standard (7:1 minimum) |

- **Green** = Passes the standard
- **Red** = Fails the standard

### Default Color Values

| Setting | Default Value |
|---------|---------------|
| Primary Color | `#131833` |
| Primary Text Color | `#fff` |
| Secondary Color | `#334581` |
| Secondary Text Color | `#fff` |
| Accent Color | `#fff` |
| Accent Text Color | `#131833` |
| Background Color | `#fff` |
| Primary Button Background | `#5472d4` |
| Primary Button Text | `#fff` |
| Secondary Button Background | `#70829a` |
| Secondary Button Text | `#fff` |

## Logo Configuration

### Logo Requirements

- **Format**: SVG, PNG, or JPG recommended
- **Size**: Max height ~100px in header display
- **Background**: Consider transparency for SVG/PNG

### Logo Preview

The page shows a live preview of your logo against the Primary Color background.

### Logo Path

The logo URL is relative to the portal's public directory. Common patterns:
- `foundationallm-logo-white.svg` (default)
- `custom-logo.png` (custom uploaded)

> **TODO**: Document the process for uploading custom logo files to the portal.

## Applying Changes

### Save Changes

1. Make your branding modifications
2. Click **Save** to apply changes
3. Changes are saved to Azure App Configuration
4. Users see updates after cache refresh (may require browser refresh)

### Reset Changes

- **Reset**: Reverts all unsaved changes to previously saved values
- Confirmation dialog appears before reset

### Set Default

- **Set Default**: Reverts all branding values to FoundationaLLM defaults
- Confirmation dialog appears before reset
- Still requires **Save** to apply

## Best Practices

### Visual Identity

- Use consistent colors from your brand guidelines
- Ensure logos are high quality and appropriate size
- Test appearance on both light and dark backgrounds (if applicable)

### Accessibility

- Maintain sufficient contrast ratios (aim for WCAG AA minimum)
- Test with the contrast information toggle enabled
- Consider users with visual impairments

### Testing

1. Preview changes in the Management Portal
2. Check the Chat User Portal appearance
3. Test on different screen sizes/devices
4. Verify readability of all text elements

## Configuration Methods

Branding can be configured through multiple methods:

| Method | Use Case |
|--------|----------|
| **Management Portal** | Interactive, visual configuration |
| **Azure App Configuration** | Programmatic, infrastructure-as-code |
| **REST API** | Automation and integration |

See reference documentation for alternative methods.

## Troubleshooting

### Changes Not Appearing

- Click Save to ensure changes are persisted
- Refresh the browser to clear cached styles
- Check browser developer tools for CSS loading issues

### Logo Not Displaying

- Verify the logo URL path is correct
- Check that the file exists in the public directory
- Ensure the file format is supported

### Colors Look Wrong

- Verify hex/RGB format is correct
- Check for transparency issues with logo
- Test in different browsers

## Related Topics

- [Branding Reference](../../reference/branding/index.md)
- [Using App Configuration for Branding](../../reference/branding/using-app-configuration.md)
- [Using Management Portal for Branding](../../reference/branding/using-management-portal.md)
- [Using REST API for Branding](../../reference/branding/using-rest-api.md)
