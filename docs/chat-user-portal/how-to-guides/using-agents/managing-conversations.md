# Managing Conversations

Create, organize, rename, and delete your conversations in the Chat User Portal.

## Understanding Conversations

A conversation (also called a chat or session) is a continuous dialog with an AI agent. Each conversation:

- Has its own message history
- Maintains context from previous messages in that conversation
- Can use a different agent than other conversations
- Is saved automatically so you can return to it later

## The Sidebar

The sidebar on the left side of the screen shows all your saved conversations:

- **Chats** header with a **+** button to create new conversations
- List of your conversations, newest first
- Each conversation shows its name (which you can customize)
- The currently selected conversation is highlighted

## Creating a New Conversation

### Using the + Button

1. Click the **+** button next to the "Chats" header in the sidebar
2. A new conversation is created immediately
3. The conversation becomes active and ready for your first message

### Automatic Creation

A new conversation is also created when:
- You switch agents during an active conversation
- Your current conversation is the first message with a new agent

> **Tip:** If you accidentally create a new conversation when you meant to continue an existing one, click the existing conversation in the sidebar to return to it.

## Selecting a Conversation

1. Look at the conversation list in the sidebar
2. Click on any conversation to open it
3. The selected conversation is highlighted with a colored background and left border
4. Your messages and the agent's responses appear in the main area

## Renaming a Conversation

By default, conversations are named with a timestamp (e.g., "Chat 12/14/2024, 10:30 AM"). You can give them more descriptive names.

### To Rename

1. Find the conversation in the sidebar
2. Hover over the conversation to reveal the action icons
3. Click the **Settings** button (gear icon ⚙️)
4. In the dialog that appears, edit the **Name** field
5. Click **Update** to save

### Naming Tips

- Use descriptive names like "Q4 Sales Analysis" or "Code Review - Login Feature"
- Include the project or topic for easy identification
- Add dates if you have ongoing conversations about the same topic

## Adding Metadata to Conversations

You can attach metadata (additional information) to conversations for organizational purposes.

1. Click the **Settings** button (gear icon) on a conversation
2. Find the **Metadata** field
3. Enter valid JSON data, such as:

```json
{
  "project": "Marketing Campaign",
  "priority": "high",
  "deadline": "2024-01-15"
}
```

4. Click **Update** to save

> **Note:** Metadata must be valid JSON format. If you enter invalid JSON, you'll see an error message.

## Deleting a Conversation

### Using the Delete Button

1. Find the conversation in the sidebar
2. Hover over the conversation to reveal the action icons
3. Click the **Delete** button (trash icon 🗑️)
4. A confirmation dialog appears: "Do you want to delete the chat '[conversation name]'?"
5. Click **Yes** to confirm deletion

### Using Keyboard

1. Navigate to the conversation using Tab key
2. Press `Delete` or `Backspace`
3. Confirm the deletion in the dialog

> **⚠️ Warning:** Deleted conversations cannot be recovered. Make sure you no longer need the information before deleting.

## Switching Agents Within Conversations

When you change agents:

1. Click the agent selector dropdown in the top navigation
2. Choose a different agent
3. A new conversation starts automatically
4. Your previous conversation with the other agent remains in the sidebar

This behavior ensures each conversation maintains consistent context with a single agent.

## Conversation Features

### Context Retention

Agents remember previous messages in your conversation:
- Recent messages are included when generating responses
- This allows for follow-up questions and clarifications
- The number of messages retained depends on agent configuration

### Automatic Saving

- Conversations save automatically as you chat
- You can close the browser and return later
- No "save" button is needed

### Session Persistence

- Conversations are tied to your user account
- Access your conversations from any device by logging in
- Conversations persist until you delete them

## Managing Many Conversations

If you have many conversations:

### Quick Navigation

- Scroll through the sidebar to find conversations
- Click any conversation to open it immediately
- The current conversation remains highlighted

### Cleanup Strategy

- Regularly delete conversations you no longer need
- Rename important conversations for easy identification
- Consider the conversation's purpose before deleting

## Troubleshooting

### Conversation Won't Load

- Wait for the loading spinner to complete
- Refresh the page (`F5` or `Ctrl + R`)
- Check your internet connection

### Messages Not Appearing

- Scroll down to see newer messages
- Wait for the conversation to fully load
- The conversation may be empty if you haven't sent any messages yet

### Cannot Delete Conversation

- Make sure you've confirmed the deletion dialog
- Check if you have a stable internet connection
- Try refreshing the page and attempting again

### Lost a Conversation

- Conversations cannot be recovered once deleted
- Check if you're logged into the correct account
- The conversation may have been deleted from another device

### New Conversation Created Unexpectedly

This happens when:
- You switched agents during an active conversation
- The current agent was changed
- You clicked the + button

Your previous conversation is still available in the sidebar.

### Rename Not Saving

- Make sure metadata (if provided) is valid JSON
- Check your internet connection
- Click the **Update** button (not just close the dialog)

## Keyboard Shortcuts

| Action | Keys |
|--------|------|
| Navigate sidebar | `Tab` through items |
| Select conversation | `Enter` when focused |
| Delete conversation | `Delete` or `Backspace` when focused |
| Cancel dialog | `Escape` |
| Confirm action | `Enter` |

## Related Topics

- [Selecting an Agent](selecting-agent.md) — Choose the right agent for your task
- [Printing Conversations](printing-conversations.md) — Save conversations as PDF or print
- [Copying Messages](copying-prompts-results.md) — Copy content from conversations
