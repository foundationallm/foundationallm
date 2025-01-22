"""
Implements the workflow plugin manager for the FoundationaLLM Skunkworks library.
"""

from typing import List

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMWorkflowBase
from foundationallm.models.agents import AgentTool, ExternalAgentWorkflow
from foundationallm.plugins import WorkflowPluginManagerBase

from skunkworks_foundationallm.workflows import AzureOpenAIRouterWorkflow

class SkunkworksWorkflowPluginManager(WorkflowPluginManagerBase):

    """
    Implements the workflow plugin manager for the FoundationaLLM Skunkworks library.
    """

    AZURE_OPENAI_ROUTER_WORKFLOW_NAME = 'AzureOpenAIRouterWorkflow'

    def create_workflow(
        self,
        workflow_config: ExternalAgentWorkflow,
        objects: dict,
        tools: List[AgentTool],
        user_identity: UserIdentity,
        config: Configuration) -> FoundationaLLMWorkflowBase:
        """
        Create a workflow instance based on the given configuration and tools.
        Parameters
            ----------
            workflow_config : ExternalAgentWorkflow
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
            case SkunkworksWorkflowPluginManager.AZURE_OPENAI_ROUTER_WORKFLOW_NAME:
                return AzureOpenAIRouterWorkflow(workflow_config, objects, tools, user_identity, config)
            case _:
                raise ValueError(f"Unknown workflow name: {workflow_config.name}")

    def refresh_tools(self):
        ...
