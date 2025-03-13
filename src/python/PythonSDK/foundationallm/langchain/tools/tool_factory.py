"""
Class: ToolFactory
Description: Factory class for creating tools based on the AgentTool configuration.
"""
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.langchain.tools import DALLEImageGenerationTool, FoundationaLLMContentSearchTool
from foundationallm.models.agents import AgentTool
from foundationallm.plugins import PluginManager, PluginManagerTypes

class ToolFactory:
    """
    Factory class for creating tools based on the AgentTool configuration.
    """
    FLLM_PACKAGE_NAME = "FoundationaLLM"
    DALLE_IMAGE_GENERATION_TOOL = "DALLEImageGeneration"
    FOUNDATIONALLM_CONTENT_SEARCH_TOOL = "FoundationaLLMContentSearchTool"

    def __init__(self, plugin_manager: PluginManager):
        """
        Initializes the tool factory.

        Parameters
        ----------
        plugin_manager : PluginManager
            The plugin manager object used to load external tools.
        """
        self.plugin_manager = plugin_manager

    def get_tool(
        self,
        agent_name: str,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration
    ) -> FoundationaLLMToolBase:
        """
        Creates an instance of a tool based on the tool configuration.
        """

        # Use a cache key based on agent name, package name, and tool name to store the tool instance in the object cache.
        cache_key = f"{agent_name}|{tool_config.package_name}|{tool_config.name}"

        if cache_key in self.plugin_manager.object_cache:
            return self.plugin_manager.object_cache[cache_key]

        if tool_config.package_name == self.FLLM_PACKAGE_NAME:
            # Initialize by class name.
            match tool_config.class_name:
                case self.DALLE_IMAGE_GENERATION_TOOL:
                    tool = DALLEImageGenerationTool(tool_config, objects, user_identity, config)
                    self.plugin_manager.object_cache[cache_key] = tool
                case self.FOUNDATIONALLM_CONTENT_SEARCH_TOOL:
                    tool = FoundationaLLMContentSearchTool(tool_config, objects, user_identity, config)
            if tool is not None:
                return tool
        else:
            tool_plugin_manager = None

            if tool_config.package_name in self.plugin_manager.external_modules:
                tool_plugin_manager = next(( \
                    pm for pm \
                    in self.plugin_manager.external_modules[tool_config.package_name].plugin_managers \
                    if pm.plugin_manager_type == PluginManagerTypes.TOOLS), None)
                if tool_plugin_manager is None:
                    raise LangChainException(f"Tool plugin manager not found for package {tool_config.package_name}")
                tool = tool_plugin_manager.create_tool(tool_config, objects, user_identity, config)
                self.plugin_manager.object_cache[cache_key] = tool
                return tool
            else:
                raise LangChainException(f"Package {tool_config.package_name} not found in the list of external modules loaded by the package manager.")

        raise LangChainException(f"Tool {tool_config.name} not found in package {tool_config.package_name}")

