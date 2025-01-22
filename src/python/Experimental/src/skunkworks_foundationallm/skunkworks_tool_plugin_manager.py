"""
Class: SkunkworksToolPluginManager
Description: Implements the ToolPluginManagerBase for the Skunkworks FoundationaLLM tools.
"""

from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.plugins import ToolPluginManagerBase

from skunkworks_foundationallm.tools import (
    FoundationaLLMIntentToolBase,
    FoundationaLLMSQLTool,
    FoundationaLLMCodeInterpreterTool,
    FoundationaLLMDataAnalysisTool
)

class SkunkworksToolPluginManager(ToolPluginManagerBase):
    """
    Implements the ToolPluginManagerBase for the Skunkworks FoundationaLLM tools.
    """

    FOUNDATIONALLM_INTENT_TOOL_BASE_NAME = 'FoundationaLLMIntentToolBase'
    FOUNDATIONALLM_SQL_TOOL_NAME = 'FoundationaLLMSQLTool'
    FOUNDATIONALLM_CODE_INTERPRETER_TOOL_NAME = 'FoundationaLLMCodeInterpreterTool'
    FOUNDATIONALLM_DATA_ANALYSIS_TOOL_NAME = 'FoundationaLLMDataAnalysisTool'

    def create_tool(self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration) -> FoundationaLLMToolBase:

        match tool_config.name:
            case SkunkworksToolPluginManager.FOUNDATIONALLM_INTENT_TOOL_BASE_NAME:
                return FoundationaLLMIntentToolBase(tool_config, objects, user_identity, config)
            case SkunkworksToolPluginManager.FOUNDATIONALLM_SQL_TOOL_NAME:
                return FoundationaLLMSQLTool(tool_config, objects, user_identity, config)
            case SkunkworksToolPluginManager.FOUNDATIONALLM_CODE_INTERPRETER_TOOL_NAME:
                return FoundationaLLMCodeInterpreterTool(tool_config, objects, user_identity, config)
            case SkunkworksToolPluginManager.FOUNDATIONALLM_DATA_ANALYSIS_TOOL_NAME:
                return FoundationaLLMDataAnalysisTool(tool_config, objects, user_identity, config)
            case _:
                raise ValueError(f"Unknown tool name: {tool_config.name}")

    def refresh_tools(self):
        ...
