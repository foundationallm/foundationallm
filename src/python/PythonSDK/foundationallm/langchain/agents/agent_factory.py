from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.agents import (
    LangChainAgentBase,
    LangChainKnowledgeManagementAgent,
    LangChainAudioClassifierAgent
)

class AgentFactory:
    """
    Factory to determine which agent to use.
    """

    def __init__(self, instance_id: str, user_identity: UserIdentity, config: Configuration):
        """
        Initializes an Orchestration for selecting which agent to use for completion.

        Parameters
        ----------
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.config = config
        self.instance_id = instance_id
        self.user_identity = user_identity

    def get_agent(self, agent_type: str) -> LangChainAgentBase:
        """
        Retrieves an agent of the the requested type.

        Parameters
        ----------
        agent_type : str
            The type type assign to the agent returned.

        Returns
        -------
        AgentBase
            Returns an agent of the requested type.
        """


        if agent_type is None:
            raise ValueError("Agent not constructed. Cannot access an object of 'NoneType'.")
        match agent_type:
            case 'knowledge-management':                
                return LangChainKnowledgeManagementAgent(
                        instance_id=self.instance_id,
                        user_identity=self.user_identity,
                        config=self.config)
            case 'audio-classification':
                return LangChainAudioClassifierAgent(
                    instance_id=self.instance_id,
                    user_identity=self.user_identity,
                    config=self.config)
            case _:
                raise ValueError(f'The agent type {agent_type} is not supported.')
