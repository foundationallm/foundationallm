from abc import abstractmethod
from typing import List
from azure.identity import DefaultAzureCredential, get_bearer_token_provider
from langchain_core.language_models import BaseLanguageModel
from langchain_core.messages import BaseMessage, AIMessage, HumanMessage
from openai import AsyncAzureOpenAI as async_aoi
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.operations import OperationsManager
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.orchestration import (
    CompletionRequestBase,
    CompletionResponse
)
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.plugins import PluginManager
from foundationallm.telemetry import Telemetry
from foundationallm.utils.object_utils import ObjectUtils

class LangChainAgentBase():
    """
    Implements the base functionality for a LangChain agent.
    """
    def __init__(self, instance_id: str, user_identity: UserIdentity, config: Configuration, plugin_manager: PluginManager, operations_manager: OperationsManager):
        """
        Initializes a knowledge management agent.

        Parameters
        ----------
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.instance_id = instance_id
        self.user_identity = user_identity
        self.config = config
        self.plugin_manager = plugin_manager
        self.ai_model = None
        self.api_endpoint = None
        self.prompt = ''
        self.full_prompt = ''
        self.has_indexing_profiles = False
        self.has_retriever = False
        self.operations_manager = operations_manager

        self.tracer = Telemetry.get_tracer('langchain-agent-base')

    @abstractmethod
    async def invoke_async(self, request: CompletionRequestBase) -> CompletionResponse:
        """
        Gets the completion for the request using an async request.

        Parameters
        ----------
        request : CompletionRequestBase
            The completion request to execute.

        Returns
        -------
        CompletionResponse
            Returns a completion response.
        """
        raise NotImplementedError()

    @abstractmethod
    def _validate_request(self, request: CompletionRequestBase):
        """
        Validates that the completion request contains all required properties.

        Parameters
        ----------
        request : CompletionRequestBase
            The completion request to validate.
        """
        raise NotImplementedError()

       
    def _get_api_endpoint_from_object_id(self, api_endpoint_object_id: str, objects: dict) -> APIEndpointConfiguration:
        """
        Get the API endpoint from its object id.
        """
        api_endpoint: APIEndpointConfiguration = None

        if api_endpoint_object_id is None or api_endpoint_object_id == '':
            raise LangChainException("Invalid API endpoint object id.", 400)

        try:
            api_endpoint = APIEndpointConfiguration(**objects.get(api_endpoint_object_id))
        except Exception as e:
            raise LangChainException(f"The API endpoint object provided in the request.objects dictionary is invalid. {str(e)}", 400)

        if api_endpoint is None:
            raise LangChainException("The API endpoint object is missing in the request.objects dictionary.", 400)

        return api_endpoint

    def _build_conversation_history(self, messages:List[MessageHistoryItem]=None, message_count:int=None) -> str:
        """
        Builds a chat history string from a list of MessageHistoryItem objects to
        be added to the prompt for the completion request.

        Parameters
        ----------
        messages : List[MessageHistoryItem]
            The list of messages from which to build the chat history.
        message_count : int
            The number of messages to include in the chat history.
        """
        if messages is None or len(messages)==0:
            return ""
        if message_count is not None:
            messages = messages[-message_count:]
        chat_history = "Chat History:\n"
        for msg in messages:
            chat_history += msg.sender + ": " + msg.text + "\n"
        chat_history += "\n\n"
        return chat_history

    def _build_conversation_history_message_list(self, messages:List[MessageHistoryItem]=None, message_count:int=None) -> List[BaseMessage]:
        """
        Builds a LangChain Message chat history list from a list of MessageHistoryItem objects to
        be added to the prompt template for the completion request.

        Parameters
        ----------
        messages : List[MessageHistoryItem]
            The list of messages from which to build the chat history.
        message_count : int
            The number of messages to include in the chat history.
        """
        if messages is None or len(messages)==0:
            return []
        if message_count is not None:
            messages = messages[-message_count:]
        history = []
        for msg in messages:
            # sender can be User (maps to HumanMessage) or Agent (maps to AIMessage)
            if msg.sender == "User":
                history.append(HumanMessage(content=msg.text))
            else:
                history.append(AIMessage(content=msg.text))
        return history

    def _record_full_prompt(self, prompt: str) -> str:
        """
        Records the full prompt for the completion request.

        Parameters
        ----------
        prompt : str
            The prompt that is populated with context.

        Returns
        -------
        str
            Returns the full prompt.
        """
        self.full_prompt = prompt
        return prompt

    def _get_image_gen_language_model(self, api_endpoint_object_id, objects: dict) -> BaseLanguageModel:        
        api_endpoint = ObjectUtils.get_object_by_id(api_endpoint_object_id, objects, APIEndpointConfiguration)        
        scope = api_endpoint.authentication_parameters.get('scope', 'https://cognitiveservices.azure.com/.default')
        # Set up a Azure AD token provider.
        token_provider = get_bearer_token_provider(
            DefaultAzureCredential(exclude_environment_credential=True),
            scope
        )

        return async_aoi(
            azure_endpoint=api_endpoint.url,
            api_version=api_endpoint.api_version,
            azure_ad_token_provider=token_provider,
        )
