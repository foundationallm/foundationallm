# Platform imports
from typing import Optional, Type, Tuple

# LangChain imports
from langchain_core.callbacks import AsyncCallbackManagerForToolRun, CallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException

from pydantic import BaseModel

# FoundationaLLM imports
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult
)
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import (
    ContentArtifactTypeNames
)
from foundationallm.models.orchestration import ContentArtifact

from .foundationallm_nop_tool_input import FoundationaLLMNopToolInput

class FoundationaLLMNopTool(FoundationaLLMToolBase):

    args_schema: Type[BaseModel] = FoundationaLLMNopToolInput

    def __init__(
        self,
        tool_config: AgentTool,
        objects: dict,
        user_identity:UserIdentity,
        config: Configuration,
        intercept_http_calls: bool = False
    ):
        """ Initializes the FoundationaLLMNopTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)

    def _run(self,
            prompt: str,
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(self,
        prompt: str = None,
        runnable_config: RunnableConfig = None,
        run_manager: Optional[AsyncCallbackManagerForToolRun] = None
        ) -> Tuple[str, FoundationaLLMToolResult]:

        original_prompt = runnable_config['configurable']['original_user_prompt']
        self.logger.info(f'Running NOP tool with prompt: {original_prompt}')

        response = "Sometimes there is a lot of value in doing nothing."

        content_artifacts = []
        metadata = {
            'prompt_tokens': str(0),
            'completion_tokens': str(0),
            'input_prompt': prompt
        }
        content_artifacts.append(ContentArtifact(
            id = self.name,
            title = self.name,
            content = original_prompt,
            source = self.name,
            type = ContentArtifactTypeNames.TOOL_EXECUTION,
            metadata=metadata))
    
        return response, FoundationaLLMToolResult(
                content=response,
                content_artifacts=content_artifacts,
                input_tokens=0,
                output_tokens=0
            )
