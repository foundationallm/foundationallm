# Core API

The Core API serves as the entry point for user requests to FoundationaLLM's underlying engine. While clients primarily interact with the Core API through the Chat UI, the Core API exposes some convenient interfaces for developers.

## Sessionless Completion

The sessionless completion endpoint enables users to query agents without first creating a chat session.

**Endpoint:** `[DEPLOYMENT URL]/core/orchestration/completion?api-version=1.0`

>**Note:** For AKS deployments, `[DEPLOYMENT URL]` is the same as the cluster FQDN, while for ACA deployments, the Core API endpoint can be found by navigating to the `[DEPLOYMENT PREFIX]coreca` Container App in the Azure Portal.

**Sample Request:**

```json
{
    "user_prompt": "What are your capabilities?",
    "settings": {
        "agent_name": "internal-context",
        "model_settings": {
            "temperature": 0.4,
            "deployment_name": "completions"
        }
    }
}
```

> [!NOTE]
> The `settings` object is optional and can be used to specify the agent name and model settings. If the `settings` object is not provided, the Core API will use the default agent and its model settings.

**Payload Headers:**

| Header | Value | Details |
| ------ | ----- | ------- |
| `Authorization` | `Bearer [ENTRA ID BEARER TOKEN]` | Valid token from Entra ID |
| `Content-Type` | `application/json` | |

**Sample Response:**

```json
{
    "text": "FoundationaLLM is a copilot platform that simplifies and streamlines building knowledge management and analytic agents over the data sources present across your enterprise. It provides integration with enterprise data sources used by agents for in-context learning, fine-grain security controls over data used by agents, and pre/post completion filters that guard against attack. The solution is scalable and load balances across multiple endpoints. It is also extensible to new data sources, new LLM orchestrators, and LLMs. You can learn more about FoundationaLLM at https://foundationallm.ai."
}
```

**Sample Postman Request:** `/orchestration/completion/Requests a completion from the downstream APIs.`