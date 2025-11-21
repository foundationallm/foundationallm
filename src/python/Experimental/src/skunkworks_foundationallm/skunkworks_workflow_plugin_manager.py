from typing import List

from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import (
    AgentTool,
    GenericAgentWorkflow,
    ExternalAgentWorkflow
)
from foundationallm.langchain.common import FoundationaLLMWorkflowBase
from foundationallm.plugins import WorkflowPluginManagerBase

from skunkworks_foundationallm.workflows import (
    FoundationaLLMRouterWorkflow
)

class SkunkworksWorkflowPluginManager(WorkflowPluginManagerBase):

    FOUNDATIONALLM_ROUTER_WORKFLOW_NAME = 'FoundationaLLMRouterWorkflow'

    def __init__(self):
        super().__init__()

    def create_workflow(
        self,
        workflow_config: GenericAgentWorkflow | ExternalAgentWorkflow,
        objects: dict,
        tools: List[AgentTool],
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
        match workflow_config.name:
            case SkunkworksWorkflowPluginManager.FOUNDATIONALLM_ROUTER_WORKFLOW_NAME:
                return FoundationaLLMRouterWorkflow(workflow_config, objects, tools, user_identity, config)
            case _:
                raise ValueError(f"Unknown tool name: {workflow_config.name}")

    def refresh_tools(self):
        print('Refreshing tools...')
