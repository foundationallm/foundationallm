# Managing Available Agents

Control which agents appear in your agent selector and learn how to access agent settings.

## Understanding the Agent Catalog

The **agent catalog** is the collection of all agents available to you. Think of it as your personal library of AI assistants. Each user's catalog may be different based on their permissions and organization settings.

### What Determines Your Agent Catalog

The agents you see in the Chat User Portal depend on several factors:

1. **Organization publishing**: Administrators publish agents for general use
2. **Access permissions**: Your role determines which agents you can access
3. **Shared agents**: Colleagues may share custom agents with you
4. **Your own agents**: Agents you create (if self-service is enabled)
5. **Your preferences**: You can enable or disable agents you have access to
6. **Featured agents**: Some agents are highlighted as featured and cannot be disabled

### How Agents Appear in the Dropdown

When you click the agent selector in the navigation bar:

1. **Featured agents** appear first, grouped at the top
2. **Other enabled agents** appear below in alphabetical order
3. Only agents you've enabled in Settings appear in the dropdown
4. Your most recently used agents may be prioritized

## User Portal vs. Management Portal

Agent management happens in different places depending on your role:

| Task | User Portal | Management Portal |
|------|-------------|-------------------|
| Select an agent to use | ✅ | ❌ |
| Enable/disable agents for yourself | ✅ | ❌ |
| Create self-service agents | ✅ (if enabled) | ✅ |
| Edit your custom agents | ✅ (if enabled) | ✅ |
| Configure enterprise agents | ❌ | ✅ |
| Manage all organization agents | ❌ | ✅ |
| Set up data sources and pipelines | ❌ | ✅ |
| Configure agent access control | ❌ | ✅ |

**User Portal** is designed for end users who want to:
- Use agents in conversations
- Manage which agents appear in their personal agent selector
- Create and edit their own custom agents (if self-service is enabled)

**Management Portal** is designed for administrators who need to:
- Create and configure enterprise-wide agents
- Manage agent permissions and access control
- Set up knowledge sources and data pipelines
- Configure platform-wide settings

## Accessing Agent Settings

1. Click the **Settings** button (gear icon ⚙️) in the bottom-left corner of the sidebar
2. The **Agents** tab opens by default
3. View your available agents in the table

## The Agents Table

The agent settings table shows:

| Column | Description |
|--------|-------------|
| **Name** | The display name of the agent |
| **Enabled** | Checkbox showing if the agent appears in your selector |
| **Edit** | Edit button (if you have permission to modify the agent) |

## Enabling and Disabling Agents

### To Enable an Agent

1. Find the agent in the table
2. Click the checkbox in the **Enabled** column
3. A checkmark (✓) appears and the agent is now available in your agent selector
4. A confirmation toast appears: "Agent is now enabled"

### To Disable an Agent

1. Find the agent in the table
2. Click the checked checkbox in the **Enabled** column
3. The checkmark disappears and the agent is removed from your selector
4. A confirmation toast appears: "Agent is now disabled"

### Agents You Cannot Disable

Some agents cannot be disabled:

- **Currently active agent**: You cannot disable the agent you're currently using in a conversation
- **Pinned featured agents**: Administrators may require certain agents to always be available

These agents show a grayed-out checkbox that cannot be changed.

## Finding Agents

### Using Search

1. Find the **Search agents by name** field above the table
2. Type part of the agent's name
3. The list filters to show matching agents
4. Clear the search to show all agents again

### Filtering by Status

1. Check the **Show enabled agents only** checkbox
2. The table shows only agents you've enabled
3. Uncheck to show all available agents

## Understanding Agent Status

| Visual Indicator | Meaning |
|-----------------|---------|
| ✓ Checkmark (blue) | Agent is enabled and available |
| Empty checkbox | Agent is disabled and hidden from selector |
| Grayed checkbox | Agent cannot be disabled (featured or in use) |
| Edit icon (blue) | You can edit this agent |
| Edit icon (gray) | You can view but not edit this agent |

## Requesting Access to More Agents

If you need access to agents not in your list:

1. Look for the **Request permission to manage agents** link at the bottom of the Settings dialog
2. Click the link to open your organization's access request process
3. Follow your organization's procedure to request additional agent access

> **Note:** This link only appears if your administrator has configured an access request URL.

## Editing Agents (If Permitted)

If you have agent contributor permissions:

1. Find the agent in the table
2. Click the **Edit** button (pencil icon)
3. You'll be taken to the agent editing page
4. Make your changes
5. Save when finished

> **Note:** The Edit button appears only if the Agent Self-Service feature is enabled for your organization.

## Managing Agents Page

For more advanced agent management:

1. Open the Settings dialog
2. Look for the **Manage Agents** link at the bottom
3. Click to open the full agent management page

This page provides:
- Detailed agent information
- Agent creation (if permitted)
- Full editing capabilities
- File management for agent knowledge bases

## Tips for Organizing Your Agents

### Keep Your List Focused

- Enable only the agents you use regularly
- Disable agents for projects you've completed
- This makes finding the right agent faster

### Use Descriptive Names When Searching

- Search by keywords that describe what the agent does
- Agent names often include their purpose (e.g., "Sales Report Generator")

### Check for New Agents Regularly

- Administrators may add new agents over time
- Periodically review disabled agents to see if new ones are available

## Troubleshooting

### Agent List is Empty

- Wait a moment — agents are still loading
- Refresh the page if the loading spinner persists
- Check with your administrator if no agents appear

### Cannot Enable/Disable an Agent

- The agent may be pinned by administrators
- You may be currently using the agent
- Switch to a different agent, then try again

### Changes Not Saving

- Ensure you have a stable internet connection
- Try refreshing the page and making the change again
- Check if you're seeing any error messages

### Edit Button is Grayed Out

- You may not have permission to edit this agent
- The agent may be read-only
- Contact your administrator for editing access

### Search Returns No Results

- Check your spelling
- Try a partial name or different keywords
- Clear the search to see all agents

## Related Topics

- [Selecting an Agent](selecting-agent.md) — Choose an agent for your conversation
- [Managing Conversations](managing-conversations.md) — Work with your chat sessions
