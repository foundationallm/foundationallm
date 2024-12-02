"""
Class: FoundationaLLMToolBase
Description: FoundationaLLM base class for tools that uses the AgentTool model for its configuration.
"""
from langchain_core.tools import BaseTool
from foundationallm.config import Configuration
from foundationallm.models.agents import AgentTool

class FoundationaLLMToolBase(BaseTool):
    """
    FoundationaLLM base class for tools that uses the AgentTool model for its configuration.
    """
    def __init__(self, tool_config: AgentTool, objects:dict, config: Configuration):
        """ Initializes the FoundationaLLMToolBase class with the tool configuration. """
        super().__init__(
            name=tool_config.name,
            description=tool_config.description
        )
        self.tool_config = tool_config
        self.config = config
        self.objects = objects

    class Config:
        """ Pydantic configuration for FoundationaLLMToolBase. """
        extra = "allow"
