# Prompt

The FoundationaLLM (FLLM) prompt resource encapsulates the system prompt of an agent. The system prompt describes the persona of the agent and any instructional guardrails used to generate the desired responses to user prompts. The prompt resource is used in the [Knowledge Management agent configuration](knowledge-management-agent.md).

## Prompt configuration

The structure of a prompt is the following:

```json
{
  "type": "multipart",
  "name": "<name>",
  "object_id": "/instances/<instance_id>/providers/FoundationaLLM.Prompt/prompts/<name>",
  "description": "<description>",
  "prefix": "<prompt_prefix>",
  "suffix": "<prompt_suffix>"
}
```

where:

- `<name>` is the name of the agent.
- `<instance_id>` is the instance ID of the deployment.
- `<description>` is the description of the prompt, describing the persona of the agent.
- `<prompt_prefix>` is the beginning of the prompt.
- `<prompt_suffix>` (optional) appended to the end of the prompt (after any prefix and context).

| Parameter | Description |
| --- | --- |
| `type` | The type - will be `multipart`. `multipart` prompts have a prefix and suffix. Support for `basic` prompts, which have no suffix, will be added in a future release. **`type` must be the first parameter in the request body.** |
| `name` | The name of the prompt. |
| `object_id` | The object ID of the prompt. Remove this key when creating a prompt, as it is automatically populated by the Management API. |
| `description` | The description of the prompt, ensure this description details the purpose or role of the prompt. |
| `prompt_prefix` | The beginning of the prompt. |
| `prompt_suffix` | Text appended to the ending of the prompt. |

## Managing prompts

This section describes how to manage knowledge management prompts using the Management API. `{{baseUrl}}` is the base URL of the Management API. `{{instanceId}}` is the unique identifier of the FLLM instance.

### Retrieve

```http
HTTP GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Prompt/prompts
```

### Create or update

```http
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Prompt/prompts/<name>
Content-Type: application/json

BODY
<prompt_configuration>
```

where `<prompt_configuration>` is the prompt configuration structure described above.

### Delete

```http
HTTP DELETE {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Prompt/prompts/<name>
```

> [!NOTE]
> The delete operation is a *logical delete*. To purge a Prompt, call the `/purge` endpoint after deleting the Prompt.

### Purge

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Prompt/prompts/<name>/purge
Content-Type: application/json

BODY
{}
```