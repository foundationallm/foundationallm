# History of breaking changes

> [!NOTE]
> This section is for changes that are not yet released but will affect future releases.

## Starting with 0.9.4-rc100

### Configuration changes

## App configuration settings

>[!IMPORTANT]
> The App Config setting `FoundationaLLM:Instance:EnableResourceProvidersCache` is obsolete and should be removed from the App Config settings.

The following App Config properties make cache settings for the resource providers configurable:

| Name | Description | Default Value  |
|---|---|---|
| `FoundationaLLM:ResourceProvidersCache:EnableCache` | Indicates whether resource providers should cache resources or not.| `true` |
| `FoundationaLLM:ResourceProvidersCache:AbsoluteCacheExpirationSeconds` | Absolute cache expiration in seconds.| 300 |
| `FoundationaLLM:ResourceProvidersCache:SlidingCacheExpirationSeconds` | Sets how many seconds the cache entry can be inactive (e.g. not accessed) before it will be removed. This will not extend the entry lifetime beyond the absolute expiration (if set). | 120 |
| `FoundationaLLM:ResourceProvidersCache:CacheSizeLimit` | The maximum number of items that can be stored in the cache. | 10000 |
| `FoundationaLLM:ResourceProvidersCache:CacheExpirationScanFrequencySeconds` | Gets or sets the minimum length of time between successive scans for expired items in seconds. | 30 |

## Starting with 0.9.3

This version introduces the concept of a well-known virtual security group (`AllAgentsVirtualSecurityGroup`) that is used by agents using Agent Access Token authentication and have their own virtual security group defined. Assign the following PBAC and RBAC roles to the `AllAgentsVirtualSecurityGroup` (replace the tokens denoted by `{{...}}` with the actual values):

### PBAC changes

```json
{
    "name": "{{pbacConversationsOwnerGuid}}",
    "type": "FoundationaLLM.Authorization/policyAssignments",
    "object_id": "/providers/FoundationaLLM.Authorization/policyAssignments/{{pbacConversationsOwnerGuid}}",
    "description": "Ownership on conversation resources for AllAgentsVirtualSecurityGroup by the FoundationaLLM.Conversation resource provider.",
    "policy_definition_id": "/providers/FoundationaLLM.Authorization/policyDefinitions/00000000-0000-0000-0001-000000000001",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Conversation/conversations",
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": "SYSTEM",
    "updated_by": "SYSTEM"
},
{
    "name": "{{pbacConversationMappingsGuid}}",
    "type": "FoundationaLLM.Authorization/policyAssignments",
    "object_id": "/providers/FoundationaLLM.Authorization/policyAssignments/{{pbacConversationMappingsGuid}}",
    "description": "Ownership on conversation mapping resources for AllAgentsVirtualSecurityGroup managed by the FoundationaLLM.AzureOpenAI resource provider.",
    "policy_definition_id": "/providers/FoundationaLLM.Authorization/policyDefinitions/00000000-0000-0000-0001-000000000001",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.AzureOpenAI/conversationMappings",
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": "SYSTEM",
    "updated_by": "SYSTEM"
},
{
    "name": "{{pbacAttachmentsOwnerGuid}}",
    "type": "FoundationaLLM.Authorization/policyAssignments",
    "object_id": "/providers/FoundationaLLM.Authorization/policyAssignments/{{pbacAttachmentsOwnerGuid}}",
    "description": "Ownership on attachment resources for AllAgentsVirtualSecurityGroup managed by the FoundationaLLM.Attachment resource provider.",
    "policy_definition_id": "/providers/FoundationaLLM.Authorization/policyDefinitions/00000000-0000-0000-0001-000000000001",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Attachment/attachments",
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": "SYSTEM",
    "updated_by": "SYSTEM"
},
{
    "name": "{{pbacFileMappingsGuid}}",
    "type": "FoundationaLLM.Authorization/policyAssignments",
    "object_id": "/providers/FoundationaLLM.Authorization/policyAssignments/{{pbacFileMappingsGuid}}",
    "description": "Ownership on file mapping resources for AllAgentsVirtualSecurityGroup managed by the FoundationaLLM.AzureOpenAI resource provider.",
    "policy_definition_id": "/providers/FoundationaLLM.Authorization/policyDefinitions/00000000-0000-0000-0001-000000000001",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.AzureOpenAI/fileMappings",
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": "SYSTEM",
    "updated_by": "SYSTEM"
}
```

### RBAC changes

```json
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{openAiAssistantsReaderGuid}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{openAiAssistantsReaderGuid}}",
    "display_name": null,
    "description": "Read Access for OpenAIAssistants for the AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Agent/workflows/OpenAIAssistants",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{langGraphReactAgentReaderGuid}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{langGraphReactAgentReaderGuid}}",
    "display_name": null,
    "description": "Read Access for LangGraphReactAgent for the AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Agent/workflows/LangGraphReactAgent",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{attachmentContributorGuid2}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{attachmentContributorGuid2}}",
    "display_name": null,
    "description": "Attachment contributor role for AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/8e77fb6a-7a78-43e1-b628-d9e2285fe25a",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{conversationContributorGuid2}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{conversationContributorGuid2}}",
    "display_name": null,
    "description": "Conversation contributor role for AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/d0d21b90-5317-499a-9208-3a6cb71b84f9",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{configReadAccessGuid3}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{configReadAccessGuid3}}",
    "display_name": null,
    "description": "Read Access for configuration for the AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Configuration/appConfigurations/FoundationaLLM:APIEndpoints:CoreAPI:Configuration:MaxUploadsPerMessage",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{configReadAccessGuid4}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{configReadAccessGuid4}}",
    "display_name": null,
    "description": "Read Access for configuration for the AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Configuration/appConfigurations/FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CompletionResponsePollingIntervalSeconds",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{externalAgentWorkflowReaderGuid}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{externalAgentWorkflowReaderGuid}}",
    "display_name": null,
    "description": "Read Access for ExternalAgentWorkflow for the AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Agent/workflows/ExternalAgentWorkflow",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{langChainExpressionLanguageReaderGuid}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{langChainExpressionLanguageReaderGuid}}",
    "display_name": null,
    "description": "Read Access for LangChainExpressionLanguage for the AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Agent/workflows/LangChainExpressionLanguage",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{openAIAssistantsFileSearchReaderGuid}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{openAIAssistantsFileSearchReaderGuid}}",
    "display_name": null,
    "description": "Read Access for OpenAIAssistantsFileSearch for the AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Agent/tools/OpenAIAssistantsFileSearch",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{openAIAssistantsCodeInterpreterReaderGuid}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{openAIAssistantsCodeInterpreterReaderGuid}}",
    "display_name": null,
    "description": "Read Access for OpenAIAssistantsCodeInterpreter for the AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Agent/tools/OpenAIAssistantsCodeInterpreter",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{dalleImageGenerationReaderGuid}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{dalleImageGenerationReaderGuid}}",
    "display_name": null,
    "description": "Read Access for DALLEImageGeneration for the AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Agent/tools/DALLEImageGeneration",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
},
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "{{foundationaLLMContentSearchToolReaderGuid}}",
    "object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/{{foundationaLLMContentSearchToolReaderGuid}}",
    "display_name": null,
    "description": "Read Access for FoundationaLLMContentSearchTool for the AllAgentsVirtualSecurityGroup group.",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "principal_id": "5bb493a2-5909-4771-93ba-d83b7b5a1de9",
    "principal_type": "Group",
    "scope": "/instances/{{instanceId}}/providers/FoundationaLLM.Agent/tools/FoundationaLLMContentSearchTool",
    "properties": null,
    "created_on": "{{deployTime}}",
    "updated_on": "{{deployTime}}",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
}
```

## Starting with 0.9.3-rc016

### Configuration Resource Provider

The `APIEnpointConfiguration` class has been updated to change the previous `StatusUrl` property to `StatusEndpoint`, which is a relative path to the status endpoint. By extension, the related JSON files now have a `status_endpoint` property that contains the relative path. Here is the OrchestrationAPI JSON template as an example of this change:

```json
{
    "type": "api-endpoint",
    "name": "OrchestrationAPI",
    "object_id": "/instances/{{instanceId}}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/OrchestrationAPI",
    "display_name": null,
    "description": null,
    "cost_center": null,
    "category": "General",
    "authentication_type": "APIKey",
    "authentication_parameters": {
        "api_key_configuration_name": "FoundationaLLM:APIEndpoints:OrchestrationAPI:Essentials:APIKey",
        "api_key_header_name": "X-API-KEY"
    },
    "url": "http://orchestration-api.{{serviceNamespaceName}}.svc.cluster.local",
    "status_endpoint": "/instances/{{instanceId}}/status",
    "url_exceptions": [],
    "timeout_seconds": 2400,
    "retry_strategy_name": "ExponentialBackoff",
    "created_on": "0001-01-01T00:00:00+00:00",
    "updated_on": "0001-01-01T00:00:00+00:00",
    "created_by": null,
    "updated_by": "SYSTEM",
    "deleted": false
}
```

The status path is used by the Management Portal's Deployment Information page to show the status of each of the APIs. This path is also used by the `/Orchestration/Services/LangChainService` and `/Orchestration/Services/SemanticKernelService` classes to check the status of the respective APIs.

> [!IMPORTANT]
> All files within the `/resource-provider/FoundationaLLM.Configuration` directory must be updated to change the name of the `status_url` field to `status_endpoint` and change the value to a relative path as needed.

### Vectorization resource provider changes

Vectorization indexing and partitioning profile settings dictionary keys are now persisted as snake case (ex. `IndexName` becomes `index_name`).

### Agent resource provider changes

#### Deployment notes

1. Ensure the feature flag is enabled for `FoundationaLLM.Agent.PrivateStore` if using the private store feature on OpenAI Assistants workflow agents is desired.
2. Open existing `OpenAIAssistantsWorkflow` agents in the Management portal and select `Save` to populate the global vector store in the OpenAI service for the assistant.

#### Agent file resources

The agent file references are now stored in a new Cosmos DB container, while the file contents are stored in the storage account. 
Here are the configuration parameters for the required Cosmos DB container:

Name | Value
--- | ---
Name | `Agents`
Maximum RU/s | 4000
Hierarchical Partition key | `/instanceId` + `/agentName`

As a result of the migration, the newly created `Agents` container will initially contain only ony type of times: `AgentFileReference`.

This is an example of such item:

```json
{
    "instanceId": "8ac6074c-bdde-43cb-a140-ec0002d96d2b",
    "agentName": "TestAgentFiles1",
    "originalFilename": "curious_cat_story.pdf",
    "contentType": "application/pdf",
    "size": 2433,
    "upn": "andrei@foundationaLLM.ai",
    "id": "af-0285ddb8-a5b8-48b0-8248-bd0ad2f123bf",
    "objectId": "/instances/8ac6074c-bdde-43cb-a140-ec0002d96d2b/providers/FoundationaLLM.Agent/agents/TestAgentFiles1/agentFiles/af-0285ddb8-a5b8-48b0-8248-bd0ad2f123bf",
    "name": "af-0285ddb8-a5b8-48b0-8248-bd0ad2f123bf",
    "filename": "/FoundationaLLM.Agent/8ac6074c-bdde-43cb-a140-ec0002d96d2b/TestAgentFiles1/private-file-store/af-0285ddb8-a5b8-48b0-8248-bd0ad2f123bf.pdf",
    "type": "agent-file",
    "deleted": false,
    "_rid": "ie9IAMu0+b0EAAAAAAAAAA==",
    "_self": "dbs/ie9IAA==/colls/ie9IAMu0+b0=/docs/ie9IAMu0+b0EAAAAAAAAAA==/",
    "_etag": "\"37012abc-0000-0200-0000-67afaa800000\"",
    "_attachments": "attachments/",
    "_ts": 1739565696
}
```

The `agent-file` type has been removed and the references are no longer saved in the agent reference store `_resource-references.json`.

### Tools

In the agent resource provider storage folder (`resource-provider/FoundationaLLM.Agent`), add a `tool` resource reference entry (`_resource_references.json`) as well as configuration file for the following tools: `OpenAIAssistantsFileSearch`, `OpenAIAssistantsCodeInterpreter`, `DALLEImageGeneration`, and `FoundationaLLMContentSearchTool`.

Reference example:

```json
{
    "Name": "OpenAIAssistantsFileSearch",
    "Filename": "/FoundationaLLM.Agent/OpenAIAssistantsFileSearch.json",
    "Type": "tool",
    "Deleted": false
}
```

File example:

```json
{
  "type": "tool",
  "name": "OpenAIAssistantsFileSearch",
  "object_id": "/instances/{{instanceId}}/providers/FoundationaLLM.Agent/tools/OpenAIAssistantsFileSearch",
  "display_name": "OpenAIAssistantsFileSearch",
  "description": "OpenAIAssistantsFileSearch",
  "cost_center": null,
  "properties": null,
  "created_on": "2025-01-10T08:22:34.2682433+00:00",
  "updated_on": "0001-01-01T00:00:00+00:00",
  "created_by": "dev@foundationaLLM.ai",
  "updated_by": null,
  "deleted": false,
  "expiration_date": null
}
```

## Starting with 0.9.3-rc010

### Resource provider cache warm-up

Resource providers now support a cache warm-up mechanism. This mechanism allows the cache to be pre-populated with the resource provider data before the service starts processing requests. This feature is useful when the service is deployed in a cold environment and needs to be warmed up before it can handle requests.

The cache warm-up mechanism is enabled when a file named `_cache_warmup.json` exists in the blob storage location associated with the resource provider. Here is an example of such a file:

```json
[
	{
		"ServiceName": "OrchestrationAPI",
		"Description": "Resources required by: service principal x, service principal y.",
		"ResourceObjectIds": [
			"/instances/73fad442-.../providers/FoundationaLLM.Configuration/apiEndpointConfigurations/GatewayAPI",
			"/instances/73fad442-.../providers/FoundationaLLM.Configuration/apiEndpointConfigurations/AzureAISearch",
			"/instances/73fad442-.../providers/FoundationaLLM.Configuration/apiEndpointConfigurations/LangChainAPI",
			"/instances/73fad442-.../providers/FoundationaLLM.Configuration/apiEndpointConfigurations/StateAPI"
		],
		"SecurityPrincipalIds": [
			"4150c6b3-...",
			"949195b1-..."
		]
	},
	{
		"ServiceName": "OrchestrationAPI",
		"Description": "Resources required by: service principal x, service principal y, service principal z.",
		"ResourceObjectIds": [
			"/instances/73fad442-.../providers/FoundationaLLM.Configuration/apiEndpointConfigurations/AzureOpenAI"
		],
		"SecurityPrincipalIds": [
			"4150c6b3-...",
			"949195b1-...",
			"d6a6317a-..."
		]
	}
]
```
The configuration contains an array of objects, each representing a cache warm-up configuration. Each object contains the following properties:

- `ServiceName` - The name of the service that the cache warm-up configuration is for.
- `Description` - A description of the cache warm-up configuration.
- `ResourceObjectIds` - The list of resource object identifiers that will be pre-loaded into the resource provider cache.

    >[!NOTE]
    > The resource object identifiers must be specific to the resource provider.

- `SecurityPrincipalIds` - The list of security principal identifiers that will be used to authenticate the cache warm-up requests.

    >[!IMPORTANT]
    > As a result of the cache warm-up process, the client authorization cache will be populated with all combinations of security principal and resource object identifiers that exist in the cache warm-up configuration. Make sure the two lists only contain the necessary values to avoid a long startup time for the resource provider.

## Starting with 0.9.3-rc002

## App configuration settings

The following App Config properties make cache settings for the `AuthorizationServiceClientCacheService` configurable:

| Name                              | Description                                                                                                                   | Default Value  |
|-----------------------------------|-------------------------------------------------------------------------------------------------------------------------------|--------|
| `FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:EnableCache`    | Indicates whether calls to the Authorization API should be cached or not.                                                                                         | `false`    |
| `FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:AbsoluteCacheExpirationSeconds`    | Absolute cache expiration in seconds.                                                                                         | 300    |
| `FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:SlidingCacheExpirationSeconds`     | Sets how many seconds the cache entry can be inactive (e.g. not accessed) before it will be removed. This will not extend the entry lifetime beyond the absolute expiration (if set). | 120    |
| `FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:CacheSizeLimit`                    | The maximum number of items that can be stored in the cache.                                                                   | 10000  |
| `FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:CacheExpirationScanFrequencySeconds` | Gets or sets the minimum length of time between successive scans for expired items in seconds.                                | 30     |

## Starting with 0.9.2-rc005

### Agent configuration changes

Starting with this version, all agents MUST transition to the agent workflow configuraiton approach.

The following agent properties are no longer supported and should be deleted as part of upgrading to this version:

- `OrchestrationSettings` - fully replaced by the agent worflow settings.
- `PromptObjectId` - replaced by the agent workflow resource object identifier with an `object_role` of `main_prompt`.
- `AIModelObjectId` - replaced by the agent workflow resource object identifier with an `object_role` of `main_model`.
- `Capabilities` - removed. The equivalent of having Azure OpenAI Assistants capabilities is having an agent workflow with the type `azure-openai-assistants-workflow`.
- `Azure.OpenAI.Assistant.Id` property in `properties` - replaced by the `assistant_id` property of an agent workflow witht the type `azure-openai-assistants-workflow`.

>[!IMPORTANT]
>If the `Azure.OpenAI.Assistant.Id` property is set in the agent properties, it's value must be copied to the `assistant_id` property of the agent workflow.

Here is an example of a fully configured worfklow section for an agent:

```json
{
    "type": "azure-openai-assistants-workflow",
		"name": "OpenAIAssistants",
		"package_name": "FoundationaLLM",
    "assistant_id": "asst_...",
    "resource_object_ids": {
        "/instances/.../providers/FoundationaLLM.Agent/workflows/OpenAIAssistants": {
            "object_id": "/instances/.../providers/FoundationaLLM.Agent/workflows/OpenAIAssistants",
            "properties": {}
        },
        "/instances/.../providers/FoundationaLLM.AIModel/aiModels/GPT4oMiniCompletionAIModel" : {
            "object_id": "/instances/.../providers/FoundationaLLM.AIModel/aiModels/GPT4oMiniCompletionAIModel",
            "properties": {
                "object_role": "main_model",
                "model_parameters": {}
            }
        },
        "/instances/.../providers/FoundationaLLM.Prompt/prompts/FoundationaLLM-mini": {
            "object_id": "/instances/.../providers/FoundationaLLM.Prompt/prompts/FoundationaLLM-mini",
            "properties": {
                "object_role": "main_prompt"
            }
        }
    }
}
```

## Starting from 0.9.1

### App configuration settings

To support the event grid infrastructure, the following new App Configuration settings are required.

```json
[
	{
		"key": "FoundationaLLM:Events:Profiles:CoreAPI",
		"label": null,
		"value": "{\"EventProcessingCycleSeconds\": 5,\"Topics\": [{\"Name\": \"resource-providers\",\"SubscriptionPrefix\": \"rp-core\"}]}",
		"content_type": "application/json",
		"tags": {}
	},
	{
		"key": "FoundationaLLM:Events:Profiles:GatekeeperAPI",
		"label": null,
		"value": "{\"EventProcessingCycleSeconds\":60,\"Topics\":[]}",
		"content_type": "application/json",
		"tags": {}
	},
	{
		"key": "FoundationaLLM:Events:Profiles:GatewayAPI",
		"label": null,
		"value": "{\"EventProcessingCycleSeconds\": 5,\"Topics\": [{\"Name\": \"resource-providers\",\"SubscriptionPrefix\": \"rp-gateway\"}]}",
		"content_type": "application/json",
		"tags": {}
	},
	{
		"key": "FoundationaLLM:Events:Profiles:ManagementAPI",
		"label": null,
		"value": "{\"EventProcessingCycleSeconds\": 5,\"Topics\": [{\"Name\": \"resource-providers\",\"SubscriptionPrefix\": \"rp-management\"}]}",
		"content_type": "application/json",
		"tags": {}
	},
	{
		"key": "FoundationaLLM:Events:Profiles:OrchestrationAPI",
		"label": null,
		"value": "{\"EventProcessingCycleSeconds\": 5,\"Topics\": [{\"Name\": \"resource-providers\",\"SubscriptionPrefix\": \"rp-orch\"}]}",
		"content_type": "application/json",
		"tags": {}
	},
	{
		"key": "FoundationaLLM:Events:Profiles:VectorizationAPI",
		"label": null,
		"value": "{\"EventProcessingCycleSeconds\":60,\"Topics\":[]}",
		"content_type": "application/json",
		"tags": {}
	},
	{
		"key": "FoundationaLLM:Events:Profiles:VectorizationWorker",
		"label": null,
		"value": "{\"EventProcessingCycleSeconds\":60,\"Topics\":[]}",
		"content_type": "application/json",
		"tags": {}
	}
]
```

>Note: The event grid system topics need to be removed.

The following topic needs to be created in the event grid namespace, must have a `resource-providers` topic with a publisher type of `Custom` and an input schema of `Cloud Events v1.0`.

### Configuration changes

Added the following App Configuration value:

|Name | Default value | Description |
|--- | --- | --- |
| `FoundationaLLM:UserPortal:Authentication:Entra:TimeoutInMinutes` | `60` | The timeout in minutes for a user's auth token in the User Portal. |
| `FoundationaLLM:UserPortal:Configuration:ShowFileUpload` | `true` | Global setting to determine if file upload is allowed on chat messages. |

## Starting with 0.9.1-rc117

### Agent configuration changes

```json
"text_rewrite_settings": {
    "user_prompt_rewrite_enabled" : true,
    "user_prompt_rewrite_settings": {
        "user_prompt_rewrite_ai_model_object_id": "/instances/73fad442-f614-4510-811f-414cb3a3d34b/providers/FoundationaLLM.AIModel/aiModels/GPT4oCompletionAIModel",
        "user_prompt_rewrite_prompt_object_id": "/instances/73fad442-f614-4510-811f-414cb3a3d34b/providers/FoundationaLLM.Prompt/prompts/FoundationaLLM-v2-Rewrite",
        "user_prompts_window_size": 1
    }
},
"cache_settings": {
    "semantic_cache_enabled": true,
    "semantic_cache_settings": {
        "embedding_ai_model_object_id": "/instances/73fad442-f614-4510-811f-414cb3a3d34b/providers/FoundationaLLM.AIModel/aiModels/DefaultEmbeddingAIModel",
        "embedding_dimensions": 2048,
        "minimum_similarity_threshold": 0.975
    }
},
```

### Semantic cache

Enable vector search in the Cosmos DB database using the following CLI command:

```cli
az cosmosdb update --resource-group <resource-group-name> --name <account-name> --capabilities EnableNoSQLVectorSearch
```

Create the `CompletionsCache` container in the Cosmos DB database with the following properties:

- **Container id**: `CompletionsCache`
- **Partition key**: `/operationId`
- **Container Vector Policy**: a policy with the following properties:
  - **Path**: `/userPromptEmbedding`
  - **Data type**: `float32`
  - **Distance function**: `Cosine`
  - **Dimensions**: 2048
  - **Index type**: `diskANN` (leave the default values)

After the container is created, set the `Time to Live` property on the container to 300 seconds.

## Starting with 0.9.1-rc105

### Configuration changes

The following new App Configuration settings are required:

|Name | Default value | Description |
|--- | --- | --- |
|`FoundationaLLM:PythonSDK:Logging:LogLevel:Azure` | `Warning` | Provides the default level of logging for Azure modules in the Python SDK. |

### Agent workflow configuration changes

Agent resource configuration files that have a `workflow` property now requires a `name` and `package_name` property. This is to support loading external workflows via plugins. For internal workflows, the `package_name` should be set to `FoundationaLLM`. Example below truncated for brevity.

```json
{
    "workflow": {
        "type": "langgraph-react-agent-workflow",
        "name": "LangGraphReactAgent",
        "package_name": "FoundationaLLM",
        "workflow_host": "LangChain",
        "graph_recursion_limit": 10,
        "resource_object_ids": {}
    }
}
```

A new `Workflow` resource must be added to the `FoundationaLLM.Agent` resource provider:

```json
{
  "type": "external-agent-workflow",
  "name": "ExternalAgentWorkflow",
  "object_id": "/instances/<instance_id>/providers/FoundationaLLM.Agent/workflows/ExternalAgentWorkflow",
  "display_name": "ExternalAgentWorkflow",
  "description": "External Agent workflow",
  "cost_center": null,
  "properties": null,
  "created_on": "2024-11-13T18:12:07.0223039+00:00",
  "updated_on": "0001-01-01T00:00:00+00:00",
  "created_by": "dev@foundationaLLM.ai",
  "updated_by": null,
  "deleted": false,
  "expiration_date": null
}
```

## Starting with 0.9.1-rc102

### Configuration changes

The following new App Configuration settings are required:

|Name | Default value | Description |
|--- | --- | --- |
|`FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration:CompletionRequestsStorage:AccountName` | `<main_storage_account_name>` | Provides the storage account used by the Orchestration API to persist completion requests. |
|`FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration:CompletionRequestsStorage:AuthenticationType` | `AzureIdentity` | Indicates that managed identity authentication should be used to access the storage account. |
|`FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration:CompletionRequestsStorage:ContainerName` | `orchestration-completion-requests` | Provides the storage container name used by the Orchestration API to persist completion requests. Should always be `orchestration-completion-requests` |

### User profile changes

A new flag named `persistOrchestrationCompletionRequests` is added to the user profile. This flag is used to determine whether the user's completion requests should be persisted in the storage account. The default value is `false`.

Sample configuration:

```json
"flags": {
        "oneDriveWorkSchoolEnabled": true,
        "persistOrchestrationCompletionRequests": true
    },
```

## Starting with 0.9.1-rc101

### Configuration changes

The following new App Configuration settings are required:

|Name | Default value | Description |
|--- | --- | --- |
|`FoundationaLLM:Code:CodeExecution:AzureContainerAppsDynamicSessions` | `{"DynamicSessionsEndpoints": []}` | Provides the configuration for the Azure Container Apps Dynamic Sessions code execution service. `DynamicSessionsEnpoints` is a list of Dynamic Sessions endpoints that are used to run code execution sessions. Must contain at least one value. |

### Agent tool configuration changes

Each agent tool should have an entry in the `properties` dictionary named `foundationallm_aca_code_execution_enabled` (`true` or `false`) to indicate whether the tool requires code execution sessions based on the the Azure Container Apps Dynamic Sessions service.

### Prompt definition changes

Prompt prefixes and suffixes support FoundationaLLM variables for dynamic replacement at runtime. The variable format is `{{foundationallm:variable_name[:format]}}` where
- `variable_name` is the name of the well-known variable.
- `format` is the optional formatting applied to the value of the variable.

The following variables are supported:

| Name | Value | Example
| --- | --- | --- |
| `current_datetime_utc` | The current UTC date and time. | `The current date is {{foundationallm:current_datetime_utc:dddd, MMMM dd, yyyy}}. This looks great.` -> `The current date is Sunday, December 15, 2024. This looks great.`

## Starting with 0.9.0

### Configuration changes

The following new App Configuration settings are required:

|Name | Default value | Description |
|--- | --- | --- |
|`FoundationaLLM:PythonSDK:Logging:LogLevel:Default` | `Information` | `-` |
|`FoundationaLLM:PythonSDK:Logging:EnableConsoleLogging` | `false` | `-` |
|`FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:RequireScopes` | `true` | Indicates whether a scope claim (scp) is required for authorization. Set to `false` to allow authentication from an external proxy API. |
|`FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:AllowACLAuthorization` | `false` | Indicates whether tokens that do not have either of the "scp" or "roles" claims are accepted (True means they are accepted). Set to `true` to allow authentication from an external proxy API. |
|`FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:Storage:AccountName` | `-` | `-` |
|`FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:Storage:AuthenticationType` | `-` | `-` |
|`FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:RootStorageContainer` | `-` | `-` |
|`FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:Modules` | `-` | `-` |
|`FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:PollingIntervalSeconds` | `10` | The interval in seconds at which the LangChain API will be polled for status. |
|`FoundationaLLM:UserPortal:Configuration:ShowMessageRating` | `true` | If `true`, rating options on agent messages will appear. |
|`FoundationaLLM:UserPortal:Configuration:ShowLastConversationOnStartup` | `false` | If `true`, the last conversation will be displayed when the user logs in. Otherwise, a new conversation placeholder appears on page load. |
|`FoundationaLLM:UserPortal:Configuration:ShowMessageTokens` | `true` | If `true`, the number of consumed tokens on agent and user messages will appear. |
|`FoundationaLLM:UserPortal:Configuration:ShowViewPrompt` | `true` | If `true`, the "View Prompt" button on agent messages will appear. |
|`FoundationaLLM:Instance:EnableResourceProvidersCache` | `false` | If `true`, the caching of resource providers will be enabled. |
| `FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:EnableCache` | `false` | If `true`, the caching of authorization call results will be enabled. |

#### Agent Tool configuration changes

Agent tools are now an array of AgentTool objects rather than a dictionary.

When defining tools for an agent, each tool now requires a `package_name` property. This property is used to identify the package that contains the tool's implementation. If the tool is internal, the `package_name` should be set to `FoundationaLLM`, if the tool is external, the `package_name` should be set to the name of the external package.

#### Security-related changes

The **Authorization API** now requires the ability to write to the Key Vault account contained within the auth resource group. Currently, the Authorization APIs managed identity is assigned to the `Key Vault Secrets User` role on the Key Vault account. This role assignment must be updated to include the `Key Vault Secrets Officer` role in addition to the user role.

#### Renamed classes

The following classes have been renamed:

| Original Class | New Class |
| --- | --- |
| `FoundationaLLM.Common.Models.Orchestration.Response.Citation` | `FoundationaLLM.Common.Models.Orchestration.Response.ContentArtifact` |

#### API endpoint changes

**Core API**

The `/instances/{instanceId}/sessions/{sessionId}/message/{id}/rate` endpoint has been updated to accept the rating in the message body, rather than as a query parameter. Send the following payload in the request body:

```json
{
  "rating": true,
  "comments": "string"
}
```

> [!NOTE]
> Please note that both properties are nullable. Set them to null to clear out the rating and comments.

## Starting with 0.8.4

### Configuration changes

The following new App Configuration settings are required:

Name | Default value
--- | ---
`FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:AllowedUploadFileExtensions` | `c, cpp, cs, css, csv, doc, docx, gif, html, java, jpeg, jpg, js, json, md, pdf, php, png, pptx, py, rb, sh, tar, tex, ts, txt, xlsx, xml, zip`
`FoundationaLLM:Branding:NoAgentsMessage` | `No agents available. Please check with your system administrator for assistance.`
`FoundationaLLM:Branding:DefaultAgentWelcomeMessage` | `Start the conversation using the text box below.`

The following new App Configuration feature flags are required:

Name | Default value
--- | ---
`FoundationaLLM.Agent.PrivateStore` | `Not enabled`

### Assistants API enabled Agent(s)
> [!IMPORTANT]
> 
> Any existing agent that has the Assistants API enabled needs to be saved from the Management UI to update itself.

### Resource provider changes

**FoundationaLLM.Authorization**

The following entries need to be added to the policy store file:

```json
{
    "name": "GUID03",
    "type": "FoundationaLLM.Authorization/policyAssignments",
    "object_id": "/providers/FoundationaLLM.Authorization/policyAssignments/GUID03",
    "description": "Ownership on conversation mapping resources managed by the FoundationaLLM.AzureOpenAI resource provider.",
    "policy_definition_id": "/providers/FoundationaLLM.Authorization/policyDefinitions/00000000-0000-0000-0001-000000000001",
    "principal_id": "SECURITY_GROUP_ID",
    "principal_type": "Group",
    "scope": "/instances/FOUNDATIONALLM_INSTANCEID/providers/FoundationaLLM.AzureOpenAI/conversationMappings",
    "created_on": "DEPLOY_TIME",
    "updated_on": "DEPLOY_TIME",
    "created_by": "SYSTEM",
    "updated_by": "SYSTEM"
},
{
    "name": "GUID04",
    "type": "FoundationaLLM.Authorization/policyAssignments",
    "object_id": "/providers/FoundationaLLM.Authorization/policyAssignments/GUID04",
    "description": "Ownership on file mapping resources managed by the FoundationaLLM.AzureOpenAI resource provider.",
    "policy_definition_id": "/providers/FoundationaLLM.Authorization/policyDefinitions/00000000-0000-0000-0001-000000000001",
    "principal_id": "SECURITY_GROUP_ID",
    "principal_type": "Group",
    "scope": "/instances/FOUNDATIONALLM_INSTANCEID/providers/FoundationaLLM.AzureOpenAI/fileMappings",
    "created_on": "DEPLOY_TIME",
    "updated_on": "DEPLOY_TIME",
    "created_by": "SYSTEM",
    "updated_by": "SYSTEM"
}
```
The following placehoders need to be replaced with the actual values:
- `SECURITY_GROUP_ID` - the ID of the security group that needs to be assigned to the policy
- `FOUNDATIONALLM_INSTANCEID` - the ID of the FoundationaLLM instance
- `DEPLOY_TIME` - the time when the policy was deployed
- `GUID03` and `GUID04` - unique identifiers for the policy assignments

**FoundationaLLM.AzureOpenAI**

The assistant and file user context artifacts are now simplified and stored in a new Cosmos DB container. Here are the configuration parameters for the new Cosmos DB container:

Name | Value
--- | ---
Name | `ExternalResources`
Maximum RU/s | 1000
Time to live | Off
Partition key | `/partitionKey`

Part of the upgrade to this version is to migrate the existing assistant and file user context artifacts to the new Cosmos DB container.
Refer to the dedicated upgrade tool for instructions on how to perform this update.

As a result of the migration, the newly created `ExternalResources` container will contain two types of items: `AzureOpenAIConversationMapping` and `AzureOpenAIFileMapping`.

This is an example of an `AzureOpenAIConversationMapping` item:

```json
{
    "conversationId": "0e56a170-5355-...",
    "openAIAssistantsAssistantId": "asst_kc...",
    "openAIAssistantsThreadId": "thread_73...",
    "openAIAssistantsThreadCreatedOn": "2024-10-14T17:57:10.510345+00:00",
    "openAIVectorStoreId": "vs_X6...",
    "openAIVectorStoreCreatedOn": null,
    "type": "AzureOpenAIConversationMapping",
    "id": "0e56a170-5355-...",
    "partitionKey": "...-73fad442-f614-4510-811f-414cb3a3d34b",
    "upn": "jackthecat@foundationaLLM.ai",
    "instanceId": "73fad442-f614-4510-811f-414cb3a3d34b",
    "openAIEndpoint": "https://openai-....openai.azure.com/",
    "objectId": null,
    "displayName": null,
    "description": null,
    "costCenter": null,
    "properties": null,
    "createdOn": "0001-01-01T00:00:00+00:00",
    "updatedOn": "0001-01-01T00:00:00+00:00",
    "createdBy": null,
    "updatedBy": null,
    "deleted": false,
    "expirationDate": null,
    "name": "0e56a170-5355-...",
    "_rid": "J2BUAKktW41bAAAAAAAAAA==",
    "_self": "dbs/J2BUAA==/colls/J2BUAKktW40=/docs/J2BUAKktW41bAAAAAAAAAA==/",
    "_etag": "\"8702b793-0000-0200-0000-672a60b90000\"",
    "_attachments": "attachments/",
    "_ts": 1730830521
}
```

This is an example of an `AzureOpenAIFileMapping` item:

```json
{
    "fileObjectId": "/instances/73fad442-f614-4510-811f-414cb3a3d34b/providers/FoundationaLLM.Attachment/attachments/a-f8...",
    "originalFileName": "some_file.csv",
    "fileContentType": "text/csv",
    "fileRequiresVectorization": false,
    "openAIFileId": "assistant-8G...",
    "openAIFileUploadedOn": "2024-10-14T23:01:02.3075592+00:00",
    "openAIAssistantsFileGeneratedOn": null,
    "openAIVectorStoreId": null,
    "type": "AzureOpenAIFileMapping",
    "id": "assistant-8G...",
    "partitionKey": "...-73fad442-f614-4510-811f-414cb3a3d34b",
    "upn": "jackthecat@foundationaLLM.ai",
    "instanceId": "73fad442-f614-4510-811f-414cb3a3d34b",
    "openAIEndpoint": "https://openai-....openai.azure.com/",
    "objectId": null,
    "displayName": null,
    "description": null,
    "costCenter": null,
    "properties": null,
    "createdOn": "0001-01-01T00:00:00+00:00",
    "updatedOn": "0001-01-01T00:00:00+00:00",
    "createdBy": null,
    "updatedBy": null,
    "deleted": false,
    "expirationDate": null,
    "name": "assistant-8G...",
    "_rid": "J2BUAKktW40yAAAAAAAAAA==",
    "_self": "dbs/J2BUAA==/colls/J2BUAKktW40=/docs/J2BUAKktW40yAAAAAAAAAA==/",
    "_etag": "\"87025e93-0000-0200-0000-672a60b70000\"",
    "_attachments": "attachments/",
    "_ts": 1730830519
}
```

### Cleanup role assignments
As a result of migrated resources from storage account to Cosmos DB, as well as the new `policy-assignments` mentioned above, the `role-assignments` store will have obsolete `Owner` role assignments on those objects. Please refer to the dedicated tool for instructions on how to perform this cleanup.

The dedicated tool will cleanup role assignments for the following resources:
- `FoundationaLLM.Attachment/attachments`
- `FoundationaLLM.AzureOpenAI/fileUserContexts`
- `FoundationaLLM.AzureOpenAI/assistantUserContexts`
- `FoundationaLLM.Conversation/conversations`

### Configuration changes

#### Resource provider templates

The `AzureOpenAI.template.json` files within `deploy/quick-start/data/resource-provider/FoundationaLLM.Configuration` and `deploy/standard/data/resource-provider/FoundationaLLM.Configuration` have been updated to set the `category` field to the value `LLM`. This discriminator allows the Management Portal to filter the list of API endpoints by category and provide options to add AI Models to endpoints with the `LLM` category.

The existing `category` property needs to be set to `LLM` on existing API endpoint configurations in the `FoundationaLLM.Configuration` resource provider that fit this description, including the `AzureOpenAI` endpoint configuration.

## Starting with 0.8.3

### Resource provider changes

If a user/group is not assigned to the instance-level Contributor role, then they will not be able to create new Conversations or upload Attachments. To adjust their permissions, the following changes are required:

**FoundationaLLM.Conversation**

In addition to assigning users/groups to the `policy-assignments/<instance_id>-policy.json` file within the `FoundationaLLM.Authorization` resource provider to assign them to the Conversation policy, we must now add them to the new **Conversation contributor role** (`role_definition_id`: `d0d21b90-5317-499a-9208-3a6cb71b84f9`) within the `role-assignments/<instance_id>-role.json` file within the `FoundationaLLM.Authorization` resource provider if the user/group is not assigned to the Contributor role on the FoundationaLLM instance (`role_definition_id`: `a9f0020f-6e3a-49bf-8d1d-35fd53058edf`). Here is an example entry:

```json
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "a40b15f1-75ce-4a40-a857-1093ac9adf4d",
    "object_id": "/instances/0a1840df-71b6-496d-905a-145d93d827f3/providers/FoundationaLLM.Authorization/roleAssignments/a40b15f1-75ce-4a40-a857-1093ac9adf4d",
    "display_name": null,
    "description": "Conversation contributor role for FLLM Users",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/d0d21b90-5317-499a-9208-3a6cb71b84f9",
    "principal_id": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
    "principal_type": "Group",
    "scope": "/instances/0a1840df-71b6-496d-905a-145d93d827f3",
    "properties": null,
    "created_on": "0001-01-01T00:00:00+00:00",
    "updated_on": "0001-01-01T00:00:00+00:00",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
}
```

**FoundationaLLM.Attachment**

In addition to assigning users/groups to the `policy-assignments/<instance_id>-policy.json` file within the `FoundationaLLM.Authorization` resource provider to assign them to the Attachment policy, we must now add them to the new **Attachment contributor role** (`role_definition_id`: `8e77fb6a-7a78-43e1-b628-d9e2285fe25a`) within the `role-assignments/<instance_id>-role.json` file within the `FoundationaLLM.Authorization` resource provider if the user/group is not assigned to the Contributor role on the FoundationaLLM instance (`role_definition_id`: `a9f0020f-6e3a-49bf-8d1d-35fd53058edf`). Here is an example entry:

```json
{
    "type": "FoundationaLLM.Authorization/roleAssignments",
    "name": "891ca947-e648-46cf-a12a-774b52ded886",
    "object_id": "/instances/0a1840df-71b6-496d-905a-145d93d827f3/providers/FoundationaLLM.Authorization/roleAssignments/891ca947-e648-46cf-a12a-774b52ded886",
    "display_name": null,
    "description": "Attachment contributor role for FLLM Users",
    "cost_center": null,
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/8e77fb6a-7a78-43e1-b628-d9e2285fe25a",
    "principal_id": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
    "principal_type": "Group",
    "scope": "/instances/0a1840df-71b6-496d-905a-145d93d827f3",
    "properties": null,
    "created_on": "0001-01-01T00:00:00+00:00",
    "updated_on": "0001-01-01T00:00:00+00:00",
    "created_by": null,
    "updated_by": null,
    "deleted": false,
    "expiration_date": null
}
```

## Starting with 0.8.2

### Configuration changes

The following settings are required:

Name | Default value
--- | ---
`FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AllowedUploadFileExtensions` | `c, cpp, cs, css, csv, doc, docx, gif, html, java, jpeg, jpg, js, json, md, pdf, php, png, pptx, py, rb, sh, tar, tex, ts, txt, xlsx, xml, zip`
`FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AzureOpenAIAssistantsFileSearchFileExtensions` | `c, cpp, cs, css, doc, docx, html, java, js, json, md, pdf, php, pptx, py, rb, sh, tex, ts, txt`
`FoundationaLLM:APIEndpoints:CoreAPI:Configuration:MaxUploadsPerMessage` |	`{ "value": 10, "value_exceptions": [] }`
`FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CompletionResponsePollingIntervalSeconds` | `{ "value": 5, "value_exceptions": [] }`
`FoundationaLLM:APIEndpoints:GatewayAPI:Configuration:AzureOpenAIAssistantsMaxVectorizationTimeSeconds` | `120`

>[!NOTE]
> Here is an example of an override for the `MaxUploadsPerMessage` setting:
> ```json
>{
>   "value": 10,
>   "value_exceptions": [
>       {
>           "user_principal_name": "ciprian@solliance.net",
>           "value": 5,
>           "enabled": true
>       }
>   ]
>}
> ```

>[!NOTE]
> Here is an example of an override for the `CompletionResponsePollingIntervalSeconds` setting:
> ```json
>{
>   "value": 5,
>   "value_exceptions": [
>       {
>           "user_principal_name": "ciprian@solliance.net",
>           "value": 3,
>           "enabled": true
>       }
>   ]
>}
>

The following settings are optional (they should not be set by default):

Name | Default value
--- | ---
`FoundationaLLM:Instance:IdentitySubstitutionSecurityPrincipalId` | <security_principal_id>
`FoundationaLLM:Instance:IdentitySubstitutionUserPrincipalNamePattern` | `^fllm_load_test_user_\d{5}_\d{3}@solliance\.net$`

>[!NOTE]
> The `FoundationaLLM:Instance:IdentitySubstitutionSecurityPrincipalId` and `FoundationaLLM:Instance:IdentitySubstitutionUserPrincipalNamePattern` settings are used for load testing purposes only. If set, their values must be replaced with the appropriate values for the specific Entra ID tenant.

### Resource provider changes

The following resource provider files must be renamed (if they already exist):

Location | Old name | New name
--- | --- | ---
`resource-provider/FoundationaLLM.Agent` | `_agent-references.json` | `_resource-references.json`
`resource-provider/FoundationaLLM.AIModel` | `_ai-model-references.json` | `_resource-references.json`
`resource-provider/FoundationaLLM.Configuration` | `_api-endpoint-references.json` | `_resource-references.json`
`resource-provider/FoundationaLLM.DataSource` | `_data-source-references.json` | `_resource-references.json`
`resource-provider/FoundationaLLM.Prompt` | `_prompt-references.json` | `_resource-references.json`

>[!NOTE]
> Within each of the renamed files, the `<entity>References` property must be renamed to `ResourceReferences`.

**FoundationaLLM.Agent**

A new property can be added to agent definitions:

```json
"tools": {
    "dalle-image-generation": {
        "name": "dalle-image-generation",
        "description": "Generates an image based on a prompt.",
        "ai_model_object_ids": {
            "main_model": "/instances/73fad442-f614-4510-811f-414cb3a3d34b/providers/FoundationaLLM.AIModel/aiModels/DALLE3"
        }
    }
}
```

**FoundationaLLM.Authorization**

A new storage container named `policy-assignments` is required. The `FoundationaLLM.Authorization` resource provider will use this container to store policy assignments.

Within the container, the `<instance_id>-policy.json` must be deployed with the default policy assignments. The template for the default policy assignments is available in `Common/Constants/Data/DefaultPolicyAssignments.json`.

**FoundationaLLM.Conversation**

When upgrading an existing FoundationaLLM instance, the items in the `Sessions` collection in Cosmos DB must be updated according to the following rules:

```
if Object is of type Session of KioskSession:
    If the property DisplayName exists and is set to a non-empty string:
        Don't touch the item
    else:
        Set DisplayName to the value of Name
        Set Name to the value of SessionId
else:
    No action needed
```

Refer to the dedicated upgrade tool for instruction on how to perform this update.

**FoundationaLLM.Configuration**

The OneDrive (Work or School) integration requires the following API Endpoint Configuration entry in the storage account:

`FoundationaLLM.Configuration/OneDriveFileStoreConnector.json`
```json
{
    "type": "api-endpoint",
    "name": "OneDriveFileStoreConnector",
    "object_id": "/instances/{{instance_id}}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/OneDriveFileStoreConnector",
    "display_name": null,
    "description": null,
    "cost_center": null,
    "category": "FileStoreConnector",
    "subcategory": "OneDriveWorkSchool",
    "authentication_type": "AzureIdentity",
    "authentication_parameters": {
      "scope": "Files.Read.All"
    },
    "url": "{{onedrive_base_url}}",
    "status_url": "",
    "url_exceptions": [],
    "timeout_seconds": 2400,
    "retry_strategy_name": "ExponentialBackoff",
    "created_on": "0001-01-01T00:00:00+00:00",
    "updated_on": "0001-01-01T00:00:00+00:00",
    "created_by": null,
    "updated_by": "SYSTEM",
    "deleted": false
  }
```

Update `FoundationaLLM.Configuration/_resource-references_.json` with the reference to the file above.
```json
{
	"Name": "OneDriveFileStoreConnector",
	"Filename": "/FoundationaLLM.Configuration/OneDriveFileStoreConnector.json",
	"Type": "api-endpoint",
	"Deleted": false
}
```

**FoundationaLLM.Attachment**

The Attachment resource provider now saves the attachment references to Cosmos DB, instead of Data Lake storage.
A new Cosmos DB container must be created, named `Attachments`, with the following partition key: `/upn`.

The following MSIs require a Cosmos DB role assigned:
1. Gateway API
2. Orchestration API
3. Management API

### Long-Running Operations

The context for a long-running operation is now stored in Cosmos DB.
A new Cosmos DB container must be created, named `Operations`, with a partition key `/id`.


## Starting with 0.8.0

Core API changes:

1. All Core API endpoints have been moved to the `/instances/{instanceId}` path. For example, the `/status` endpoint is now `/instances/{instanceId}/status`.
2. The `/orchestration/*` endpoints have been moved to `/instances/{instanceId}/completions/*`.
   1. The previous `/orchestration/completions` endpoint is now `/instances/{instanceId}/completions`.
3. The `/sessions/{sessionId}/completion` endpoint has been moved to `/instances/{instanceId}/completions`. Instead of having the `sessionId` as a path parameter, it is now in the request body as part of the `CompletionRequest` payload.
4. `/sessions/{sessionId}/summarize-name` has been removed. In the future, the `/completions` endpoint will be used to generate summaries.
5. `OrchestrationRequest` and `CompletionRequest` have combined into a single `CompletionRequest` object.
6. `DirectionCompletionRequest` has been removed. Use `CompletionRequest` instead.
7. `Status` controllers `\status` action in the .NET API projects return value has renamed the `Instance` property to `InstanceName`.
8. The `CompletionController.cs` file under `dotnet/CoreApi/controllers` has introduced the `Async-Completions` endpoint to handle asynchronous completions.
9. With the introduction of `Async-Completions`, long running operations can now report on completion status based on `Pending`, `InProgress`, `Completed` and `Failed` states.
10. Vectorization Embedding Profile introduces a required key in the `Settings` property named `model_name`. Every embedding request now flows through the Gateway API.
11. Vectorization Indexing Profile introduces a required key `api_endpoint_configuration_object_id` in the `Settings` property.
12. Retirement of `SemanticKernel` embedding type. All embedding requests now flow through the Gateway API.

Gatekeeper API changes:
1. All Gatekeeper API endpoints have been moved to the `/instances/{instanceId}` path. For example, the `/status` endpoint is now `/instances/{instanceId}/status`.
2. The `/orchestration/*` endpoints have been moved to `/instances/{instanceId}/completions/*`.

Orchestration API changes:
1. All Gatekeeper API endpoints have been moved to the `/instances/{instanceId}` path. For example, the `/status` endpoint is now `/instances/{instanceId}/status`.
2. The `/orchestration/*` endpoints have been moved to `/instances/{instanceId}/completions/*`.
=======
### New APIs

**Gateway Adapter API** - requires the following configuration settings:

- `FoundationaLLM:APIs:GatewayAdapterAPI:APIUrl`
- `FoundationaLLM:APIs:GatewayAdapterAPI:APIKey` (mapped to the `foundationallm-apis-gatewayadapterapi-apikey` secret)
- `FoundationaLLM:APIs:GatewayAdapterAPI:APIAppInsightsConnectionString` (mapped to the `foundationallm-app-insights-connection-string` secret)
- 
**State API** - requires the following configuration settings:

- `FoundationaLLM:APIs:StateAPI:APIUrl`
- `FoundationaLLM:APIs:StateAPI:APIKey` (mapped to the `foundationallm-apis-stateapi-apikey` secret)
- `FoundationaLLM:APIs:StateAPI:APIAppInsightsConnectionString` (mapped to the `foundationallm-app-insights-connection-string` secret)

> [!NOTE]
> These new APIs will be converted to use the new `APIEndpoint` artifacts.

### Changes in app registration names

API Name | Entra ID app registration name | Application ID URI | Scope name
--- | --- | --- | ---
Core API | `FoundationaLLM-Core-API` | `api://FoundationaLLM-Core` | `Data.Read`
Management API | `FoundationaLLM-Management-API` | `api://FoundationaLLM-Management` | `Data.Manage`
Authorization API | `FoundationaLLM-Authorization-API` | `api://FoundationaLLM-Authorization` | `Authorization.Manage`
User Portal | `FoundationaLLM-Core-Portal` | `api://FoundationaLLM-Core-Portal` | N/A
Management Portal | `FoundationaLLM-Management-Portal` | `api://FoundationaLLM-Management-Portal` | N/A

### Changes in app configuration settings

The `FoundationaLLM:APIs` and `FoundationaLLM:ExternalAPIs` configuration namespaces have been replaced with the `FoundationaLLM:APIEndpoints` configuration namespace.

> [!IMPORTANT]
> All existing API registrations need to be updated to reflect these changes. The only two settings that will exist under `FoundationaLLM:APIEndpoints` are `APIKey` (for those API enpoints which use API key authentication) and `AppInsightsConnectionString`, all the other settings are now part of the `APIEndpoint` artifact managed by the `FoundationaLLM.Configuration` resource provider.
> This is an example for `CoreAPI`:
> - `FoundationaLLM:APIEndpoints:CoreAPI:APIKey`
> - `FoundationaLLM:APIEndpoints:CoreAPI:AppInsightsConnectionString`

The `FoundationaLLM:AzureAIStudio` configuration namespace expects an `APIEndpointConfigurationName` property instead of `BaseUrl`.

A new configuration setting named `FoundationaLLM:Instance:SecurityGroupRetrievalStrategy` with a value of `IdentityManagementService` must exist in the app configuration. It will be added by default in new deployments.

Two new configuration settings required by the new `FoundationaLLM.AzureOpenAI` resource provider:
- `FoundationaLLM:ResourceProviders:AzureOpenAI:Storage:AuthenticationType`
- `FoundationaLLM:ResourceProviders:AzureOpenAI:Storage:AccountName`

## Pre-0.8.0

1. Vectorization resource stores use a unique collection name, `Resources`. They also add a new top-level property named `DefaultResourceName`.
2. The items in the `index_references` collection have a property incorrectly named `type` which was renamed to `index_entry_id`.
3. New gateway API, requires the following app configurations:
   - `FoundationaLLM:APIs:GatewayAPI:APIUrl`
   - `FoundationaLLM:APIs:GatewayAPI:APIKey` (with secret `foundationallm-apis-gatewayapi-apikey`)
   - `FoundationaLLM:APIs:GatewayAPI:AppInsightsConnectionString` (with secret `foundationallm-app-insights-connection-string`)
   - `FoundationaLLM:Gateway:AzureOpenAIAccounts`
4. The `AgentFactory` and `AgentFactoryAPI` classes have been renamed to `Orchestration` and `OrchestrationAPI`, respectively. The following App Config settings need to be replaced in existing environments:

    - `FoundationaLLM:APIs:AgentFactoryAPI:APIKey` -> `FoundationaLLM:APIs:OrchestrationAPI:APIKey`
    - `FoundationaLLM:APIs:AgentFactoryAPI:APIUrl` -> `FoundationaLLM:APIs:OrchestrationAPI:APIUrl`
    - `FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString` -> `FoundationaLLM:APIs:OrchestrationAPI:AppInsightsConnectionString`
    - `FoundationaLLM:Events:AzureEventGridEventService:Profiles:AgentFactoryAPI` -> `FoundationaLLM:Events:AzureEventGridEventService:Profiles:OrchestrationAPI`
    - `FoundationaLLM:APIs:AgentFactoryAPI:ForceHttpsRedirection` -? `FoundationaLLM:APIs:OrchestrationAPI:ForceHttpsRedirection`

5. The following Key Vault secrets need to be replaced in existing environments:

    - `foundationallm-apis-agentfactoryapi-apikey` -> `foundationallm-apis-orchestrationapi-apikey`

    There is an upgrade script available that migrates these settings and secrets to their new names.

6. The following App Config settings are no longer needed:

    - `FoundationaLLM:Vectorization:Queues:Embed:ConnectionString`
    - `FoundationaLLM:Vectorization:Queues:Extract:ConnectionString`
    - `FoundationaLLM:Vectorization:Queues:Index:ConnectionString`
    - `FoundationaLLM:Vectorization:Queues:Partition:ConnectionString`

7. The following Key Vault secret is no longer needed:

    - `foundationallm-vectorization-queues-connectionstring`

8. The following App Config settings need to be added as key-values:

   - `FoundationaLLM:Vectorization:Queues:Embed:AccountName` (set to the name of the storage account that contains the vectorization queues - e.g., `stejahszxcubrpi`)
   - `FoundationaLLM:Vectorization:Queues:Extract:AccountName` (set to the name of the storage account that contains the vectorization queues - e.g., `stejahszxcubrpi`)
   - `FoundationaLLM:Vectorization:Queues:Index:AccountName` (set to the name of the storage account that contains the vectorization queues - e.g., `stejahszxcubrpi`)
   - `FoundationaLLM:Vectorization:Queues:Partition:AccountName` (set to the name of the storage account that contains the vectorization queues - e.g., `stejahszxcubrpi`)

9. The value for the App Config setting `FoundationaLLM:Events:AzureEventGridEventService:Profiles:OrchestrationAPI` should be set in the following format:

    ```json
    {
        "EventProcessingCycleSeconds": 20,
        "Topics": [
            {
                "Name": "storage",
                "SubscriptionPrefix": "orch",
                "EventTypeProfiles": [
                    {
                        "EventType": "Microsoft.Storage.BlobCreated",
                        "EventSets": [
                            {
                                "Namespace": "ResourceProvider.FoundationaLLM.Agent",
                                "Source": "/subscriptions/0a03d4f9-c6e4-4ee1-87fb-e2005d2c213d/resourceGroups/rg-fllm-aca-050/providers/Microsoft.Storage/storageAccounts/stejahszxcubrpi",
                                "SubjectPrefix": "/blobServices/default/containers/resource-provider/blobs/FoundationaLLM.Agent"
                            },
                            {
                                "Namespace": "ResourceProvider.FoundationaLLM.Vectorization",
                                "Source": "/subscriptions/0a03d4f9-c6e4-4ee1-87fb-e2005d2c213d/resourceGroups/rg-fllm-aca-050/providers/Microsoft.Storage/storageAccounts/stejahszxcubrpi",
                                "SubjectPrefix": "/blobServices/default/containers/resource-provider/blobs/FoundationaLLM.Vectorization"
                            },
                            {
                                "Namespace": "ResourceProvider.FoundationaLLM.Prompt",
                                "Source": "/subscriptions/0a03d4f9-c6e4-4ee1-87fb-e2005d2c213d/resourceGroups/rg-fllm-aca-050/providers/Microsoft.Storage/storageAccounts/stejahszxcubrpi",
                                "SubjectPrefix": "/blobServices/default/containers/resource-provider/blobs/FoundationaLLM.Prompt"
                            }
                        ]
                    }
                ]
            }
        ]
    }
    ```

10. Vectorization text embedding profiles require only two items in the `configuration_references` section: `DeploymentName` and `Endpoint`. Optionally, a `deployment_name` entry can be specified in the `settings` section to override the default value in `configuration_references.Endpoint`. Here is an example of the updated format for a text embedding profile:

    ```json
    {
        "type": "text-embedding-profile",
        "name": "AzureOpenAI_Embedding_BaselineGlobalMacro",
        "object_id": "/instances/a6221c30-0bf2-4003-adb8-d3086bb2ad49/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/AzureOpenAI_Embedding_BaselineGlobalMacro",
        "display_name": null,
        "description": null,
        "text_embedding": "SemanticKernelTextEmbedding",
        "settings": {
            "deployment_name": "embeddings-3-large"
        },
        "configuration_references": {
            "DeploymentName": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName",
            "Endpoint": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint"
        },
        "created_on": "0001-01-01T00:00:00+00:00",
        "updated_on": "0001-01-01T00:00:00+00:00",
        "created_by": null,
        "updated_by": null,
        "deleted": false
    }
    ```

11. External orchestration APIs must be configured using the `FoundationaLLM:ExternalAPIs` configuration namespace. For example, the `BaselineTradingGlobalMacro` external API has the following configurations:
    - `FoundationaLLM:ExternalAPIs:BaselineTradingGlobalMacro:APIUrl`
    - `FoundationaLLM:ExternalAPIs:BaselineTradingGlobalMacro:APIKey`

> [!NOTE]
> These entries do not need to be created as part of the deployment process.

12. App Config key namespace that was previously `FoundationaLLM:Vectorization:ContentSources:*` has been moved to `FoundationaLLM:DataSources:*`. All existing keys need to be moved to the new namespace.
13. New app config entries required:

    - `FoundationaLLM:Attachment:ResourceProviderService:Storage:AuthenticationType`
    - `FoundationaLLM:Attachment:ResourceProviderService:Storage:AccountName`

14. App Config key namespace that was previously `FoundationaLLM:Vectorization:ContentSources:*` has been moved to `FoundationaLLM:DataSources:*`. All existing keys need to be moved to the new namespace.

15. The following App Config setting needs to be added/updated as key-values:

   - Add `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableAzureContentSafetyPromptShield`
   - Add `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableLakeraGuard`
   - Add `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableEnkryptGuardrails`
   - Rename `FoundationaLLM:AzureContentSafety:APIKey` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:APIKey`
   - Rename `FoundationaLLM:AzureContentSafety:APIUrl` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:APIUrl`
   - Rename `FoundationaLLM:AzureContentSafety:HateSeverity` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:HateSeverity`
   - Rename `FoundationaLLM:AzureContentSafety:SelfHarmSeverity` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:SelfHarmSeverity`
   - Rename `FoundationaLLM:AzureContentSafety:SexualSeverity` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:SexualSeverity`
   - Rename `FoundationaLLM:AzureContentSafety:ViolenceSeverity` in `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:ViolenceSeverity`
   - Add `FoundationaLLM:APIs:Gatekeeper:LakeraGuard:APIKey`
   - Add `FoundationaLLM:APIs:Gatekeeper:LakeraGuard:APIUrl`
   - Add `FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails:APIKey`
   - Add `FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails:APIUrl`

16. The following Key Vault secret is needed:

    - `lakera-guard-api-key`
    - `enkrypt-guardrails-apikey`



