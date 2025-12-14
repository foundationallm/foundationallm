# Copying Messages from Conversations

Copy agent responses and your own messages to use in documents, emails, or other applications.

## Copying a Message

### Copying Your Own Messages

Your messages appear on the right side of the conversation with a colored background (typically matching your organization's theme color).

To copy a message you sent:

1. Find the message you want to copy
2. Look in the **upper-right corner** of your message, next to the timestamp
3. Click the **copy icon** (it looks like two overlapping rectangles 📋)
4. A green confirmation message appears briefly at the top of the screen: *"Message copied to clipboard with formatting!"*

> **Tip:** The copy icon may only appear when you hover over the message area with your mouse.

### Copying Agent Responses

Agent responses appear on the left side of the conversation with a light gray background. They typically have the agent's name and icon at the top.

To copy an agent's response:

1. Find the agent response you want to copy
2. Look at the **bottom of the message** where you'll see action buttons
3. Click the **Copy** button (it shows a copy icon and the word "Copy")
4. A green confirmation message appears briefly: *"Message copied to clipboard with formatting!"*

## Pasting Copied Content

The copied content automatically adapts to where you paste it:

### In Word Processors and Email (Rich Text)

When you paste into applications that support formatting (Microsoft Word, Google Docs, Outlook, Gmail):

- **Bold text** and *italics* are preserved
- Headings maintain their size and weight
- Bullet points and numbered lists appear formatted
- Tables are rendered as actual tables
- Code appears in a styled box with a monospace font

### In Plain Text Editors

When you paste into plain text applications (Notepad, code editors, some chat apps):

- Content appears as raw Markdown text
- Formatting markers like `**bold**` and `- bullet` are visible
- Code blocks show with their original ``` markers

## Copying Code Blocks

Agent responses often include code examples in special formatted blocks. Each code block has its own dedicated **Copy** button for convenience.

To copy just the code from a code block:

1. Find the code block in the agent's response—it appears with a dark background and colored syntax highlighting
2. Look at the **top of the code block** where you'll see:
   - The programming language name on the left (e.g., "python", "javascript", "sql")
   - A **Copy** button on the right
3. Click the **Copy** button
4. A confirmation message appears: *"Code copied to clipboard with formatting!"*

### Where Code Gets Pasted

| Destination | What Happens |
|-------------|--------------|
| Code editor (VS Code, etc.) | Clean code ready to run—no extra formatting |
| Microsoft Word or Google Docs | Code in a styled gray box with monospace font |
| Email | Code in a formatted box, easy to read |
| Notepad | Plain code text |

## Tips for Best Results

### Choose the Right Copy Button

- **Use the message Copy button** when you want the full response including explanations, lists, and context
- **Use the code block Copy button** when you only need the code itself

### Pasting Options

| Destination | What You Get |
|-------------|--------------|
| Microsoft Word | Fully formatted text with styles |
| Google Docs | Fully formatted text with styles |
| Email (Outlook, Gmail) | Formatted text preserving headers and lists |
| Notepad / Plain text | Raw Markdown source |
| VS Code / Code editors | Raw text or Markdown |
| Slack / Teams | May vary—try both regular paste and paste as plain text |

### Keyboard Shortcuts

After copying, use these shortcuts to paste:

| Action | Windows | Mac |
|--------|---------|-----|
| Standard paste | `Ctrl + V` | `Cmd + V` |
| Paste as plain text | `Ctrl + Shift + V` | `Cmd + Shift + V` |

> **Note:** "Paste as plain text" is useful when the formatted version doesn't look right in your destination app.

## Troubleshooting

### Nothing Happens When I Click Copy

If clicking Copy doesn't show a confirmation message:

1. **Check browser permissions**: Your browser may have blocked clipboard access
   - Look for a clipboard or permission icon in your browser's address bar
   - Click it and select "Allow" for clipboard access
2. **Refresh the page**: Press `F5` or click the refresh button, then try again
3. **Try a different browser**: Chrome and Edge have the best clipboard support. Firefox works well too. Safari has limited support for advanced copy features.

### No Confirmation Message Appears

The confirmation message appears briefly at the top of the screen. If you miss it:
- The copy likely still worked—try pasting (`Ctrl+V` / `Cmd+V`) to verify
- Make sure pop-ups or notifications aren't blocked for this site

### Formatting Looks Wrong After Pasting

**In email:**
- Check that you're composing in "Rich Text" or "HTML" mode, not "Plain Text" mode
- In Outlook: Look for the "Format Text" tab and select "HTML"

**In Word or Docs:**
- Use "Paste Special" (`Ctrl+Shift+V` in Word) and choose "Keep Source Formatting"
- Or paste normally and use "Paste Options" button that appears to adjust

**For code:**
- If code styling doesn't appear correctly, use "Paste as plain text" instead
- In Word, insert a text box or use a code-formatting add-in for best results

### Large Responses Are Slow to Copy

Very long agent responses may take a moment to process before the confirmation appears. Wait a second or two for the confirmation message before pasting.

## What Gets Copied

When you copy a message, the system copies:

✅ **Included:**
- All text content
- Formatting (bold, italic, headers, lists)
- Tables
- Code blocks with syntax highlighting
- Links

❌ **Not included:**
- Attached files (you'll need to download these separately)
- Images generated by the agent (right-click to save these)
- Interactive elements

## Related Topics

- [Printing Conversations](printing-conversations.md) — Create printable versions of your chats
- [Managing Conversations](managing-conversations.md) — Organize and find your past conversations
- [Downloading Files](downloading-files.md) — Save files generated by agents
