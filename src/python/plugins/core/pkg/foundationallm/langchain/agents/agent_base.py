from abc import abstractmethod
from foundationallm.config import Configuration, UserIdentity
from foundationallm.operations import OperationsManager
from foundationallm.models.orchestration import (
    CompletionRequestBase,
    CompletionResponse
)
from foundationallm.plugins import PluginManager
from foundationallm.telemetry import Telemetry

class AgentBase():
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
