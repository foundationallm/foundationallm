from typing import Optional

from foundationallm.langchain.tools import FoundationaLLMToolBase

class ToolPluginManagerBase():
    def __init__(self):
        pass

    def create_tool(self,
        tool_name: str,
        ai_model_object_ids: Optional[dict],
        api_endpoint_configuration_object_ids: Optional[dict],
        properties: Optional[dict]) -> FoundationaLLMToolBase:

        print(f'Created tool {tool_name}')

    def refresh_tools():
        pass
