from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.plugins import ToolPluginManagerBase

from foundationallm_agent_plugins_azure_ai.tools import (
    FoundationaLLMNopTool
)

class FoundationaLLMAgentToolAzureAIPluginManager(ToolPluginManagerBase):

    FOUNDATIONALLM_NOP_TOOL_CLASS = 'FoundationaLLMNopTool'

    def __init__(self):
        super().__init__()

    def create_tool(self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration,
        intercept_http_calls: bool = False
    ) -> FoundationaLLMToolBase:

        match tool_config.class_name:
            case FoundationaLLMAgentToolAzureAIPluginManager.FOUNDATIONALLM_NOP_TOOL_CLASS:
                return FoundationaLLMNopTool(tool_config, objects, user_identity, config, intercept_http_calls=intercept_http_calls)
            case _:
                raise ValueError(f'Unknown tool class: {tool_config.class_name}')

    def refresh_tools(self):
        print('Refreshing tools...')
