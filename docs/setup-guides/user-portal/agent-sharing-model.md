# Agent Sharing Model

FoundationaLLM provides a flexible sharing model for self-service agents that allows agent creators to collaborate with others while maintaining control over their agents.

## Overview

When you create a self-service agent, you become the **Owner** of that agent. As an owner, you can share your agent with other users at different permission levels.

## Permission Levels

### Owners

Owners have full control over the agent:

| Permission | Description |
|------------|-------------|
| **Use** | Interact with the agent in chat |
| **Edit** | Modify all agent properties |
| **Share** | Grant access to other users |
| **Delete** | Permanently remove the agent |
| **Transfer Ownership** | Assign another user as owner |

**Key Characteristics:**
- An agent must have at least one owner
- Multiple users can be owners
- Only owners can delete an agent or transfer ownership

### Collaborators

Collaborators can help develop and maintain the agent:

| Permission | Description |
|------------|-------------|
| **Use** | Interact with the agent in chat |
| **Edit** | Modify agent properties |
| **Share** | Grant User-level access to others |

**Key Characteristics:**
- Cannot delete the agent
- Cannot grant Owner or Collaborator access to others
- Can share the agent with Users

### Users

Users can only interact with the agent:

| Permission | Description |
|------------|-------------|
| **Use** | Interact with the agent in chat |

**Key Characteristics:**
- Cannot modify the agent
- Cannot share the agent
- Ideal for general consumption

## Permission Matrix

| Action | Owner | Collaborator | User |
|--------|-------|--------------|------|
| Chat with agent | ✅ | ✅ | ✅ |
| View agent settings | ✅ | ✅ | ❌ |
| Edit agent properties | ✅ | ✅ | ❌ |
| Add Users | ✅ | ✅ | ❌ |
| Add Collaborators | ✅ | ❌ | ❌ |
| Add Owners | ✅ | ❌ | ❌ |
| Remove Users | ✅ | ✅* | ❌ |
| Remove Collaborators | ✅ | ❌ | ❌ |
| Remove Owners | ✅ | ❌ | ❌ |
| Delete agent | ✅ | ❌ | ❌ |
| Transfer ownership | ✅ | ❌ | ❌ |

*Collaborators can only remove Users they granted access to.

## Sharing an Agent

### Adding Users

To share your agent with other users:

1. Open your agent settings in the User Portal
2. Navigate to the **Sharing** tab
3. Click **Add People**
4. Enter email addresses or search for users
5. Select the permission level (Owner, Collaborator, User)
6. Click **Share**

<!-- [TODO: Add screenshot of sharing dialog] -->

### Sharing with Groups

You can also share with Microsoft Entra ID security groups:

1. In the **Sharing** tab, click **Add People**
2. Switch to the **Groups** tab
3. Search for the group name
4. Select the permission level
5. Click **Share**

> [!NOTE]
> Group sharing applies the selected permission level to all group members.

### Managing Existing Access

To modify or remove existing access:

1. Open your agent's **Sharing** tab
2. Find the user or group in the list
3. Click the dropdown to change their permission level, or
4. Click **Remove** to revoke access

## Viewing Who Has Access

To see everyone who has access to your agent:

1. Open your agent settings
2. Navigate to the **Sharing** tab
3. View the list of users and groups with their permission levels

The list shows:
- User/group name
- Email address
- Permission level
- Date access was granted
- Who granted the access

## Best Practices

### Choosing Permission Levels

| Scenario | Recommended Level |
|----------|-------------------|
| Team members helping develop the agent | Collaborator |
| General users who just need to use it | User |
| Backup administrator | Owner |
| External partners | User (with caution) |

### Security Considerations

1. **Principle of least privilege**: Grant the minimum permissions necessary
2. **Regular audits**: Periodically review who has access
3. **Prompt review**: Be careful about sharing agents with sensitive prompts
4. **Remove inactive users**: Revoke access for users who no longer need it

### Collaboration Tips

1. Document your agent's purpose and behavior for collaborators
2. Establish naming conventions for shared agents
3. Use the welcome message to communicate update history
4. Coordinate changes with other collaborators to avoid conflicts

## Sharing and RBAC

The agent sharing model works alongside FoundationaLLM's role-based access control (RBAC):

- Agent sharing determines access to **specific agents**
- RBAC determines access to **platform features and resources**

A user must have appropriate RBAC permissions AND agent-level access to use a shared agent.

For more information on RBAC, see [Role-Based Access Control](../../role-based-access-control/index.md).

<!-- [TODO: Clarify how sharing interacts with existing RBAC role assignments] -->

## Troubleshooting

### User Can't See Shared Agent

If a user reports they can't see an agent you shared:

1. Verify the sharing was completed successfully
2. Ask the user to refresh their browser
3. Check if the agent is active (not expired)
4. Verify the user's RBAC permissions
5. Check if the user has disabled the agent in their personal settings

### Can't Share with Specific User

If you're unable to share with a specific user:

1. Verify you have Owner or Collaborator permissions
2. Check that the user exists in the directory
3. Verify there are no sharing limits configured

<!-- [TODO: Document sharing limits if any] -->

## Related Topics

- [Self-Service Agent Creation](self-service-agent-creation.md)
- [Managing Your Agent Catalog](agent-management.md)
- [Role-Based Access Control](../../role-based-access-control/index.md)
