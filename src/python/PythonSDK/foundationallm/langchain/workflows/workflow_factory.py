"""
Class: WorkflowFactory
Description: Factory class for creating an external workflow instance based on the Agent workflow configuration.
"""
from typing import List
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMWorkflowBase
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.langchain.workflows.azure_ai.azure_ai_agent_service_workflow import AzureAIAgentServiceWorkflow
from foundationallm.models.agents import (
    AgentTool,
    GenericAgentWorkflow,
    ExternalAgentWorkflow
)
from foundationallm.operations import OperationsManager
from foundationallm.plugins import PluginManager, PluginManagerTypes

class WorkflowFactory:
    """
    Factory class for creating an external agent workflow instance based on the Agent workflow configuration.
    """   
    def __init__(self, plugin_manager: PluginManager, operations_manager: OperationsManager = None):
        """
        Initializes the workflow factory.

        Parameters
        ----------
        plugin_manager : PluginManager
            The plugin manager object used to load external workflows.
        """
        self.plugin_manager = plugin_manager
        self.operations_manager = operations_manager

    def get_workflow(
        self,
        workflow_config: GenericAgentWorkflow | ExternalAgentWorkflow,
        objects: dict,
        tools: List[AgentTool],
        user_identity: UserIdentity,
        config: Configuration
    ) -> FoundationaLLMWorkflowBase:
        """
        Creates an instance of an agent workflow based on the agent workflow configuration.

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
        if workflow_config.package_name == "FoundationaLLM":
            raise LangChainException(f"The legacy FoundationaLLM virtual package is not supported by the workflow factory anymore.")
        else:
            workflow_plugin_manager = None

            if workflow_config.package_name in self.plugin_manager.external_modules:
                workflow_plugin_manager = next(( \
                    wm for wm \
                    in self.plugin_manager.external_modules[workflow_config.package_name].plugin_managers \
                    if wm.plugin_manager_type == PluginManagerTypes.WORKFLOWS), None)
                if workflow_plugin_manager is None:
                    raise LangChainException(f"Workflow plugin manager not found for package {workflow_config.package_name}")
                return workflow_plugin_manager.create_workflow(
                    workflow_config,
                    objects,
                    tools,
                    self.operations_manager,
                    user_identity,
                    config)

            raise LangChainException(f"Package {workflow_config.package_name} not found in the list of external modules loaded by the package manager.")
