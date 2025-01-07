"""
Class: FoundationaLLMToolBase
Description: FoundationaLLM base class for tools that uses the AgentTool model for its configuration.
"""

from typing import List, Dict

# Platform imports
from azure.identity import DefaultAzureCredential
from logging import Logger
from opentelemetry.trace import Tracer

# LangChain imports
from langchain_core.tools import BaseTool

# FoundationaLLM imports
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool
from foundationallm.telemetry import Telemetry

class FoundationaLLMToolBase(BaseTool):
    """
    FoundationaLLM base class for tools that uses the AgentTool model for its configuration.
    """

    response_format: str = 'content_and_artifact'

    def __init__(self, tool_config: AgentTool, objects:Dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMToolBase class with the tool configuration. """
        super().__init__(
            name=tool_config.name,
            description=tool_config.description
        )
        self.tool_config = tool_config
        self.objects = objects
        self.user_identity = user_identity
        self.config = config

        self.logger: Logger = Telemetry.get_logger(self.name)
        self.tracer: Tracer = Telemetry.get_tracer(self.name)
        self.default_credential = DefaultAzureCredential(exclude_environment_credential=True)

    class Config:
        """ Pydantic configuration for FoundationaLLMToolBase. """
        extra = "allow"
