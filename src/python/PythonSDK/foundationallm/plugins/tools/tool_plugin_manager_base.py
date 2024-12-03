from abc import ABC, abstractmethod
from typing import Optional

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.models.agents import AgentTool

class ToolPluginManagerBase(ABC):
    def __init__(self):
        pass

    @abstractmethod
    def create_tool(self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration) -> FoundationaLLMToolBase:
        pass

    @abstractmethod
    def refresh_tools():
        pass
