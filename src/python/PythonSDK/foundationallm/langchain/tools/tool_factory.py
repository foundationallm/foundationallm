"""
Class: ToolFactory
Description: Factory class for creating tools based on the AgentTool configuration.
"""
from foundationallm.config import Configuration
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.models.agents import AgentTool
from foundationallm.langchain.tools import FLLMToolBase, DALLEImageGenerationTool

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
    ) -> FLLMToolBase:
        """
        Creates an instance of a tool based on the tool configuration.
        """
        print("tool 1")
        if tool_config.package_name == self.FLLM_PACKAGE_NAME:
            print("tool 2")
            # internal tools
            match tool_config.name:
                case DALLE_IMAGE_GENERATION_TOOL_NAME:
                    print("tool 3")
                    return DALLEImageGenerationTool(tool_config, objects, config)
        # else: external tools
               
        raise LangChainException(f"Tool {tool_config.name} not found in package {tool_config.package_name}")   
        
