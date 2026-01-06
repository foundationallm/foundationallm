# Configuring Accessibility Settings

Customize the Chat User Portal to meet your visual and notification preferences. FoundationaLLM is designed with accessibility in mind, supporting WCAG (Web Content Accessibility Guidelines) compliance.

## WCAG Compliance Overview

The Chat User Portal is built following WCAG 2.1 Level AA guidelines to ensure accessibility for users with diverse abilities:

| WCAG Principle | Implementation |
|----------------|----------------|
| **Perceivable** | Text alternatives, adaptable content, distinguishable elements |
| **Operable** | Keyboard accessible, sufficient time, navigable |
| **Understandable** | Readable, predictable, input assistance |
| **Robust** | Compatible with assistive technologies |

### Key Accessibility Features

| Feature | Description |
|---------|-------------|
| **Keyboard Navigation** | Full functionality without a mouse |
| **Screen Reader Support** | ARIA labels and semantic HTML |
| **Text Scaling** | Adjustable text size up to 200% |
| **Color Independence** | Information not conveyed by color alone |
| **Focus Indicators** | Visible focus states on interactive elements |
| **Error Identification** | Clear error messages and guidance |

## Accessing Accessibility Settings

1. Click the **Settings** button (gear icon ⚙️) in the bottom-left corner of the sidebar, next to your name
2. In the Settings dialog, click the **Accessibility** tab
3. Adjust settings as needed
4. Click **Close** when finished — changes are applied immediately

## Available Settings

### Auto-Hide Popup Notifications

Control whether notification messages (toasts) disappear automatically or stay visible until you dismiss them.

| Setting | Behavior |
|---------|----------|
| **On** (default) | Notifications fade away after a few seconds |
| **Off** | Notifications stay visible until you click to dismiss them |

**When to disable auto-hide:**
- If you need more time to read notifications
- If you use a screen reader and want notifications to remain accessible
- If you frequently miss important status messages

**How to change:**
1. Find the **Auto hide popup notifications** toggle
2. Click the toggle to switch between On and Off

### Text Size

Adjust the overall text size throughout the portal to improve readability.

| Size | Best For |
|------|----------|
| **80%** | Seeing more content on screen |
| **100%** (default) | Standard viewing |
| **150%** | Easier reading, larger text |

**How to change:**
1. Find the **Text size** slider
2. Drag the slider left to decrease or right to increase
3. The percentage displays next to the slider (e.g., "120%")
4. Text updates immediately as you adjust

> **Tip:** You can also use your browser's built-in zoom (`Ctrl +` / `Ctrl -` on Windows, `Cmd +` / `Cmd -` on Mac) for additional size adjustments.

## Keyboard Navigation

The Chat User Portal supports full keyboard navigation:

### General Navigation

| Action | Keys |
|--------|------|
| Move between elements | `Tab` / `Shift + Tab` |
| Activate buttons | `Enter` or `Space` |
| Close dialogs/popups | `Escape` |
| Submit messages | `Enter` |
| Add new line in message | `Shift + Enter` |

### Sidebar Shortcuts

| Action | Keys |
|--------|------|
| Navigate to chat | `Tab` to chat, then `Enter` |
| Delete a chat | Select chat, then `Delete` or `Backspace` |
| Close sidebar | `Tab` to toggle button, then `Enter` |

### Message Area

| Action | Keys |
|--------|------|
| Navigate between messages | `Tab` through message elements |
| Copy message | `Tab` to Copy button, then `Enter` |
| Rate a response | `Tab` to Rate button, then `Enter` |

## Screen Reader Support

The portal includes accessibility features for screen readers:

- **ARIA labels**: Interactive elements have descriptive labels
- **Role attributes**: Elements are properly identified (buttons, dialogs, lists)
- **Live regions**: Status changes and notifications are announced
- **Focus management**: Focus moves logically through the interface

### Recommended Screen Readers

| Platform | Recommended |
|----------|-------------|
| Windows | NVDA, JAWS |
| Mac | VoiceOver |
| Linux | Orca |

## Visual Accessibility Tips

### Using Browser Features

Your browser offers additional accessibility options:

| Feature | How to Access |
|---------|---------------|
| **Zoom** | `Ctrl +` / `Ctrl -` (Windows) or `Cmd +` / `Cmd -` (Mac) |
| **High contrast mode** | Enable in Windows Settings > Accessibility |
| **Dark mode** | Some browsers apply system dark mode automatically |
| **Custom fonts** | Configure in browser accessibility settings |

### Display Recommendations

For the best experience:
- Use a screen resolution of at least 1280×720
- Ensure sufficient contrast between text and background
- Position your monitor to reduce glare
- Take regular breaks when reading long conversations

## Mobile Accessibility

On mobile devices:
- Use your device's built-in accessibility features (VoiceOver on iOS, TalkBack on Android)
- Enable larger text in your device's Display settings
- The interface automatically adjusts for smaller screens
- Touch targets are sized for easy tapping

## Troubleshooting

### Text Size Not Changing

1. Try refreshing the page (`F5` or `Ctrl + R`)
2. Check that JavaScript is enabled in your browser
3. Clear your browser cache and try again

### Screen Reader Not Reading Elements

1. Ensure your screen reader is in "forms mode" or "focus mode" when interacting with inputs
2. Try a different browser — Chrome and Firefox have the best screen reader support
3. Update your screen reader to the latest version

### Keyboard Focus Not Visible

1. Check your browser's accessibility settings for "Show focus indicators"
2. Some browser extensions can interfere with focus styles
3. Try disabling browser extensions temporarily

### Settings Not Saving

Accessibility settings are saved in your browser's session storage. They persist as long as:
- You stay logged in
- You don't clear browser data
- You use the same browser

If settings reset unexpectedly:
1. Check that cookies and site data aren't being blocked
2. Avoid using "private" or "incognito" mode for persistent settings

## WCAG 2.1 Feature Details

### Perceivable Content

**Text Alternatives (WCAG 1.1)**
- All images have alt text descriptions
- Icons are accompanied by text labels or ARIA labels
- Non-text content is described for screen readers

**Adaptable Content (WCAG 1.3)**
- Content is structured with proper headings
- Form elements have associated labels
- Reading order is logical and consistent

**Distinguishable Content (WCAG 1.4)**
- Text can be resized up to 200% without loss of functionality
- Color contrast meets 4.5:1 ratio for normal text
- Color is not the only means of conveying information
- Content reflows for different screen sizes

### Operable Interface

**Keyboard Accessible (WCAG 2.1)**
- All functionality available via keyboard
- No keyboard traps
- Skip navigation option for repetitive content

**Enough Time (WCAG 2.2)**
- Session timeouts provide warning and extension options
- Auto-hide notifications can be disabled
- No time-limited interactions required

**Seizure Prevention (WCAG 2.3)**
- No flashing content above 3 flashes per second
- Animations respect reduced-motion preferences

**Navigable (WCAG 2.4)**
- Page titles are descriptive
- Focus order is logical
- Link purpose is clear from context
- Multiple ways to find content

### Understandable Content

**Readable (WCAG 3.1)**
- Page language is identified
- Technical terms are explained
- Plain language is used where possible

**Predictable (WCAG 3.2)**
- Navigation is consistent across pages
- Components behave predictably
- Changes don't occur unexpectedly

**Input Assistance (WCAG 3.3)**
- Errors are clearly identified
- Labels and instructions are provided
- Error suggestions help users correct mistakes

### Robust Technology

**Compatible (WCAG 4.1)**
- Valid HTML markup
- ARIA properly implemented
- Status messages announced to screen readers
- Compatible with current assistive technologies

## Color Contrast

The User Portal maintains accessible color contrast ratios:

| Element | Contrast Ratio | WCAG Requirement |
|---------|----------------|------------------|
| Body text | 4.5:1 or higher | Level AA |
| Large text | 3:1 or higher | Level AA |
| UI components | 3:1 or higher | Level AA |
| Focus indicators | 3:1 or higher | Level AA |

### Checking Contrast

If you have difficulty distinguishing colors:

1. Enable your operating system's high contrast mode
2. Use browser extensions like "High Contrast" for Chrome
3. Adjust display settings in your system preferences

## Reduced Motion

For users sensitive to motion:

### System Preferences

The portal respects your operating system's reduced motion settings:

- **Windows**: Settings > Accessibility > Visual effects > Show animations
- **macOS**: System Preferences > Accessibility > Display > Reduce motion
- **iOS**: Settings > Accessibility > Motion > Reduce Motion
- **Android**: Settings > Accessibility > Remove animations

### What Changes with Reduced Motion

| Normal Behavior | With Reduced Motion |
|-----------------|---------------------|
| Animated transitions | Instant transitions |
| Sliding panels | Instant show/hide |
| Loading spinners | Static indicators |
| Smooth scrolling | Instant scrolling |

## Assistive Technology Compatibility

### Tested Screen Readers

| Screen Reader | Browser | Support Level |
|---------------|---------|---------------|
| NVDA | Firefox, Chrome | Full |
| JAWS | Chrome, Edge | Full |
| VoiceOver | Safari, Chrome | Full |
| Narrator | Edge | Full |
| Orca | Firefox | Basic |

### Tips for Screen Reader Users

1. **Use Browse Mode** to read conversation content
2. **Use Forms Mode** when typing in the message input
3. **Listen for ARIA live regions** announcing new messages
4. **Use heading navigation** (H key) to jump between sections

### Voice Control

For voice control users (Dragon NaturallySpeaking, Voice Control):

- Buttons have visible text labels
- Form fields have accessible names
- Links are descriptively named
- "Click [element text]" commands work reliably

## Cognitive Accessibility

### Simple Language

- Error messages explain what went wrong and how to fix it
- Instructions use clear, concise language
- Complex terms include explanations

### Consistent Design

- Navigation stays in the same location
- Similar functions use similar designs
- Buttons and controls are predictably placed

### Memory Support

- Conversation history is preserved
- Settings persist across sessions
- Important information isn't hidden

## Reporting Accessibility Issues

If you encounter accessibility barriers:

1. Note the specific issue and how it affects you
2. Include the browser and assistive technology you're using
3. Contact your organization's IT support
4. Request that they report the issue to FoundationaLLM

## Related Topics

- [Managing Conversations](managing-conversations.md) — Navigate and organize your chats
- [Printing Conversations](printing-conversations.md) — Create accessible printed copies
- [Viewing Status Messages](viewing-status-messages.md) — Accessible status notifications