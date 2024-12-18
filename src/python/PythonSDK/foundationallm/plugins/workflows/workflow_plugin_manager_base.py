from abc import ABC, abstractmethod
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.models.agents import AgentWorkflowBase

class WorkflowPluginManagerBase(ABC):
    def __init__(self):
        pass

    @abstractmethod
    def create_workflow(self,
        workflow_config: AgentWorkflowBase,        
        user_identity: UserIdentity,
        config: Configuration) -> FoundationaLLMToolBase:
        pass

    @abstractmethod
    def refresh_tools():
        pass
