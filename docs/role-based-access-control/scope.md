# Understand scope for FoundationaLLM RBAC

_Scope_ is the set of resources that a role assignment can access. When assigning roles, it is importan to understand how scope works so that you grant security principals only the level of access they need. Limiting scope also limits the potential damage that can be done if a security principal is compromised.

## Scope levels

In FoundationaLLM, you can specify a scope at the following levels:

- _Instance_: The FoundationaLLM deployment itself.
- _Resource_: A specific resource in FoundationaLLM, such as an agent.

The following rules apply to scope levels:

- Scopes are structured as a hierarchy. For example, a resource scope is always a child of an instance scope.
- Each level make the scope more specific. For example, a resource scope is more specific than an instance scope.
- Roles can be assigned at any of these levels of scope.
- Lower levels inherit the permissions of higher levels. For example, a role assignment at the instance level applies to all resources in the instance.

## Scope format

Scope is a string that identifies the exact scope of the role assignment. The scope is usually referred to as the _resource identifier_ or _resource ID_.

The scope consists of a series of identifiers separated by the slash (/) character. You can think of this string as expressing the following hierarchy, where text without placeholders (`{}`) are fixed identifiers:

```text
/instances
    /{instanceId}
        /providers
            /{providerName}
                /{resourceType}
                    /{resourceSubType1}
                        /{resourceSubType2}
                            /{resourceName}
```

- `{instanceId}` is unique identifier of the FoundationaLLM deployment (a GUID).
- `{providerName}` is the name of the FoundationaLLM resource provider (for example, `FoundationaLLM.Agent`).
- `{resourceType}` and `{resourceSubType*}` identify levels within the resource provider.
- `{resourceName}` is the name of a specific resource.

## Scope examples

Scope | Example
--- | ---
Instance | `/instances/11111111-1111-1111-1111-111111111111`
Resource | `/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Agent/agents/agent1`
Resource | `/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.DataSource/dataSources/datasource1`
Resource | `/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Agent/agents/agent1/models/gpt4`.