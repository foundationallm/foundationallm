from abc import ABC, abstractmethod
from typing import List
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMWorkflowBase
from foundationallm.models.agents import (
    AgentTool,
    GenericAgentWorkflow,
    ExternalAgentWorkflow
)
from foundationallm.operations import OperationsManager
from foundationallm.plugins import PluginManagerTypes

class WorkflowPluginManagerBase(ABC):
    """
    The base class for all workflow plugin managers.
    """
    def __init__(self):
        
        self.plugin_manager_type = PluginManagerTypes.WORKFLOWS

    @abstractmethod
    def create_workflow(self,
        workflow_config: GenericAgentWorkflow | ExternalAgentWorkflow,
        objects: dict,
        tools: List[AgentTool],
        operations_manager: OperationsManager,
        user_identity: UserIdentity,
        config: Configuration) -> FoundationaLLMWorkflowBase:
        """
        Create a workflow instance based on the given configuration and tools.
        Parameters
            ----------
            workflow_config : GenericAgentWorkflow | ExternalAgentWorkflow
                The workflow assigned to the agent.
            objects : dict
                The exploded objects assigned from the agent.
            tools : List[AgentTool]
                The tools assigned to the agent.
            user_identity : UserIdentity
                The user identity of the user initiating the request.
            config : Configuration
                The application configuration for FoundationaLLM.
        """
        pass

    @abstractmethod
    def refresh_tools(self):
        pass
