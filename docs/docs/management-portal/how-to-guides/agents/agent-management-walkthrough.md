# Agent Management UX Walkthrough

A complete walkthrough of agent management tasks in the Management Portal.

## Overview

This guide walks you through the full user experience of managing agents in the Management Portal, from navigation to common tasks.

## Accessing Agent Management

### Step 1: Navigate to the Management Portal

1. Open your browser and navigate to the Management Portal URL
2. Sign in with your Microsoft Entra ID credentials
3. The portal dashboard loads

### Step 2: Find the Agents Section

1. Look at the left sidebar navigation
2. Find the **Agents** section
3. You'll see several options:
   - **Create New Agent** — Create a new agent
   - **All Agents** — View all agents you have access to
   - **My Agents** — View only agents you own

## Viewing Agents

### The All Agents Page

1. Click **All Agents** in the sidebar
2. The agent list table loads with these columns:

| Column | Description |
|--------|-------------|
| **Name** | Agent name (with display name if different) |
| **Description** | Brief description of the agent's purpose |
| **Expiration Date** | When the agent expires (if set) |
| **Edit** | ⚙️ Settings icon to modify |
| **Delete** | 🗑️ Trash icon to remove |
| **Set Default** | ⭐ Star icon to make default |

### Identifying Default Agents

- The current default agent displays a **Default** chip badge next to its name
- Default agents have a filled star icon

### Searching for Agents

1. Find the **Search** box above the table
2. Type the agent name or description keywords
3. The list filters in real-time as you type
4. Clear the search to show all agents again

### Sorting the List

1. Click any column header to sort by that column
2. Click again to reverse the sort order
3. An arrow indicator shows the current sort direction

### Pagination

1. Use the **rows per page** dropdown to show 10, 25, 50, or 100 agents
2. Navigate between pages using the pagination controls at the bottom
3. The total count of agents is displayed

## Common Agent Management Tasks

### Task: Find a Specific Agent

**Scenario:** You need to find an agent named "Sales Assistant"

1. Go to **All Agents**
2. Type "Sales" in the search box
3. Review the filtered results
4. Click on the agent row or Edit icon to view details

### Task: Edit Agent Configuration

**Scenario:** You need to change an agent's welcome message

1. Find the agent in the list
2. Click the **Settings** icon (⚙️) in the Edit column
3. The agent edit form opens
4. Scroll to find the **Welcome Message** field
5. Edit the content (rich text editor available)
6. Click **Save Changes**
7. A success message confirms the update

### Task: Delete an Agent

**Scenario:** An agent is no longer needed and should be removed

1. Find the agent in the list
2. Click the **Trash** icon (🗑️) in the Delete column
3. A confirmation dialog appears: "Are you sure you want to delete the agent '[name]'?"
4. Click **Yes** to confirm or **Cancel** to abort
5. The agent is removed from the list

> **Warning:** Deletion is permanent. Users will no longer be able to access the deleted agent.

### Task: Set an Agent as Default

**Scenario:** You want a specific agent to be the default for all users

**Prerequisites:** You need the User Access Administrator role

1. Find the agent you want as default
2. Click the **Star** icon (⭐) in the Set Default column
3. Confirm by clicking **Yes** in the dialog
4. The **Default** badge moves to this agent
5. The previous default (if any) is cleared

### Task: View Only Your Agents

**Scenario:** You want to focus on agents you own

1. Click **My Agents** in the sidebar instead of All Agents
2. The list shows only agents where you have the Owner role
3. All the same management actions are available

## Access Control for Agents

### Viewing Agent Permissions

1. Open an agent for editing
2. Click **Access Control** at the top right of the page
3. The role assignments panel opens showing:
   - Current role assignments
   - Assigned principals (users/groups)
   - Role types (Owner, Contributor, Reader)

### Adding Role Assignments

1. In Access Control, click **Add Role Assignment**
2. Select the scope (agent or prompt)
3. Choose the principal type (User, Group, Service Principal)
4. Enter the principal ID or search for the user
5. Select the role (Owner, Contributor, Reader)
6. Click **Save**

### Removing Role Assignments

1. In Access Control, find the assignment to remove
2. Click the **Delete** button next to it
3. Confirm the removal
4. The access is revoked immediately

## Filtering and Organization

### Best Practices for Finding Agents

| Goal | Approach |
|------|----------|
| Find by name | Use search with partial name |
| Find by purpose | Search using description keywords |
| Find recent agents | Sort by creation date (if visible) |
| Find your agents | Use My Agents view |
| Find expiring agents | Sort by Expiration Date |

### Organizing Your View

1. **Use search** to quickly find specific agents
2. **Sort by name** for alphabetical navigation
3. **Increase rows per page** to see more at once
4. **Use My Agents** when focusing on your own work

## Refreshing Data

### Manual Refresh

1. Click the **Refresh** button (🔄) at the top right of the table
2. The list reloads with the latest data
3. Useful after making changes in another tab

### Automatic Refresh

- The agent list doesn't auto-refresh
- Make changes in one session and refresh others to see updates

## Troubleshooting Common Issues

### Agent Not Appearing in List

- Wait a moment for data to load
- Click Refresh to reload the list
- Verify you have permission to view the agent
- Check if you're in All Agents vs My Agents

### Can't Edit an Agent

- The Edit icon appears grayed out
- You may only have Reader permission
- Contact the agent Owner for edit access

### Can't Delete an Agent

- The Delete icon appears grayed out
- You need Owner or Contributor permission
- Contact an administrator for help

### Can't Set Default Agent

- You need User Access Administrator role
- Contact your administrator to request this permission
- Or ask an administrator to set the default for you

### Changes Not Saving

- Check for validation errors (red text below fields)
- Ensure required fields are filled
- Verify your session hasn't timed out
- Try refreshing and re-entering changes

## Related Topics

- [Create New Agent](create-new-agent.md) — Full guide to agent creation
- [Agent Creation Walkthrough](agent-creation-walkthrough.md) — Step-by-step creation UX
- [All Agents](all-agents.md) — Reference for the All Agents page
- [My Agents](my-agents.md) — Reference for the My Agents page
