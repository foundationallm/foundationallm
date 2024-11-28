"""
Class: ToolFactory
Description: Factory class for creating tools based on the AgentTool configuration.
"""
from foundationallm.config import Configuration
from foundationallm.models.agents import AgentTool
from foundationallm.tools import FLLMToolBase, DALLEImageGenerationTool

class ToolFactory:
    """
    Factory class for creating tools based on the AgentTool configuration.
    """
    FLLM_PACKAGE_NAME = "FoundationaLLM"
    DALLE_TOOL_NAME = "dalle-image-generation"

    def get_tool(
        self,
        tool_config: AgentTool,
        objects: dict,
        config: Configuration 
    ) -> FLLMToolBase:
        """
        Creates an instance of a tool based on the tool configuration.
        """        
        if tool_config.package_name == self.FLLM_PACKAGE_NAME:            
            # internal tools
            if tool_config.name == self.DALLE_TOOL_NAME:
                return DALLEImageGenerationTool(tool_config, objects, config)
        # else: external tools
