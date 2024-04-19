from typing import Optional
from pydantic import BaseModel

class OrchestrationSettings(BaseModel):
    """
        Represents settings for the orchestration.
        
        Parameters
        ----------
        agent_parameters : dict
            Parameters to set or override the behavior of the agent.
        endpoint_configuration : dict
            Options to set or override endpoint configuration (endpoint and key) used to access a
            language model by the orchestrator.
        model_parameters : dict
            Parameters to set or override the behavior of the language model as defined on the agent.
    """
    agent_parameters: Optional[dict] = None
    endpoint_configuration: Optional[dict] = None
    model_parameters: Optional[dict] = None
    orchestrator: Optional[str] = 'LangChain'
