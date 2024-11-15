from enum import Enum

class CompletionRequestObjectKeys(str, Enum):
   """Enumerator of the Completion Request Object Keys."""
   OPENAI_ASSISTANT_ID = "OpenAI.Assistants.Assistant.Id"
   OPENAI_THREAD_ID = "OpenAI.Assistants.Thread.Id"
   GATEWAY_API_ENDPOINT_CONFIGURATION = "GatewayAPIEndpointConfiguration"
