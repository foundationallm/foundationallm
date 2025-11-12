from typing import List

from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import (
    AgentTool,
    GenericAgentWorkflow,
    ExternalAgentWorkflow
)
from foundationallm.langchain.common import FoundationaLLMWorkflowBase
from foundationallm.operations import OperationsManager
from foundationallm.plugins import WorkflowPluginManagerBase
from foundationallm_agent_plugins.workflows import FoundationaLLMFunctionCallingWorkflow

class FoundationaLLMAgentWorkflowPluginManager(WorkflowPluginManagerBase):

    FOUNDATIONALLM_FUNCTION_CALLING_WORKFLOW_CLASS_NAME = 'FoundationaLLMFunctionCallingWorkflow'

    def __init__(self):
        super().__init__()

    def create_workflow(self,
        workflow_config: GenericAgentWorkflow | ExternalAgentWorkflow,
        objects: dict,
        tools: List[AgentTool],
        operations_manager: OperationsManager,
        user_identity: UserIdentity,
        config: Configuration,
        intercept_http_calls: bool = False) -> FoundationaLLMWorkflowBase:
        """
        Creates a workflow instance based on the workflow configuration.

        Parameters
        ----------
        workflow_config : GenericAgentWorkflow | ExternalAgentWorkflow
            The workflow configuration.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.

        Returns
        -------
        FoundationaLLMWorkflowBase
            The workflow instance.
        """
        if workflow_config.class_name == FoundationaLLMAgentWorkflowPluginManager.FOUNDATIONALLM_FUNCTION_CALLING_WORKFLOW_CLASS_NAME:
            return FoundationaLLMFunctionCallingWorkflow(
                workflow_config,
                objects,
                tools,
                operations_manager,
                user_identity,
                config,
                intercept_http_calls=intercept_http_calls)
        raise ValueError(f'Unknown workflow name: {workflow_config.name}')

    def refresh_tools(self):
        print('Refreshing tools...') 