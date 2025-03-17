# Quota Definition

The quota definitions are stored in the main FoundationaLLM storage account, in the `quota` container in a file named `quota-store.json`. If the file does not exist, the file is automatically created. The file contains a list of quota definitions with the following structure:

```json
{
	"name": "TestAPI01CompletionsUPNRawRequestRateLimit",
	"description": "Defines a per UPN raw request rate limit on the TestAPI01 Completions controller.",
	"context": "TestAPI01:Completions",
	"type": "RawRequestRateLimit",
	"metric_partition": "UserPrincipalName",
	"metric_limit": 120,
	"metric_window_seconds": 60,
	"lockout_duration_seconds": 60,
	"distributed_enforcement": false
}
```

The following table provides details about the quota definition properties:

Name | Description | Notes
--- | --- | ---
`name` | The name of the quota definition. |
`description` | A description of the quota definition. |
`context` | The context of the quota definition. | The format of the context is `<service_name>:<controller_name>` or `<service_name>:<controller_name>:<agent_name>`. Currently the following contexts can be used: `CoreAPI:Completions`, `CoreAPI:Completions:<agent_name>` where `<agent_name>` must be a valid agent name.
`type` | The type of the quota enforcement applied. | The following types are supported: `RawRequestRateLimit` and `AgentRequestRateLimit`. `RawRequestRateLimt` defines the quota metric to be raw API requests and requires a context of `<service_name>:<controller_name>`. `AgentRequestRateLimit` defines the quota metric to be agent completion requests and requires a context of `<service_name>:<controller_name>:<agent_name>`.
`metric_partition` | The metric partition used to enforce the quota. | The following partitions are supported: `None` (the metric is not partitioned) `UserPrincipalName` (the metric is partitioned by user principal name) and `UserIdentifier` (the metric is partitioned by user identifier).
`metric_limit` | The limit of the metric. | The limit is enforced over the `metric_window_seconds`. In the example above, a maximum number of 120 raw API requests are allowed per user principal name in a 60-second window.
`metric_window_seconds` | The time window in seconds over which the limit is enforced. | In the example above, a maximum number of 120 raw API requests are allowed per user principal name in a 60-second window.
`lockout_duration_seconds` | The duration in seconds for which the caller is locked out after exceeding the quota. | The lockout duration is applied after the user exceeds the quota limit. The user is locked out for the specified duration before the quota is reset.
`distributed_enforcement` | Indicates whether the quota is enforced across multiple instances of the same API. | If `true`, the quota is enforced across multiple instances. If `false`, the quota is enforced on a single instance. Currently, only `false` is supported.