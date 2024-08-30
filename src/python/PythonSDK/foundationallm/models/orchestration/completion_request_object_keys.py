from enum import Enum

class CompletionRequestObjectKeys(str, Enum):
   """Enumerator of the Completion Request Object Keys."""
   OPENAI_ASSISTANT_ID = "OpenAI.AssistantId"
   OPENAI_THREAD_ID = "OpenAI.AssistantThreadId"
   GATEWAY_API_ENDPOINT_CONFIGURATION = "GatewayAPIEndpointConfiguration"
