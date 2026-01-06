# Managing Your Agent Catalog

The User Portal allows you to customize which agents appear in your agent dropdown, giving you control over your workspace and enabling you to focus on the agents you use most frequently.

## Overview

Each user in FoundationaLLM can personalize their agent catalog:

- **Select visible agents**: Choose which agents appear in your dropdown
- **Feature important agents**: Featured agents appear prominently at the top
- **Filter and search**: Quickly find agents by name
- **Persist preferences**: Your selections are saved across sessions

## Accessing Agent Settings

1. Click on the **Settings** icon in the User Portal header
2. Navigate to the **Agents** tab
3. View the list of all agents available to you

<!-- [TODO: Add screenshot of Settings dialog with Agents tab] -->

## Managing Visible Agents

### Viewing Available Agents

The agent list shows all agents you have permission to access. Each agent displays:

- Agent name and description
- Agent status (active/inactive)
- Whether it's a featured agent

### Enabling/Disabling Agents

To show or hide an agent in your dropdown:

1. Locate the agent in the list
2. Toggle the **Enabled** switch
3. Click **Save** to apply changes

Disabled agents will not appear in your agent dropdown but remain available to re-enable at any time.

### Filtering Agents

Use the filter controls to narrow down the agent list:

- **Search by name**: Type in the search box to filter agents
- **Show enabled only**: Toggle to see only agents currently in your catalog
- **Show featured only**: Toggle to see only featured agents

## Featured Agents

Featured agents are highlighted by administrators to help users discover new or important agents. They appear in a dedicated "Featured Agents" section at the top of the agent dropdown.

> [!NOTE]
> Featured agent designation is configured by administrators via the `FoundationaLLM:UserPortal:Configuration:FeaturedAgentNames` setting.

## Setting a Default Agent

To set a default agent that loads automatically when you open the User Portal:

1. Open **Settings** > **Agents**
2. Select the agent you want as default
3. Click **Set as Default**
4. Click **Save**

When no default is set, the User Portal displays the default agent welcome message configured in branding settings.

## How Agents Appear in the Dropdown

The agent dropdown is organized as follows:

1. **Featured Agents** (if any) - Highlighted section at the top
2. **Your Agents** (if self-service enabled) - Agents you've created
3. **Available Agents** - All other enabled agents, sorted alphabetically

Each agent entry shows:
- Agent display name (or name if no display name configured)
- Brief description
- Status indicator

## Agent Visibility and Permissions

Your visible agents are determined by:

1. **Role-based access control (RBAC)**: You can only see agents you have Reader permission on
2. **Your personal settings**: You can hide agents you have access to but don't use
3. **Agent status**: Expired agents may not appear even if you have access

For more information on RBAC, see [Role-Based Access Control](../../role-based-access-control/index.md).

## Troubleshooting

### Agent Not Appearing

If an agent you expect to see is missing:

1. Check that you have Reader permission on the agent
2. Verify the agent is not expired
3. Check your agent settings to ensure it's not disabled
4. Contact your administrator if issues persist

### Changes Not Persisting

If your agent selections reset:

1. Ensure you click **Save** after making changes
2. Clear your browser cache and try again
3. Check for any error messages in the Settings dialog

<!-- [TODO: Add screenshot of agent dropdown with sections labeled] -->

## Related Topics

- [Self-Service Agent Creation](self-service-agent-creation.md)
- [Agent Sharing Model](agent-sharing-model.md)
- [Agent Management Walkthrough](walkthroughs/agent-management-walkthrough.md)
