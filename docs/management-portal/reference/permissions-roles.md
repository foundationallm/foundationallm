# Permissions & Roles Reference

Reference documentation for FoundationaLLM permissions and role definitions.

## Overview

This document provides a reference for all roles and permissions in FoundationaLLM.

> **Note:** This reference is generated from `AuthorizableActions.json` and `RoleDefinitions.json`.

## Role Definitions

### Owner
Full access to all resources and operations.

**Permissions:**
- All actions on all resources

### Contributor
Can create and manage resources but cannot manage access.

**Permissions:**
- Create resources
- Update resources
- Delete resources
- Execute operations

### Reader
View-only access to resources.

**Permissions:**
- Read resources
- List resources

## Authorizable Actions

### Agent Actions
- `FoundationaLLM.Agent/agents/read`
- `FoundationaLLM.Agent/agents/write`
- `FoundationaLLM.Agent/agents/delete`

### Prompt Actions
- `FoundationaLLM.Prompt/prompts/read`
- `FoundationaLLM.Prompt/prompts/write`
- `FoundationaLLM.Prompt/prompts/delete`

### Data Pipeline Actions
- `FoundationaLLM.DataPipeline/dataPipelines/read`
- `FoundationaLLM.DataPipeline/dataPipelines/write`
- `FoundationaLLM.DataPipeline/dataPipelines/process`

## Scope Hierarchy

```
/instances/{instanceId}
    /providers/FoundationaLLM.Agent
        /agents/{agentName}
    /providers/FoundationaLLM.Prompt
        /prompts/{promptName}
    ...
```

## Related Topics

- [Role-Based Access Control](../../platform-operations/security-permissions/role-based-access-control/index.md)
- [Role Definitions](../../platform-operations/security-permissions/role-based-access-control/role-definitions.md)
- [Instance Access Control](../how-to-guides/security/instance-access-control.md)
