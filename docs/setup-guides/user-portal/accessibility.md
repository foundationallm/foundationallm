# Accessibility Features

FoundationaLLM is committed to providing an accessible User Portal experience that meets or exceeds Web Content Accessibility Guidelines (WCAG) 2.1 Level AA standards.

## Overview

The User Portal includes comprehensive accessibility features to ensure all users, including those with disabilities, can effectively interact with AI agents.

## WCAG 2.1 Compliance

### Compliance Level

FoundationaLLM targets **WCAG 2.1 Level AA** compliance, which includes:

- **Level A**: Minimum accessibility requirements
- **Level AA**: Addresses the most common barriers for users with disabilities

### Four Principles of Accessibility (POUR)

| Principle | Description | Implementation |
|-----------|-------------|----------------|
| **Perceivable** | Information must be presentable to users | Alt text, color contrast, captions |
| **Operable** | Interface must be operable by all users | Keyboard navigation, focus management |
| **Understandable** | Information must be understandable | Clear labels, error messages, consistent navigation |
| **Robust** | Content must be robust enough for assistive technologies | Semantic HTML, ARIA attributes |

## Keyboard Navigation

### Full Keyboard Support

All User Portal functionality is accessible via keyboard:

| Action | Keyboard Shortcut |
|--------|-------------------|
| Navigate between elements | `Tab` / `Shift + Tab` |
| Activate buttons/links | `Enter` or `Space` |
| Close dialogs | `Escape` |
| Navigate dropdown options | `Arrow Up` / `Arrow Down` |
| Select dropdown option | `Enter` |
| Open agent selector | <!-- [TODO: Document keyboard shortcut] --> |
| Send message | `Enter` (in chat input) |
| New line in message | `Shift + Enter` |

### Focus Indicators

All interactive elements display visible focus indicators:

- Buttons show focus rings when tabbed to
- Form inputs show clear border highlight on focus
- Links show underline and color change on focus

<!-- [TODO: Add screenshot showing focus indicators] -->

## Screen Reader Compatibility

### Tested Screen Readers

The User Portal has been tested with:

- **NVDA** (Windows)
- **JAWS** (Windows)
- **VoiceOver** (macOS/iOS)
- **TalkBack** (Android)

### ARIA Implementation

The User Portal uses ARIA (Accessible Rich Internet Applications) attributes:

| Component | ARIA Features |
|-----------|--------------|
| Agent dropdown | `role="listbox"`, `aria-expanded`, `aria-selected` |
| Chat messages | `role="log"`, `aria-live="polite"` |
| Dialogs | `role="dialog"`, `aria-modal`, `aria-labelledby` |
| Form inputs | `aria-label`, `aria-required`, `aria-invalid` |
| Buttons | `aria-label` for icon-only buttons |
| Loading states | `aria-busy`, `aria-live` announcements |

### Screen Reader Announcements

The portal announces:

- New chat messages as they arrive
- Loading states and completion
- Error messages
- Form validation results
- Agent selection changes

## Color Contrast

### Contrast Requirements

FoundationaLLM meets WCAG 2.1 contrast requirements:

| Element Type | Required Ratio | Target |
|--------------|---------------|--------|
| Normal text | 4.5:1 minimum | AA |
| Large text (18pt+) | 3:1 minimum | AA |
| UI components | 3:1 minimum | AA |

### Branding and Contrast

When customizing branding colors, the Management Portal provides contrast information:

- **AA rating**: Meets minimum requirements
- **AAA rating**: Meets enhanced requirements

See [Branding Management Portal](../branding/branding-management-portal.md) for contrast checking tools.

### High Contrast Mode

The User Portal respects operating system high contrast settings:

- Windows High Contrast mode detected automatically
- macOS Increase Contrast detected automatically
- Custom high contrast themes honored

## Text and Typography

### Text Sizing

- Base font size: 16px
- All text can be resized up to 200% without loss of functionality
- Responsive layout accommodates text zoom

### Readability

- Minimum line height: 1.5
- Clear sans-serif fonts
- Adequate paragraph spacing
- No justified text (left-aligned for easier reading)

## Forms and Inputs

### Form Field Accessibility

All form fields include:

| Feature | Description |
|---------|-------------|
| **Labels** | All fields have visible, associated labels |
| **Instructions** | Help text provided for complex fields |
| **Required indicators** | Required fields marked with asterisk (*) |
| **Error messages** | Clear, specific error messages |
| **Focus order** | Logical tab order through fields |

### Error Handling

Form errors are communicated through:

1. **Visual indication**: Red border and error icon
2. **Text message**: Specific error description
3. **ARIA announcement**: Error read by screen readers
4. **Focus management**: Focus moves to first error

Example error format:
```
[Error icon] Agent name is required. Please enter a name for your agent.
```

## Chat Interface Accessibility

### Message Accessibility

Each chat message includes:

- Sender identification (user or agent name)
- Timestamp (available to screen readers)
- Message content
- Any attachments with descriptive text

### Conversation Navigation

- Messages are navigable via screen reader
- New messages announced automatically
- Option to pause auto-announcements

### File Uploads

File upload accessibility features:

- Drag-and-drop has keyboard alternative (button)
- File type restrictions announced
- Upload progress announced
- Success/failure clearly communicated

## Agent Selector Accessibility

The agent dropdown is fully accessible:

| Feature | Implementation |
|---------|----------------|
| Keyboard operable | Arrow keys, Enter, Escape |
| Screen reader compatible | ARIA listbox pattern |
| Focus management | Focus returns to trigger after close |
| Clear selections | Currently selected agent announced |
| Search functionality | Type-ahead search in list |

## Explainer Text and Help

Throughout the User Portal, explainer text helps users understand features:

- Tooltips accessible via keyboard focus
- Help icons with descriptive text
- Context-sensitive help for complex operations
- Documentation links open in new tabs (announced)

<!-- [TODO: Document specific explainer text patterns used] -->

## Known Limitations

The following accessibility issues are being addressed:

<!-- [TODO: List known accessibility limitations] -->
<!-- [TODO: Provide timeline for remediation] -->

| Issue | Status | Target Fix |
|-------|--------|------------|
| *To be documented* | | |

## Reporting Accessibility Issues

If you encounter accessibility barriers:

1. **Contact support**: https://foundationallm.ai/contact
2. **Include details**:
   - Browser and version
   - Assistive technology used
   - Steps to reproduce the issue
   - Impact on your workflow

We are committed to addressing accessibility issues promptly.

## Testing Accessibility

### Automated Testing

FoundationaLLM uses automated accessibility testing:

- axe DevTools integration
- Lighthouse audits
- Pa11y CI testing

### Manual Testing

Regular manual testing includes:

- Keyboard-only navigation testing
- Screen reader testing
- Color contrast verification
- Cognitive load assessment

## Related Topics

- [Branding Management Portal](../branding/branding-management-portal.md) - Includes contrast checker
- [User Portal Guide](index.md) - Main User Portal documentation

## External Resources

- [Web Content Accessibility Guidelines (WCAG) 2.1](https://www.w3.org/WAI/WCAG21/quickref/)
- [WAI-ARIA Authoring Practices](https://www.w3.org/WAI/ARIA/apg/)
- [Microsoft Inclusive Design](https://inclusive.microsoft.design/)
