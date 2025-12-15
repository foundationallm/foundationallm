# Understanding Scope

Scope defines where a role assignment applies, controlling which resources a principal can access.

## Scope Levels

| Level | Description | Example |
|-------|-------------|---------|
| **Instance** | Entire FoundationaLLM deployment | All agents, prompts, data sources |
| **Resource** | Specific resource | Single agent |

## Scope Hierarchy

```
/instances/{instanceId}                           <- Instance scope
    /providers/{providerName}
        /{resourceType}
            /{resourceName}                       <- Resource scope
                /{subResourceType}
                    /{subResourceName}            <- Sub-resource scope
```

## Scope Format

Scopes are resource identifiers following this pattern:

```
/instances/{instanceId}/providers/{providerName}/{resourceType}/{resourceName}
```

### Components

| Component | Description |
|-----------|-------------|
| `instanceId` | Unique GUID of your FoundationaLLM deployment |
| `providerName` | Resource provider (e.g., `FoundationaLLM.Agent`) |
| `resourceType` | Type of resource (e.g., `agents`) |
| `resourceName` | Name of specific resource |

## Scope Examples

### Instance Scope

```
/instances/11111111-1111-1111-1111-111111111111
```

Applies to all resources in the instance.

### Agent Scope

```
/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Agent/agents/sales-agent
```

Applies only to the `sales-agent` agent.

### Data Source Scope

```
/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.DataSource/dataSources/customer-data
```

Applies only to the `customer-data` data source.

### Prompt Scope

```
/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Prompt/prompts/support-prompt
```

Applies only to the `support-prompt` prompt.

## Inheritance Rules

| Rule | Description |
|------|-------------|
| **Hierarchical** | Child scopes inherit parent permissions |
| **Additive** | Lower scopes can add permissions |
| **No Reduction** | Cannot remove inherited permissions at lower levels |

### Example

| Assignment | Scope | Effect |
|------------|-------|--------|
| Contributor @ Instance | `/instances/{id}` | Can manage all resources |
| Reader @ Agent | `/instances/{id}/providers/.../agents/x` | Can read agent x |

A user with Contributor at instance level can manage agent x without needing the Reader assignment.

## Choosing Scope

| Scenario | Recommended Scope |
|----------|-------------------|
| Platform administrators | Instance |
| Department access to specific agents | Resource (agent) |
| Data team managing data sources | Resource (data sources) |
| Read-only access to everything | Instance |

## Scope in the Portal

### Instance Access Control

- Navigate to **Security** > **Instance Access Control**
- Assignments here apply to all resources

### Resource Access Control

- Navigate to specific resource (e.g., Agent)
- Click **Access Control** button
- Assignments here apply only to that resource

### Viewing Scope

In role assignment lists, the **Scope** column shows:
- `This resource` - Direct assignment on current resource
- `Instance (inherited)` - Inherited from instance level

## Scope Best Practices

| Practice | Description |
|----------|-------------|
| **Start Narrow** | Grant at resource level when possible |
| **Use Instance Sparingly** | Only for true platform administrators |
| **Group Resources** | Consider organizational structure |
| **Document Scope Decisions** | Add descriptions explaining scope choice |

## Related Topics

- [Role Definitions](role-definitions.md)
- [Role Assignments](role-assignments.md)
- [Role Management](role-management.md)
