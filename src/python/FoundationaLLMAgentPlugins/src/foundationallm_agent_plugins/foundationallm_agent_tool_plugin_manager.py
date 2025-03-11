from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.plugins import ToolPluginManagerBase

from foundationallm_agent_plugins.tools import (
    FoundationaLLMNopTool,
    FoundationaLLMCodeInterpreterTool,
    FoundationaLLMKnowledgeSearchTool
)

class FoundationaLLMAgentToolPluginManager(ToolPluginManagerBase):

    FOUNDATIONALLM_NOP_TOOL_NAME = 'FoundationaLLMNopTool'
    FOUNDATIONALLM_CODE_INTERPRETER_TOOL_NAME = 'FoundationaLLMCodeInterpreterTool'
    FOUNDATIONALLM_KNOWLEDGE_SEARCH_TOOL_NAME = 'FoundationaLLMKnowledgeSearchTool'

    def __init__(self):
        super().__init__()

    def create_tool(self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration) -> FoundationaLLMToolBase:

        match tool_config.name:
            case FoundationaLLMAgentToolPluginManager.FOUNDATIONALLM_NOP_TOOL_NAME:
                return FoundationaLLMNopTool(tool_config, objects, user_identity, config)
            case FoundationaLLMAgentToolPluginManager.FOUNDATIONALLM_CODE_INTERPRETER_TOOL_NAME:
                return FoundationaLLMCodeInterpreterTool(tool_config, objects, user_identity, config)
            case FoundationaLLMAgentToolPluginManager.FOUNDATIONALLM_KNOWLEDGE_SEARCH_TOOL_NAME:
                return FoundationaLLMKnowledgeSearchTool(tool_config, objects, user_identity, config)
            case _:
                raise ValueError(f'Unknown tool name: {tool_config.name}')

    def refresh_tools():
        print('Refreshing tools...') 