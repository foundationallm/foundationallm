"""
Implements the FoundationaLLM file analysis tool.
"""

# Platform imports
import asyncio
from io import BytesIO
from typing import Optional, List, ClassVar, Tuple

# Azure imports
from azure.identity import DefaultAzureCredential
from azure.storage.blob import BlobServiceClient

from opentelemetry.trace import SpanKind

# LangChain imports
from langchain_core.callbacks import CallbackManagerForToolRun
from langchain_core.messages import (
    BaseMessage,
    SystemMessage,
    HumanMessage,
    AIMessage
)
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException

# FoundationaLLM imports
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult
)
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import RunnableConfigKeys

class FoundationaLLMFileAnalysisTool(FoundationaLLMToolBase):
    """ A tool for analyzing files using a large language model. """

    DYNAMIC_SESSION_ENDPOINT: ClassVar[str] = "code_session_endpoint"
    DYNAMIC_SESSION_ID: ClassVar[str] = "code_session_id"

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMDataAnalysisTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """

        super().__init__(tool_config, objects, user_identity, config)

        self.main_llm = self.get_main_language_model()
        self.main_prompt = self.get_main_prompt()
        self.final_prompt = self.get_prompt("final_prompt")
        self.default_error_message = "An error occurred while analyzing the file."

        self.__setup_file_analysis_configuration(tool_config, objects, config)

    def _run(self,
            prompt: str,
            runnable_config: RunnableConfig = None,
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:

        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(
        self,
        *args,
        prompt: str = None,
        message_history: List[BaseMessage] = [],
        runnable_config: RunnableConfig = None,
        **kwargs,
        ) -> Tuple[str, FoundationaLLMToolResult]:

        input_tokens = 0
        output_tokens = 0
        generated_code = ''
        final_response = ''

        # Get the original prompt            
        if runnable_config is None:
            original_prompt = prompt
        else:
            user_prompt = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT] \
                if RunnableConfigKeys.ORIGINAL_USER_PROMPT in runnable_config['configurable'] \
                else None
            user_prompt_rewrite = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE] \
                if RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE in runnable_config['configurable'] \
                else None
            original_prompt = user_prompt_rewrite or user_prompt or prompt

        messages = [
            SystemMessage(content=self.main_prompt),
            *message_history,
            HumanMessage(content=original_prompt)
        ]

        with self.tracer.start_as_current_span(self.name, kind=SpanKind.INTERNAL):
            try:

                with self.tracer.start_as_current_span(f'{self.name}_initial_llm_call', kind=SpanKind.INTERNAL):

                    response = await self.main_llm.ainvoke(messages)

                    input_tokens += response.usage_metadata['input_tokens']
                    output_tokens += response.usage_metadata['output_tokens']

                    generated_code = response.content

                data = self.__get_file_binary_content()
                upload_result = self.code_interpreter_tool.upload_file(
                    data=BytesIO(data),
                    remote_file_path=self.file_name,
                )
                code_result = self.code_interpreter_tool.invoke(generated_code)

                final_messages = [
                        SystemMessage(content=self.final_prompt),
                        HumanMessage(content=original_prompt),
                        AIMessage(content=code_result)
                    ]

                with self.tracer.start_as_current_span(f'{self.name}_final_llm_call', kind=SpanKind.INTERNAL):
                    final_llm_response = await self.main_llm.ainvoke(final_messages, tools=None)
                    input_tokens += final_llm_response.usage_metadata['input_tokens']
                    output_tokens += final_llm_response.usage_metadata['output_tokens']
                    final_response = final_llm_response.content

                return final_response, FoundationaLLMToolResult(
                    content=final_response,
                    content_artifacts=[
                        self.create_content_artifact(
                            original_prompt,
                            tool_input=generated_code,
                            prompt_tokens=input_tokens,
                            completion_tokens=output_tokens
                        )
                    ],
                    input_tokens=input_tokens,
                    output_tokens=output_tokens
                )

            except Exception as e:
                self.logger.error('An error occured in tool %s: %s', self.name, e)
                return self.default_error_message, FoundationaLLMToolResult(
                    content=self.default_error_message,
                    content_artifacts=[
                        self.create_content_artifact(
                            original_prompt,
                            tool_input=generated_code,
                            prompt_tokens=input_tokens,
                            completion_tokens=output_tokens
                        ),
                        self.create_error_content_artifact(
                            original_prompt,
                            e
                        )
                    ],
                    input_tokens=input_tokens,
                    output_tokens=output_tokens
                )

    def __setup_file_analysis_configuration(
            self,
            tool_config: AgentTool,
            objects: dict,
            config: Configuration,
    ):
        # self.code_interpreter_tool = SessionsPythonREPLTool(
        #     session_id=objects[tool_config.name][self.DYNAMIC_SESSION_ID],
        #     pool_management_endpoint=objects[tool_config.name][self.DYNAMIC_SESSION_ENDPOINT]
        # )

        self.storage_type = tool_config.properties['storage_type']

        tokens = tool_config.properties['file_path'].split('/')
        self.file_container_name = tokens[0]
        self.file_name = tokens[-1]
        self.file_path = '/'.join(tokens[1:])

        if (self.storage_type == "OneLake"):

            credential = DefaultAzureCredential()
            self.storage_client = BlobServiceClient(account_url=f"https://onelake.dfs.fabric.microsoft.com", credential=credential)
            self.storage_container_client = self.storage_client.get_container_client(self.file_container_name)

    def __get_file_binary_content(
            self
    ) -> bytes:

        blob = self.storage_container_client.get_blob_client(self.file_path)
        stream = BytesIO()
        blob.download_blob().readinto(stream)
        return stream.getvalue()
