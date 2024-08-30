from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentFactory, LangChainAgentBase
from foundationallm.operations import OperationsManager
from foundationallm.models.orchestration import (
    CompletionRequestBase,
    CompletionResponse
)

class OrchestrationManager:
    """Client that acts as the entry point for interacting with the FoundationaLLM Python SDK."""

    def __init__(self,
        completion_request: CompletionRequestBase,
        configuration: Configuration,
        operations_manager: OperationsManager):
        """
        Initializes an instance of the OrchestrationManager.
        
        Parameters
        ----------
        completion_request : CompletionRequest
            The CompletionRequest is the metadata object containing the details needed for
            the OrchestrationManager to assemble an agent.
        configuration : Configuration
            The configuration object containing the details needed for the OrchestrationManager to assemble an agent.
        operations_manager : OperationsManager
            The operations manager object for allowing an agent to interact with the State API.
        """
        self.agent = self.__create_agent(
            completion_request = completion_request,
            config = configuration,
            operations_manager = operations_manager
        )

    def __create_agent(self, completion_request: CompletionRequestBase, config: Configuration, operations_manager: OperationsManager) -> LangChainAgentBase:
        """Creates an agent for executing completion requests."""
        return AgentFactory().get_agent(completion_request.agent.type, config, operations_manager)

    def invoke(self, request: CompletionRequestBase) -> CompletionResponse:
        """
        Executes a completion request against the LanguageModel using 
        the LangChain agent assembled by the OrchestrationManager.
        
        Parameters
        ----------
        request : CompletionRequestBase
            The completion request to execute.
            
        Returns
        -------
        CompletionResponse
            Object containing the completion response and token usage details.
        """
        return self.agent.invoke(request)

    async def ainvoke(self, request: CompletionRequestBase) -> CompletionResponse:
        """
        Executes an async completion request against the LanguageModel using 
        the LangChain agent assembled by the OrchestrationManager.
        
        Parameters
        ----------
        request : CompletionRequestBase
            The completion request to execute.
            
        Returns
        -------
        CompletionResponse
            Object containing the completion response and token usage details.
        """
        completion_response = await self.agent.ainvoke(request)
        return completion_response
