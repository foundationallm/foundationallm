"""
Class: ToolFactory
Description: Factory class for creating tools based on the AgentTool configuration.
"""
from foundationallm.config import Configuration
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.models.agents import AgentTool
from foundationallm.langchain.tools import FoundationaLLMToolBase, DALLEImageGenerationTool

class ToolFactory:
    """
    Factory class for creating tools based on the AgentTool configuration.
    """
    FLLM_PACKAGE_NAME = "FoundationaLLM"
    DALLE_IMAGE_GENERATION_TOOL_NAME = "DALLEImageGenerationTool"

    def get_tool(
        self,
        tool_config: AgentTool,
        objects: dict,
        config: Configuration
    ) -> FoundationaLLMToolBase:
        """
        Creates an instance of a tool based on the tool configuration.
        """
        if tool_config.package_name == self.FLLM_PACKAGE_NAME:
            # internal tools
            match tool_config.name:
                case self.DALLE_IMAGE_GENERATION_TOOL_NAME:
                    return DALLEImageGenerationTool(tool_config, objects, config)
        # else: external tools

        raise LangChainException(f"Tool {tool_config.name} not found in package {tool_config.package_name}")

