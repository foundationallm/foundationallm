"""
Implements the FoundationaLLM file analysis tool.
"""

# Platform imports
import asyncio
from typing import Any, Optional, List, ClassVar

# Azure imports
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

# LangChain imports
from langchain_azure_dynamic_sessions import SessionsPythonREPLTool
from langchain_core.callbacks import AsyncCallbackManagerForToolRun, CallbackManagerForToolRun
from langchain_core.messages import HumanMessage, SystemMessage, BaseMessage
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException
from langchain_openai import AzureOpenAI, AzureChatOpenAI

# FoundationaLLM imports
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.constants import (
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    AIModelResourceTypeNames,
    PromptResourceTypeNames
)
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.models.agents import AgentTool

class FoundationaLLMFileAnalysisTool(FoundationaLLMToolBase):

    DYNAMIC_SESSION_ENDPOINT: ClassVar[str] = "foundationallm_aca_code_execution_endpoint"
    DYNAMIC_SESSION_ID: ClassVar[str] = "foundationallm_aca_code_execution_session_id"

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMDataAnalysisTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """

        super().__init__(tool_config, objects, user_identity, config)

        self.main_llm = self.get_main_language_model()
        self.main_prompt = self.get_main_prompt()
        self.final_prompt = self.get_prompt("final_prompt")
        self.default_error_message = "An error occurred while analyzing the file."

        self.code_interpreter_tool = SessionsPythonREPLTool(
            session_id=objects[tool_config.name][self.DYNAMIC_SESSION_ID],
            pool_management_endpoint=objects[tool_config.name][self.DYNAMIC_SESSION_ENDPOINT]
        )

    def __build_messages(
        self,
        prompt: str,
        runnable_config: RunnableConfig
    ) -> List[BaseMessage]:

        user_prompt = runnable_config['configurable']['original_user_prompt'] if 'original_user_prompt' in runnable_config['configurable'] else prompt

        messages = [
            SystemMessage(content=self.code_gen_prompt),
            HumanMessage(content=user_prompt)
        ] if self.code_gen_prompt else [HumanMessage(content=user_prompt)]

        return messages

    def _run(self,
            prompt: str,
            runnable_config: RunnableConfig = None,
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:

        result = self.code_gen_llm.invoke(
            self.__build_messages(prompt, runnable_config))

        code_result = self.code_interpreter_tool.invoke(result.content)

        return code_result

    async def _arun(self,
        prompt: str = None,
        runnable_config: RunnableConfig = None,
        run_manager: Optional[AsyncCallbackManagerForToolRun] = None
        ) -> str:

        result = asyncio.run(
            self.code_gen_llm.ainvoke(self.__build_messages(prompt, runnable_config)))

        code_result = self.code_interpreter_tool.invoke(result.content)

        return code_result
