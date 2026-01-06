# All Agents

Learn how to view and manage all agents in the Management Portal.

## Overview

The **All Agents** page displays every agent configured in your FoundationaLLM instance that you have permission to view. This is the central location for managing agent configurations.

## Accessing All Agents

1. In the Management Portal sidebar, click **All Agents** under the **Agents** section
2. The agent list table loads, showing all available agents

## Agent List Table

The table displays the following columns:

| Column | Description |
|--------|-------------|
| **Name** | Agent name and display name (if set). Default agents show a star chip badge. |
| **Description** | Brief description of the agent's purpose |
| **Expiration Date** | Date when the agent becomes inactive (if set) |
| **Edit** | Settings icon to modify agent configuration |
| **Delete** | Trash icon to remove the agent |
| **Set Default** | Star icon to make this the default agent |

## Searching and Filtering

### Search
Use the search box at the top of the table to filter agents by:
- Agent name
- Description

Type your search term and the list filters in real-time.

### Sorting
Click any column header to sort by that column:
- Click once for ascending order
- Click again for descending order

### Pagination
Configure how many agents to display per page:
- Use the dropdown to select 10, 25, 50, or 100 rows
- Navigate between pages using the pagination controls

## Managing Agents

### Editing an Agent

1. Locate the agent in the list
2. Click the **Settings icon** (⚙️) in the Edit column
3. The agent edit page opens with all configuration sections
4. Make your changes
5. Click **Save Changes**

> **Note:** The Edit icon is disabled (grayed out) if you don't have write permission for the agent.

### Deleting an Agent

1. Locate the agent in the list
2. Click the **Trash icon** (🗑️) in the Delete column
3. A confirmation dialog appears: "Are you sure you want to delete the agent '[name]'?"
4. Click **Yes** to confirm or **Cancel** to abort

> **Warning:** Deleted agents are removed from the system. Users will no longer be able to interact with deleted agents.

> **Note:** The Delete icon is disabled if you don't have delete permission for the agent.

### Setting a Default Agent

The default agent is automatically selected for new conversations in the Chat User Portal.

1. Locate the agent you want to set as default
2. Click the **Star icon** (⭐) in the Set Default column
3. Confirm by clicking **Yes** in the dialog

When an agent is the default, it displays a **Default** chip next to its name.

> **Note:** Only users with the "User Access Administrator" role can set default agents.

## Refreshing the List

Click the **Refresh** button (🔄) at the top right of the table to reload the agent list and see the latest changes.

## Permissions

Your available actions depend on your role assignments:

| Action | Required Permission |
|--------|---------------------|
| View agents | `FoundationaLLM.Agent/agents/read` |
| Edit agents | `FoundationaLLM.Agent/agents/write` |
| Delete agents | `FoundationaLLM.Agent/agents/delete` |
| Set default agent | User Access Administrator role |

## Related Topics

- [My Agents](my-agents.md) - View only agents you own
- [Create New Agent](create-new-agent.md) - Create a new agent
- [Instance Access Control](../security/instance-access-control.md) - Manage permissions
