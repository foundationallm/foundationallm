# Using Management Portal for Branding

Configure branding settings interactively through the Management Portal UI.

## Overview

The Management Portal provides an interactive interface for branding customization with:
- Real-time preview of color changes
- WCAG accessibility compliance indicators
- Rich text editing for text fields
- Visual logo preview

## Accessing Branding Settings

1. Navigate to the Management Portal
2. In the sidebar, click **Branding** under **FLLM Platform**

## Interface Overview

The branding page is organized into sections:

### General Settings Section

Configure non-color branding elements:
- Company Name
- FavIcon URL
- Footer Text (rich text editor)
- Kiosk Mode toggle
- Logo Text
- Logo URL (with preview)
- Page Title
- Agent Icon URL
- No Agents Message (rich text editor)
- Default Agent Welcome Message (rich text editor)

### Color Settings Section

Color configuration organized by functional area:
- Accent colors (background + text)
- Background color
- Primary colors (background + text)
- Secondary colors (background + text)
- Primary button colors
- Secondary button colors

## Making Changes

### Text Fields

1. Locate the setting to change
2. Enter the new value in the text input
3. Changes are staged (not saved until you click Save)

### Rich Text Fields

For Footer Text, No Agents Message, and Default Welcome Message:
1. Click in the editor field
2. Use the toolbar for formatting (bold, italic, links, etc.)
3. Enter your content
4. Changes are staged until saved

### Colors

1. Locate the color setting to change
2. Either:
   - Click the color swatch to open the color picker
   - Enter a hex or RGB value directly
3. Review the preview square
4. Check contrast compliance if enabled

### Logo

1. Enter the logo URL (relative to public directory)
2. View the preview against the Primary Color background
3. Ensure the logo is visible and appropriate size

## Accessibility Testing

Enable accessibility testing:

1. Toggle **Show contrast information** at the top of the page
2. For each color pair, you'll see:
   - Contrast ratio (e.g., "4.5:1")
   - AA compliance (Pass/Fail)
   - AAA compliance (Pass/Fail)

## Saving Changes

### Save Button

Click **Save** to apply all changes:
- Changes are persisted to Azure App Configuration
- Users see updates after browser refresh

### Reset Button

Click **Reset** to discard all unsaved changes:
- Reverts to last saved values
- Confirmation dialog appears

### Set Default Button

Click **Set Default** to restore factory defaults:
- Reverts all values to FoundationaLLM defaults
- Still requires Save to apply
- Confirmation dialog appears

## Validation

The interface validates:
- Color format (hex/RGB)
- URL formats (for logo/favicon)
- Required fields

## Best Practices

1. **Test on both portals** - Changes affect both Management and User portals
2. **Check accessibility** - Enable contrast information to verify WCAG compliance
3. **Preview logos** - Use the preview to verify logo appearance
4. **Save incrementally** - Save after major changes to avoid losing work

## Related Topics

- [Branding Reference](index.md)
- [Using App Configuration for Branding](using-app-configuration.md)
- [Branding Configuration (How-To)](../../how-to-guides/fllm-platform/branding.md)
