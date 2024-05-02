#from langchain_core.language_models import BaseLanguageModel
from foundationallm.config import Configuration, Context
from foundationallm.models.orchestration import CompletionRequestBase
from foundationallm.resources import ResourceProvider
from foundationallm.langchain.agents import (
    AgentBase,
    KnowledgeManagementAgent
)

class AgentFactory:
    """
    Factory to determine which agent to use.
    """

    def __init__(
            self,
            completion_request: CompletionRequestBase,
            config: Configuration,
            context: Context,
            resource_provider: ResourceProvider=None
        ):
        """
        Initializes an Orchestration for selecting which agent to use for completion.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.completion_request = completion_request
        self.config = config
        self.context = context
        self.resource_provider = resource_provider or ResourceProvider(config=config)

    def get_agent(self) -> AgentBase:
        """
        Retrieves an agent of the the requested type.
        
        Returns
        -------
        AgentBase
            Returns an agent of the requested type.
        """
        agent = self.completion_request.agent
        if agent is None:
            raise ValueError("Agent not constructed. Cannot access an object of 'NoneType'.")
        
        match agent.type:
            case 'knowledge-management' | 'internal-context':
                return KnowledgeManagementAgent(
                    self.completion_request,
                    config=self.config,
                    resource_provider=self.resource_provider
                )
            case _:
                raise ValueError(f'No agent found for the specified agent type: {agent.type}.')
