# Understand FoundationaLLM role definitions

## Role definition

A role defininition (or just role) is a collection of permissions. A role definition lists the actions that can be performed, such as read, write, and delete.

The following table describes the propoerties of a role definition.

Property | Description
--- | ---
Name | The display name of the role definition.
Id | The unique identifier of the role definition.
Description | The description of the role definition.
Actions | An array of strings that lists the control plane actions that a role definition can perform. For example, `FoundationaLLM.Agent/agents/create`.
NotActions | An array of strings that lists the actions that are excluded from the actions listed in the Actions property.
DataActions | An array of strings that lists the data plane actions that a role definition can perform. For example, `FoundationaLLM.Agent/agents/read`.
NotDataActions | An array of strings that lists the data plane actions that are excluded from the actions listed in the DataActions property.
AssignableScopes | An array of strings that lists the scopes that the role definition can be assigned to.

## Actions format

The string that represents an action has the following format:

`FoundationaLLM.{ProviderName}/{resourceType}/{action}`

Examples of actions include `read`, `write`, and `delete`.

The wildcard character (`*`) can be used to match any resource type or action. For example, `FoundationaLLM.Agent/*/read` matches all read actions for all resource types in the `FoundationaLLM.Agent` provider.

## Role definition example

The following example shows the `Contributor` role definition. The wildcard (`*`) character under `Actions` indicates that the principal assigned to the role can perform all actions (i.e., it can manage everything). This includes also actions defined in the future, as FoundationaLLM adds new resource types. The actions under `NotActions` are subtracted from `Actions`. In this specific case, `NotActions` removes the role's ability to manage access to resources.

```json
{
  "Name": "Contributor",
  "Id": "e459c3a6-6b93-4062-85b3-fffc9fb253df",
  "Description": "Allows you to manage everything except access to resources.",
  "Actions": [
    "*"
  ],
  "NotActions": [
    "FoundationaLLM.Authorization/*/delete",
    "FoundationaLLM.Authorization/*/write"
  ],
  "DataActions": [],
  "NotDataActions": [],
  "AssignableScopes": [
    "/"
  ]
}
```

## Control and data actions

Control plane actions are specified in the `Actions` and `NotActions` properties.

Examples of control plane actions in FoundationaLLM include:

- Manage access to an agent
- Create a new data source
- Delete a prompt

Data plane actions are specified in the `DataActions` and `NotDataActions` properties.

**NOTE**: FoundationaLLM maintains a strict separation between the control and data planes. Control plane access is not inherited to the data plane. For example, if a user has the `FoundationaLLM.Agent/agents/create` permission, it does not mean that the user has the `FoundationaLLM.Agent/agents/read` permission.
