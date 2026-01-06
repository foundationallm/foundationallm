# Sharing Agents with Others

Learn how to share custom agents with colleagues and manage agent access permissions.

## Overview

When you create a custom agent, you can share it with others in your organization. The sharing model defines who can access your agent and what they can do with it.

## Sharing Model Roles

FoundationaLLM uses a three-tier sharing model for agents:

| Role | Can Use | Can Edit | Can Share | Can Delete |
|------|---------|----------|-----------|------------|
| **Owner** | ✅ | ✅ | ✅ | ✅ |
| **Collaborator** | ✅ | ✅ | ✅ | ❌ |
| **User** | ✅ | ❌ | ❌ | ❌ |

### Owner

Owners have full control over the agent:

- **Use the agent** in conversations
- **Edit** all agent settings and configuration
- **Share** the agent with others (assign any role)
- **Delete** the agent permanently
- **Transfer ownership** to another user

When you create an agent, you automatically become its Owner.

### Collaborator

Collaborators can help maintain and improve the agent:

- **Use the agent** in conversations
- **Edit** agent settings (prompts, tools, configuration)
- **Share** the agent with others (assign User or Collaborator role)
- Cannot delete the agent
- Cannot remove the Owner's access

Collaborators are ideal for team members who help maintain the agent but shouldn't have full control.

### User

Users can interact with the agent but cannot modify it:

- **Use the agent** in conversations
- Cannot edit any settings
- Cannot share with others
- Cannot delete the agent

Users are ideal for end users who simply need to use the agent.

## Sharing an Agent

### Accessing Share Settings

1. Open the agent for editing (if you have Owner or Collaborator access)
2. Look for the **Share** or **Access Control** button
3. The sharing panel opens

### Adding Users

1. In the sharing panel, click **Add User** or **Add Role Assignment**
2. Search for the user by name or email
3. Select the user from the results
4. Choose a role: **Owner**, **Collaborator**, or **User**
5. Click **Add** or **Save**

### Removing Users

1. Open the sharing panel
2. Find the user in the list
3. Click the **Remove** button next to their name
4. Confirm the removal

> **Note:** You cannot remove the last Owner. Transfer ownership first if needed.

### Changing Roles

1. Open the sharing panel
2. Find the user whose role you want to change
3. Click on their current role
4. Select the new role from the dropdown
5. Save changes

## Sharing Best Practices

### Team Collaboration

| Scenario | Recommended Approach |
|----------|----------------------|
| Small team maintaining an agent | Make all team members Collaborators |
| Department-wide agent | Owners maintain, department members are Users |
| Cross-team project | Project leads as Collaborators, team members as Users |

### Access Principles

- **Least Privilege**: Grant the minimum role needed
- **Multiple Owners**: Consider having 2-3 Owners for important agents
- **Regular Review**: Periodically audit who has access
- **Clear Ownership**: Ensure someone is responsible for each agent

## Who Can See Shared Agents

### Visibility Rules

Shared agents appear in the agent selector for users with access:

1. Users see agents where they have any role (Owner, Collaborator, or User)
2. Agents appear in the dropdown alongside other available agents
3. Users can enable/disable shared agents in Settings

### Organization Policies

Your administrator may configure:

- Whether self-service agents are visible to others by default
- Required approval for sharing agents widely
- Limits on the number of users an agent can be shared with

## Managing Received Shares

### Viewing Your Shared Agents

1. Open **Settings** > **Agents** tab
2. View the list of available agents
3. Shared agents show your role (Owner, Collaborator, or User)

### Removing Yourself from a Shared Agent

1. Open **Settings** > **Agents** tab
2. Find the shared agent
3. Disable the agent using the checkbox

> **Note:** This hides the agent from your view but doesn't remove your access. The Owner can still see you have access.

## Ownership Transfer

### When to Transfer Ownership

- Original creator leaving the organization
- Changing team responsibilities
- Consolidating agent management

### How to Transfer Ownership

1. Open the agent sharing settings (as current Owner)
2. Add the new Owner with the **Owner** role
3. The new user now has full control
4. Optionally, change your role to Collaborator or remove yourself

## Notifications

> **TODO**: Document notification behavior for sharing events (when users are notified of being added to/removed from agents).

## Troubleshooting

### Can't Share Agent

- Verify you have Owner or Collaborator role
- Check that sharing features are enabled for your organization
- Ensure the user exists in your organization's directory

### User Can't See Shared Agent

- Verify the user was added correctly
- Check if the agent is Active (not expired)
- The user may need to enable the agent in their Settings

### Can't Remove User

- You may not have permission (check your role)
- Cannot remove the last Owner
- System accounts may have protected access

### Can't Change Roles

- Only Owners can assign Owner role
- Collaborators can only assign User or Collaborator roles
- Some role changes may require approval

## Related Topics

- [Creating and Editing Custom Agents](creating-editing-agents.md) — Create your own agents
- [Managing Available Agents](managing-available-agents.md) — Control which agents you see
- [Selecting an Agent](selecting-agent.md) — Choose an agent for conversations
