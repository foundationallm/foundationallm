# API Raw Request Rate

The API raw request rate is a quota that limits the number of raw requests made to API controllers. Currently, only controllers from the Core API are supported.

This quota can be enforced per API controller for all users or per specific user principal name (UPN) or user identifier. In most cases, the relationship between user principal name and user identifier is one-to-one, so the quota would be effectively the same if `metric_partition` were set to `UserPrincipalName` or `UserIdentitifer`. In these cases, the recommended value for `metric_partition` is `UserPrincipalName`. However, in some more advanced scenarios (e.g., when calls are made by another service that authenticates against the Core API using a managed identity), the user principal name may not be available, and the quota should be enforced by user identifier. In these cases, the recommended value for `metric_partition` is `UserIdentifier`.

The following table lists the supported controllers and their contexts:

| Controller | Context |
| --- | --- |
| `Completions` | `CoreAPI:Completions` |
| `CompletionsStatus` | `CoreAPI:CompletionsStatus` * |
| `Branding` | `CoreAPI:Branding` |
| `Configuration` | `CoreAPI:Configuration` |
| `Files` | `CoreAPI:Files` |
| `OneDriveWorkSchool` | `CoreAPI:OneDriveWorkSchool` |
| `Sessions` | `CoreAPI:Sessions` |
| `UserProfiles` | `CoreAPI:UserProfiles` |
| `Status` | `CoreAPI:Status` |

* Before FoundationaLLM `v0.9.7-rc158`, the `CompletionsStatus` controller was not available, and the `Completions` controller was used to check the status of completions. Starting with FoundationaLLM `v0.9.7-rc158`, the `CompletionsStatus` controller is available, and it is used to check the status of completions. Certain client applications (the User Portal being the most notable example) require extensive use of the completions status endpoint, in order to provide a better user experience. Since the number of requests asking for the status of completions is significantly higher than the number of requests asking for completions defining an effective API raw request rate limit for the `Completions` controller is not practical prior to version `v0.9.7-rc158` and is not recommended. Starting with `v0.9.7-rc158`, separate raw request rate limits can be defined for the `Completions` and `CompletionsStatus` controllers, allowing for a more effective quota enforcement. When defining a quota for the `CompletionsStatus` controller, you need to take into account the value of the `FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CompletionResponsePollingIntervalMilliseconds` configuration setting, which defines the polling interval for the completions status endpoint in the User Portal. The default value is 100 milliseconds, meaning that the User Portal will poll the completions status endpoint every 100 milliseconds to check the status of the completion. This means that if a user has 2 active completions, they will make 20 requests per second to the completions status endpoint, which can quickly add up to a significant number of requests. Therefore, it is recommended to define a separate quota for the `CompletionsStatus` controller with a higher limit than the `Completions` controller.
