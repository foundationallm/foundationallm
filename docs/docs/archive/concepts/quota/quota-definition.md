# Quota Definition

The quota definitions are stored in the main FoundationaLLM storage account, in the `quota` container in a file named `quota-store.json`. If the file does not exist, the file is automatically created. The file contains a list of quota definitions with the following structure:

```json
{
	"name": "CoreAPICompletionsUPNRawRequestRateLimit",
	"description": "Defines a per UPN raw request rate limit on the Core API Completions controller.",
	"context": "CoreAPI:Completions",
	"type": "RawRequestRateLimit",
	"metric_partition": "UserPrincipalName",
	"metric_limit": 120,
	"metric_window_seconds": 60,
	"lockout_duration_seconds": 60,
	"distributed_enforcement": false
}
```

The example above defines a quota for the Core API Completions controller that limits the number of raw API requests per user principal name (UPN) to 120 requests in a 60-second window. If the user principal exceeds this limit, they are locked out for 60 seconds before the quota is reset.

>[!NOTE]
> A user principal name corresponds to either a user or an agent access token. If the same agent access token is shared by multiple applications calling the Core API, the quota is enforced across all applications using that token.

>[!NOTE]
> FoundationaLLM using a smoothing time window of 20 seconds for the quota enforcement. This means that the quota is enforced every 20 seconds, and the number of requests is averaged over that time window. This helps to smooth out spikes in traffic and provides a more consistent experience for users. In the example above, this means that the specified user principal name can make up to 40 requests every 20 seconds. The first time the user principal name exceeds the limit, they are locked out for 60 seconds. After the lockout period, the user principal name can make requests again, and the quota is reset.
>
> It is recommended to set the `metric_window_seconds` to a multiple of 20 seconds to align with the smoothing time window. The standard practice is to set the `metric_window_seconds` to 60 seconds, which makes it easy to understand for the consumers of the API. Also, it is recommended to set the `metric_limit` to a multiple of `metric_window_seconds` divided by 20 seconds, so that you avoid misinterpretation of the quota limits due to roundings. In the example above, since `metric_window_seconds` is set to 60 seconds, `metric_limit` should be set to a multiple of 3 (60 seconds / 20 seconds). Therefore, the value of `metric_limit` is set to 120. If `metric_limit` were set to 3, the user principal name would be allowed to make only 1 request every 20 seconds. It is also important to note that setting `metric_limit` to a value below `metric_windows_seconds` divided by 20 seconds would result in the user principal name being locked out immediately after the first request, which is not a desirable behavior.

The following table provides details about the quota definition properties:

Name | Description | Notes
--- | --- | ---
`name` | The name of the quota definition. |
`description` | A description of the quota definition. |
`context` | The context of the quota definition. | The format of the context is `<service_name>:<controller_name>` or `<service_name>:<controller_name>:<agent_name>`. Currently the following contexts can be used: `CoreAPI:<controller_name>`, `CoreAPI:Completions:<agent_name>` where `<agent_name>` must be a valid agent name. For more details on the available controllers and their contexts, see [API Raw Request Rate](api-raw-request-rate.md).
`type` | The type of the quota enforcement applied. | The following types are supported: `RawRequestRateLimit` and `AgentRequestRateLimit`. `RawRequestRateLimt` defines the quota metric to be raw API requests and requires a context of `<service_name>:<controller_name>`. `AgentRequestRateLimit` defines the quota metric to be agent completion requests and requires a context of `<service_name>:<controller_name>:<agent_name>`. For more details, see [API Raw Request Rate](api-raw-request-rate.md) or [Agent Request Rate](agent-request-rate.md).
`metric_partition` | The metric partition used to enforce the quota. | The following partitions are supported: `None` (the metric is not partitioned) `UserPrincipalName` (the metric is partitioned by user principal name) and `UserIdentifier` (the metric is partitioned by user identifier). In the example above, the metric is partitioned by user principal name, meaning that each user principal name has its own quota limit. If `metric_partition` were set to `None`, the quota would be enforced globally across all calls, regardless of user principal names, meaning that the limit would apply to all users collectively. If `metric_partition` were set to `UserIdentifier`, the quota would be enforced per user identifier, which is a unique identifier for each user. In most cases, the relationship between user principal name and user identifier is one-to-one, so the quota would be effectively the same as if `metric_partition` were set to `UserPrincipalName`. In these cases, the recommended value for `metric_partition` is `UserPrincipalName`. However, in some more advanced scenarios (e.g., when calls are made by another service that authenticates against the Core API using a managed identity), the user principal name may not be available, and the quota should be enforced by user identifier. In these cases, the recommended value for `metric_partition` is `UserIdentifier`.
`metric_limit` | The limit of the metric. | The limit is enforced over the `metric_window_seconds`. In the example above, a maximum number of 120 raw API requests are allowed per user principal name in a 60-second window.
`metric_window_seconds` | The time window in seconds over which the limit is enforced. | In the example above, a maximum number of 120 raw API requests are allowed per user principal name in a 60-second window.
`lockout_duration_seconds` | The duration in seconds for which the caller is locked out after exceeding the quota. | The lockout duration is applied after the user exceeds the quota limit. The user is locked out for the specified duration before the quota is reset.
`distributed_enforcement` | Indicates whether the quota is enforced across multiple instances of the same API. | If `true`, the quota is enforced across all the instances of the API. If `false`, the quota is enforced individually on every single instance. In the example above, if the platform in running 5 instances of the Core API, one user principal name can make up to 5 x 120 = 600 raw API requests in a 60-second window. If `distributed_enforcement` were set to `true`, the user principal name would be allowed to make only 120 raw API requests in a 60-second window across all instances of the Core API.

