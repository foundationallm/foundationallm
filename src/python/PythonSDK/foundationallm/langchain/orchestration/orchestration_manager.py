from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.agents import AgentFactory, LangChainAgentBase
from foundationallm.models.orchestration import (
    CompletionRequestBase,
    CompletionResponse
)

class OrchestrationManager:
    """Client that acts as the entry point for interacting with the FoundationaLLM Python SDK."""

    def __init__(self,
        completion_request: CompletionRequestBase,
        instance_id: str,
        user_identity: UserIdentity,
        configuration: Configuration):
        """
        Initializes an instance of the OrchestrationManager.
        
        Parameters
        ----------
        completion_request : CompletionRequest
            The CompletionRequest is the metadata object containing the details needed for
            the OrchestrationManager to assemble an agent.
        instance_id : str
            The unique identifier of the FoundationaLLM instance.
        user_identity : UserIdentity
            The user context under which to execution completion requests.
        """        
        self.completion_request = completion_request
        self.instance_id = instance_id
        self.user_identity = user_identity
        self.agent = self.__create_agent(config=configuration)        

    def __create_agent(self, config: Configuration) -> LangChainAgentBase:
        """Creates an agent for executing completion requests."""        
        agent_factory = AgentFactory(
            instance_id = self.instance_id,
            user_identity = self.user_identity,
            config=config)        
        return agent_factory.get_agent(self.completion_request.agent.type)

    def invoke(self, request: CompletionRequestBase) -> CompletionResponse:
        """
        Executes a completion request against the LanguageModel using 
        the LangChain agent assembled by the OrchestrationManager.
        
        Parameters
        ----------
        prompt : str
            The prompt for which a completion is being generated.
            
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
        prompt : str
            The prompt for which a completion is being generated.
            
        Returns
        -------
        CompletionResponse
            Object containing the completion response and token usage details.
        """        
        completion_response = await self.agent.ainvoke(request)
        return completion_response
