from typing import List

from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import (
    AgentTool,
    GenericAgentWorkflow,
    AgentWorkflowBase
)
from foundationallm.langchain.common import FoundationaLLMWorkflowBase
from foundationallm.operations import OperationsManager
from foundationallm.plugins import WorkflowPluginManagerBase
from foundationallm_agent_plugins_langchain.workflows import (
    FoundationaLLMLangChainAgentWorkflow,
    FoundationaLLMLangChainLCELWorkflow,
    FoundationaLLMLangGraphReActAgentWorkflow,
)

class FoundationaLLMAgentWorkflowLangChainPluginManager(WorkflowPluginManagerBase):

    FOUNDATIONALLM_LANGCHAIN_AGENT_WORKFLOW_CLASS_NAME = 'FoundationaLLMLangChainAgentWorkflow'
    FOUNDATIONALLM_LANGCHAIN_LCEL_WORKFLOW_CLASS_NAME = 'FoundationaLLMLangChainLCELWorkflow'
    FOUNDATIONALLM_LANGGRAPH_REACT_WORKFLOW_CLASS_NAME = 'FoundationaLLMLangGraphReActAgentWorkflow'

    def __init__(self):
        super().__init__()

    def create_workflow(self,
        workflow_config: GenericAgentWorkflow | AgentWorkflowBase,
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
        workflow_config : GenericAgentWorkflow | AgentWorkflowBase
            The workflow configuration.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.
        intercept_http_calls : bool, optional
            Whether to intercept HTTP calls made by the workflow, by default False.

        Returns
        -------
        FoundationaLLMWorkflowBase
            The workflow instance.
        """
        if workflow_config.class_name == FoundationaLLMAgentWorkflowLangChainPluginManager.FOUNDATIONALLM_LANGCHAIN_AGENT_WORKFLOW_CLASS_NAME:
            return FoundationaLLMLangChainAgentWorkflow(
                workflow_config,
                objects,
                tools,
                operations_manager,
                user_identity,
                config,
                intercept_http_calls=intercept_http_calls)
        if workflow_config.class_name == FoundationaLLMAgentWorkflowLangChainPluginManager.FOUNDATIONALLM_LANGCHAIN_LCEL_WORKFLOW_CLASS_NAME:
            return FoundationaLLMLangChainLCELWorkflow(
                workflow_config,
                objects,
                tools,
                operations_manager,
                user_identity,
                config,
                intercept_http_calls=intercept_http_calls)
        if workflow_config.class_name == FoundationaLLMAgentWorkflowLangChainPluginManager.FOUNDATIONALLM_LANGGRAPH_REACT_WORKFLOW_CLASS_NAME:
            return FoundationaLLMLangGraphReActAgentWorkflow(
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
