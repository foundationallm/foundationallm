# pylint: disable=W0221

from typing import Optional, Tuple, Type, List, ClassVar, Any
from uuid import uuid4
import json

from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.messages import (
    SystemMessage,
    HumanMessage
)
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException

from opentelemetry.trace import SpanKind
from pydantic import BaseModel

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult
)
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import (
    ContentArtifactTypeNames,
    RunnableConfigKeys
)
from foundationallm.models.orchestration import CompletionRequestObjectKeys, ContentArtifact
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.services import HttpClientService
from foundationallm.utils import LoggingAsyncHttpClient

from .foundationallm_code_interpreter_tool_input import FoundationaLLMCodeInterpreterToolInput

class FoundationaLLMCodeInterpreterTool(FoundationaLLMToolBase):
    """ A tool for executing Python code in a code interpreter. """
    args_schema: Type[BaseModel] = FoundationaLLMCodeInterpreterToolInput
    DYNAMIC_SESSION_ENDPOINT: ClassVar[str] = "code_session_endpoint"
    DYNAMIC_SESSION_ID: ClassVar[str] = "code_session_id"

    def __init__(
        self,
        tool_config: AgentTool,
        objects: dict,
        user_identity:UserIdentity,
        config: Configuration,
        intercept_http_calls: bool = False
    ):
        """ Initializes the FoundationaLLMCodeInterpreterTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)

        context_api_endpoint_configuration = APIEndpointConfiguration(**objects.get(CompletionRequestObjectKeys.CONTEXT_API_ENDPOINT_CONFIGURATION, None))
        if context_api_endpoint_configuration:
            self.context_api_client = HttpClientService(
                context_api_endpoint_configuration,
                user_identity,
                config
            )
        else:
            raise ToolException("The Context API endpoint configuration is required to use the Code Interpreter tool.")
        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID, None)
        self.main_llm = self.get_main_language_model(
            http_async_client=LoggingAsyncHttpClient(timeout=30.0)
        ) if intercept_http_calls else self.get_main_language_model()

    def _run(
        self,
        prompt: str,
        file_names: Optional[List[str]] = [],
        run_manager: Optional[CallbackManagerForToolRun] = None,
        **kwargs: Any) -> Any:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(self,
            prompt: str,
            file_names: Optional[List[str]] = [],
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
            runnable_config: RunnableConfig = None,
            **kwargs: Any) -> Tuple[str, FoundationaLLMToolResult]:

        main_prompt = self.get_main_prompt()

        # Get the original prompt
        if runnable_config is None:
            user_prompt = None
            user_prompt_rewrite = None
        else:
            user_prompt = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT] \
                if RunnableConfigKeys.ORIGINAL_USER_PROMPT in runnable_config['configurable'] \
                else None
            user_prompt_rewrite = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE] \
                if RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE in runnable_config['configurable'] \
                else None

        session_id = runnable_config['configurable'][self.tool_config.name][self.DYNAMIC_SESSION_ID]

        llm_prompt = prompt or user_prompt_rewrite or user_prompt
        content_artifacts = []
        operation_id = None
        input_tokens = 0
        output_tokens = 0
        generated_code = ''

        with self.tracer.start_as_current_span(f'{self.name}_initial_llm_call', kind=SpanKind.INTERNAL):

            available_file_names = '\n'.join([f'/{file_name}/' for file_name in file_names])
            code_generation_prompt = main_prompt.replace('{{file_names}}', available_file_names)

            messages = [
                SystemMessage(content=code_generation_prompt),
                HumanMessage(content=llm_prompt)
            ]

            response = await self.main_llm.ainvoke(messages)

            input_tokens += response.usage_metadata['input_tokens']
            output_tokens += response.usage_metadata['output_tokens']

            generated_code = self.__get_code_from_content_blocks(response.content_blocks)

        if generated_code.strip() == '':
            code_execution_response = {
                'status': 'Failed',
                'execution_result': '',
                'error_output': 'No code was generated by the language model to execute.',
                'standard_output': ''
            }
        else:
            # Start the process of executing the code in the code interpreter

            # returns the operation_id
            self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
            operation_response = await self.context_api_client.post_async(
                endpoint = f"/instances/{self.instance_id}/codeSessions/{session_id}/uploadFiles",
                data = json.dumps({
                    "file_names": file_names
                })
            )
            operation_id = operation_response['operation_id']

            # Obtain beginning file list from the context API
            beginning_files_list = []
            beginning_files_list_response = await self.context_api_client.post_async(
                    endpoint = f"/instances/{self.instance_id}/codeSessions/{session_id}/downloadFiles",
                    data = json.dumps({
                        "operation_id": operation_id
                    })
                )
            beginning_files_list = beginning_files_list_response['file_records']

            try:
                # Execute the code
                code_execution_response = await self.context_api_client.post_async(
                    endpoint = f"/instances/{self.instance_id}/codeSessions/{session_id}/executeCode",
                    data = json.dumps({
                        "code_to_execute": generated_code
                    })
                )

                # Get an updated list of files from the code interpreter
                files_list = []
                if operation_id:
                    files_list_response = await self.context_api_client.post_async(
                        endpoint = f"/instances/{self.instance_id}/codeSessions/{session_id}/downloadFiles",
                        data = json.dumps({
                            "operation_id": operation_id
                        })
                    )
                    files_list = files_list_response['file_records']
                    # Remove files that were already present in the beginning of the session
                    files_list = {key: value for key, value in files_list.items() if key not in beginning_files_list}

                if files_list:
                    # Download the files from the code interpreter to the user storage container
                    for file_name, file_data in files_list.items():
                        content_artifacts.append(ContentArtifact(
                            id = self.name,
                            title = f'{self.name} (file)',
                            type = ContentArtifactTypeNames.FILE,
                            filepath = file_name,
                            metadata = {
                                'file_object_id': file_data['file_object_id'],
                                'original_file_name': file_data['file_name'],
                                'file_path': file_data['file_path'],
                                'file_size': str(file_data['file_size_bytes']),
                                'content_type': file_data['content_type'],
                                'conversation_id': file_data['conversation_id']
                            }
                        ))
            except Exception as e:
                # Handle cases where the code execution container does not get a chance to return
                # a response (e.g., crashes, timeouts, etc.)
                code_execution_response = {
                    'status': 'Failed',
                    'execution_result': '',
                    'error_output': str(e),
                    'standard_output': ''
                }

        final_response = ""
        if code_execution_response['status'] == 'Failed':
            final_response = '\n'.join([
                "The generated code could not be executed successfully. ",
                code_execution_response['execution_result'],
                code_execution_response.get('error_output', '')]
            )
        elif code_execution_response['status'] == 'Succeeded':
            if (code_execution_response.get('error_output') or '').strip() != '':
                # If there is error output, prioritize that
                final_response = code_execution_response.get('error_output', '')
            elif (code_execution_response.get('execution_result') or '') != '{}':
                final_response = code_execution_response.get('execution_result', '')
            else:
                final_response = (code_execution_response.get('standard_output') or '').strip()
            
            # If we have files but no output, provide context about what was created
            if not final_response.strip() and files_list:
                file_descriptions = [f"- {file_data['file_name']}" for file_data in files_list.values()]
                final_response = "Code executed successfully. The following files were created:\n" + "\n".join(file_descriptions)

        else:
            status = code_execution_response.get('status', 'Unknown')
            error_output = code_execution_response.get('error_output', '')
            standard_output = code_execution_response.get('standard_output', '')

            final_response = (
                f"Code execution failed with unexpected status '{status}'. "
                f"Error details: {error_output if error_output else 'No error details available'}. "
                f"Output: {standard_output if standard_output else 'No output available'}"
            )

        content_artifacts.append(ContentArtifact(
            id = self.name,
            title = self.name,
            type = ContentArtifactTypeNames.TOOL_EXECUTION,
            filepath = str(uuid4()), # needs to have a unique filepath to not be filtered out upstream.
            metadata = {
                'original_user_prompt': user_prompt_rewrite or user_prompt,
                'tool_input_prompt': prompt,
                'tool_input_files': ', '.join(file_names) if file_names else '',
                'tool_generated_code': generated_code,
                'tool_output': code_execution_response.get('standard_output', ''),
                'tool_error': code_execution_response.get('error_output', ''),
                'tool_result': final_response
            }
        ))

        return final_response, FoundationaLLMToolResult(
            content=final_response,
            content_artifacts=content_artifacts,
            input_tokens=input_tokens,
            output_tokens=output_tokens
        )

    def __get_code_from_content_blocks(self, content_blocks: List[dict]) -> str:
        """ Extracts code from content blocks returned by the LLM. """
        if isinstance(content_blocks, list):
            text_parts = [self.__prepare_code(block["text"]) for block in content_blocks if block.get("type") == "text"]
            text = "".join(text_parts)
            return text
        else:
            return ""

    def __prepare_code(self, code: str) -> str:
        """ Prepares the code for execution by removing any leading/trailing whitespace and ensuring it is valid code. """
        # Remove leading/trailing whitespace
        code = code.strip()
        # Ensure the code is valid Python code

        if code.startswith('```python'):
            code = code[9:].strip()
        if code.startswith('```'):
            code = code[3:].strip()
        if code.endswith('```'):
            code = code[:-3].strip()

        if not code.endswith('\n'):
            code += '\n'
        return code
