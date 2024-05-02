from abc import abstractmethod
from typing import List, Optional 

from langchain_core.language_models import BaseLanguageModel
from langchain_openai import AzureChatOpenAI, AzureOpenAI, ChatOpenAI, OpenAI
from foundationallm.config.configuration import Configuration
from foundationallm.models.language_models import LanguageModelProvider
from foundationallm.models.orchestration import CompletionResponse, MessageHistoryItem, OrchestrationSettings

class AgentBase():
    """Base class for Agents"""

    @abstractmethod
    def invoke(self, prompt: str) -> CompletionResponse:
        """
        Execute the agent's invoke method.
        
        Parameters
        ----------
        prompt : str
            The prompt for which a completion is begin generated.

        Returns
        -------
        CompletionResponse
            Returns a response containing the completion plus token usage and cost details.
        """

    def _build_conversation_history(messages:List[MessageHistoryItem]=None, message_count:int=None) -> str:
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

    def _get_completion_model(
            self,
            config: Configuration,
            agent_orchestration_settings: OrchestrationSettings,
            override_settings: Optional[OrchestrationSettings] = None) -> BaseLanguageModel:
        """
        Create an API endpoint connector chat completion model.

        Parameters
        ----------
        config : Configuration
            Application configuration class for retrieving configuration settings.
        agent_orchestration_settings : OrchestrationSettings
            The settings for the completion request configured on the agent.
        override_settings : Optional[OrchestrationSettings]
            The settings to override the agent's settings.

        Returns
        -------
        BaseLanguageModel
            Returns an API connector for a chat completion model.
        """
        if agent_orchestration_settings is None:
            raise ValueError("Orchestration settings are required for completion requests.")

        if agent_orchestration_settings.endpoint_configuration is None:
            raise ValueError("Endpoint configuration is required for completion requests.")
        
        # Get endpoint settings (pull from Application Configuration using keys)
        endpoint = config.get_value(agent_orchestration_settings.endpoint_configuration.get('endpoint'))
        if endpoint is None:
            raise ValueError("Endpoint is required for completion requests.")

        api_key = config.get_value(agent_orchestration_settings.endpoint_configuration.get('api_key'))
        if api_key is None:
            raise ValueError("API Key is required for completion requests.")

        api_version = config.get_value(agent_orchestration_settings.endpoint_configuration.get('api_version'))
        if api_version is None:
            raise ValueError("API Version is required for completion requests.")

        provider = LanguageModelProvider.MICROSOFT
        if agent_orchestration_settings.endpoint_configuration.get('provider') is not None:
            try:
                provider = config.get_value(agent_orchestration_settings.endpoint_configuration.get('provider'))
            except:
                provider = LanguageModelProvider.MICROSOFT

        operation_type = "chat"
        if agent_orchestration_settings.endpoint_configuration.get('operation_type') is not None:
            try:
                operation_type = config.get_value(agent_orchestration_settings.endpoint_configuration.get('operation_type'))
            except:
                operation_type = "chat"

        api = None

        if provider == LanguageModelProvider.MICROSOFT:
            # Get Azure OpenAI Chat model settings
            deployment_name = (override_settings.model_parameters.get('deployment_name')
                                if override_settings is not None and override_settings.model_parameters is not None and override_settings.model_parameters.get('deployment_name') is not None
                                else agent_orchestration_settings.model_parameters.get('deployment_name'))
            if deployment_name is None:
                raise ValueError("Deployment name is required for Azure OpenAI completion requests.")

            api = (
                AzureChatOpenAI(azure_endpoint=endpoint, api_key=api_key, api_version=api_version, azure_deployment=deployment_name)
                if operation_type == 'chat'
                else AzureOpenAI(azure_endpoint=endpoint, api_key=api_key, api_version=api_version, azure_deployment=deployment_name)
            )
        else:
            api = (
                ChatOpenAI(base_url=endpoint, api_key=api_key)
                if operation_type == 'chat'
                else OpenAI(base_url=endpoint, api_key=api_key)
            )

        # Set model parameters from agent orchestration settings.
        for key, value in agent_orchestration_settings.model_parameters.items():
            if hasattr(api, key):
                setattr(api, key, value)

        # Override model parameters from completion request settings, if any exist.
        if override_settings is not None and override_settings.model_parameters is not None:            
            for key, value in override_settings.model_parameters.items():
                if hasattr(api, key):
                    setattr(api, key, value)

        return api
