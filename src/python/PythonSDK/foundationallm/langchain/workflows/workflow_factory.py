"""
Class: WorkflowFactory
Description: Factory class for creating workflows based on the Agent workflow configuration.
"""
from typing import Any
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.plugins import PluginManager

class WorkflowFactory:
    """
    Factory class for creating workflows based on the Agent workflow configuration.
    """
    FLLM_PACKAGE_NAME = "FoundationaLLM"    

    def __init__(self, plugin_manager: PluginManager):
        """
        Initializes the workflow factory.

        Parameters
        ----------
        plugin_manager : PluginManager
            The plugin manager object used to load external workflows.
        """
        self.plugin_manager = plugin_manager

    def get_workflow(
        self,
        workflow_config,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration
    ) -> Any:
        """
        Creates an instance of a workflow based on the agent workflow configuration.
        """
        if workflow_config.package_name == self.FLLM_PACKAGE_NAME:
            # internal workflow
            return workflow_config
        else:
            workflow_plugin_manager = None
            if workflow_config.package_name in self.plugin_manager.external_modules:
                workflow_plugin_manager = self.plugin_manager.external_modules[workflow_config.package_name].plugin_manager
                return workflow_plugin_manager.create_workflow(workflow_config, user_identity, config)
            else:
                raise LangChainException(f"Package {workflow_config.package_name} not found in the list of external modules loaded by the package manager.")

        raise LangChainException(f"Workflow {workflow_config.name} not found in package {workflow_config.package_name}")

