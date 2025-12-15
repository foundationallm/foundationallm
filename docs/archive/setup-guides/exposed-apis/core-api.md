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
        "model_parameters": {
            "temperature": 0.4,
            "deployment_name": "completions",
            "top_k": 5,
            "top_p": 0.9,
            "do_sample": true,
            "max_new_tokens": 100,
            "return_full_text": true,
            "ignore_eos": true
        },
        "agent_parameters": {
            "index_filter_expression": "search.ismatch('FoundationaLLM', 'Text')",
            "index_top_n": 5
        }
    }
}
```

> [!NOTE]
> The `settings` object provides to override various parameters at runtime, and is optional. Within `settings` both `model_parameters` and `settings.agent_parameters` (along with their members) are optional. If not provided, the Core API will use the default model and agent settings.

**model_parameters:**

| Name | Type | Description |
| ---- | ---- | ----------- |
| `temperature` | `float` | Controls randomness. Lowering the temperature means that the model will produce more repetitive and deterministic responses. Increasing the temperature will result in more unexpected or creative responses. Try adjusting temperature or Top P but not both. This value should be a float between 0.0 and 1.0. |
| `deployment_name` | `string` | The deployment name for the language model. |
| `top_k` | `int` | The number of highest probability vocabulary tokens to keep for top-k-filtering. Default value is null, which disables top-k-filtering. |
| `top_p` | `float` | The cumulative probability of parameter highest probability vocabulary tokens to keep for nucleus sampling. Top P (or Top Probabilities) is imilar to temperature, this controls randomness but uses a different method. Lowering Top P will narrow the model’s token selection to likelier tokens. Increasing Top P will let the model choose from tokens with both high and low likelihood. Try adjusting temperature or Top P but not both. |
| `do_sample` | `bool` | Whether or not to use sampling; use greedy decoding otherwise. |
| `max_new_tokens` | `int` | Sets a limit on the number of tokens per model response. The API supports a maximum of number of tokens (depending on the deployment) shared between the prompt (including system message, examples, message history, and user query) and the model's response. One token is roughly 4 characters for typical English text. |
| `return_full_text` | `bool` | Whether or not to return the full text (prompt + response) or only the generated part (response). Default value is false. |
| `ignore_eos` | `bool` | Whether to ignore the End of Sequence(EOS) token and continue generating tokens after the EOS token is generated. Defaults to False. |

**agent_parameters:**

| Name | Type | Description |
| ---- | ---- | ----------- |
| `index_filter_expression` | `string` | This value should be a string representing the search filter expression to limit documents to be searched by the index retriever |
| `index_top_n` | `int` | Controls the number of search results to return from an index for prompt augmentation. |

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