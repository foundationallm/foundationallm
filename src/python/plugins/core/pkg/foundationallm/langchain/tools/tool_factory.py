"""
Class: ToolFactory
Description: Factory class for creating tools based on the AgentTool configuration.
"""
import re

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.langchain.tools import DALLEImageGenerationTool
from foundationallm.models.agents import AgentTool
from foundationallm.plugins import PluginManager, PluginManagerTypes
from foundationallm.telemetry import Telemetry

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
        self.logger = Telemetry.get_logger(self.__class__.__name__)

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

        # NOTE: Disabled tool caching due to multiple implications with object lifetimes and stateful tools.
        # For example, tools that use LangChain LLM classed that authentication with tokens that expire pose
        # challenges when caching tools.

        # TODO: Revisit tool caching strategy in the future. Implement a more elaborate caching mechanism that
        # considers object lifetimes, stateful vs stateless tools, and token expiration handling.

        # Use a cache key based on agent name, package name, and tool name to store the tool instance in the object cache.
        # cache_key = f"{(self.__normalize_upn(user_identity.upn))}|{agent_name}|{tool_config.package_name}|{tool_config.name}"

        # if cache_key in self.plugin_manager.object_cache:
        #     self.logger.info("Using cached tool instance for key: %s", cache_key)
        #     return self.plugin_manager.object_cache[cache_key]

        # self.logger.info("Creating new tool instance for key: %s", cache_key)

        if tool_config.package_name == self.FLLM_PACKAGE_NAME:
            # Initialize by class name.
            match tool_config.class_name:
                case self.DALLE_IMAGE_GENERATION_TOOL:
                    tool = DALLEImageGenerationTool(tool_config, objects, user_identity, config)
                    # self.plugin_manager.object_cache[cache_key] = tool
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
                # self.plugin_manager.object_cache[cache_key] = tool
                return tool

            raise LangChainException(f"Package {tool_config.package_name} not found in the list of external modules loaded by the package manager.")

        raise LangChainException(f"Tool {tool_config.name} not found in package {tool_config.package_name}")

    def __normalize_upn(self, upn: str) -> str:
        """
        Normalize a UPN string by:
        - Trimming whitespace
        - Lowercasing
        - Replacing special characters with '-'
        """

        upn = upn.strip().lower()
        # Replace any character that is not a-z, 0-9 with '-'
        upn = re.sub(r'[^a-z0-9]', '-', upn)
        return upn