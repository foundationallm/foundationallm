"""
Class: FoundationaLLMFunctionCallingWorkflow
Description: FoundationaLLM Function Calling workflow to invoke tools at a low level.
"""

import base64
import json
import re
import time
from typing import Dict, List, Optional
from opentelemetry.trace import SpanKind

from langchain_core.messages import (
    AIMessage,
    BaseMessage,
    HumanMessage,
    SystemMessage
)
from langchain_core.runnables import RunnableConfig

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMWorkflowBase,
    FoundationaLLMToolBase
)
from foundationallm.models.agents import (
    GenericAgentWorkflow,
    ExternalAgentWorkflow
)
from foundationallm.models.constants import (
    AgentCapabilityCategories,
    ContentArtifactTypeNames,
    RunnableConfigKeys,
    TemplateVariables
)
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.operations import OperationStatus
from foundationallm.models.orchestration import (
    CompletionRequestObjectKeys,
    CompletionResponse,
    ContentArtifact,
    FileHistoryItem,
    OpenAITextMessageContentItem,
    OpenAIImageFileMessageContentItem,
    OpenAIFilePathMessageContentItem
)
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.operations import OperationsManager
from foundationallm.services import HttpClientService

class FoundationaLLMFunctionCallingWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow implementing a router pattern for tool invocation
    using Azure OpenAI completion models.
    """

    def __init__(
        self,
        workflow_config: GenericAgentWorkflow | ExternalAgentWorkflow,
        objects: Dict,
        tools: List[FoundationaLLMToolBase],
        operations_manager: OperationsManager,
        user_identity: UserIdentity,
        config: Configuration,
        intercept_http_calls: bool = False
    ):
        """
        Initializes the FoundationaLLMWorkflowBase class with the workflow configuration.

        Parameters
        ----------
        workflow_config : GenericAgentWorkflow | ExternalAgentWorkflow
            The workflow assigned to the agent.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.
        intercept_http_calls : bool, optional
            Whether to intercept HTTP calls made by the workflow, by default False.
        """
        super().__init__(workflow_config, objects, tools, operations_manager, user_identity, config)
        self.name = workflow_config.name
        self.default_error_message = workflow_config.properties.get(
            'default_error_message',
            'An error occurred while processing the request.') \
            if workflow_config.properties else 'An error occurred while processing the request.'

        self.create_workflow_llm(intercept_http_calls=intercept_http_calls)
        self.__create_context_client()

        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID, None)

        # Special commands pattern for user prompts
        # Matches patterns like [command1, command2]: user prompt
        self.special_commands_pattern = re.compile(
            r'^\[\s*([A-Za-z_][A-Za-z0-9_]*(\s*,\s*[A-Za-z_][A-Za-z0-9_]*)*)?\s*\]:'
        )

    async def invoke_async(self,
                           operation_id: str,
                           user_prompt:str,
                           user_prompt_rewrite: Optional[str],
                           message_history: List[MessageHistoryItem],
                           file_history: List[FileHistoryItem],
                           conversation_id: Optional[str] = None,
                           objects: dict = None)-> CompletionResponse:
        """
        Invokes the workflow asynchronously.

        Parameters
        ----------
        operation_id : str
            The unique identifier of the FoundationaLLM operation.
        user_prompt : str
            The user prompt message.
        user_prompt_rewrite : str
            The user prompt rewrite message containing additional context to clarify the user's intent.
        message_history : List[BaseMessage]
            The message history.
        file_history : List[FileHistoryItem]
            The file history.
        conversation_id : Optional[str]
            The conversation identifier for the workflow execution.
        objects : dict
            The exploded objects assigned from the agent. This is used to pass additional context to the workflow.
        """
        if objects is None:
            objects = {}

        workflow_main_prompt = self.create_workflow_main_prompt()
        workflow_router_prompt = self.create_workflow_router_prompt()
        workflow_files_prompt = self.create_workflow_files_prompt()
        workflow_final_prompt = self.create_workflow_final_prompt()

        llm_prompt = user_prompt_rewrite or user_prompt

        commands, llm_prompt = self.__extract_special_commands(llm_prompt)

        runnable_config = self.__get_tools_runnable_config(
            user_prompt,
            user_prompt_rewrite,
            file_history,
            conversation_id=conversation_id,
            objects=objects
        )

        messages = await self.__get_message_list(
            llm_prompt,
            workflow_main_prompt,
            workflow_router_prompt,
            workflow_files_prompt,
            message_history,
            objects,
            file_history
        )

        content_artifacts: List[ContentArtifact] = []
        input_tokens = 0
        output_tokens = 0
        intermediate_responses: List[str] = []
        
        initial_response: Optional[str] = None
        final_response: Optional[str] = None
        failure_response: str = 'Failed to generate a response.'

        final_message: Optional[HumanMessage] = None
        error_message: Optional[str] = None

        with self.tracer.start_as_current_span(f'{self.name}_workflow', kind=SpanKind.INTERNAL):

            await self.operations_manager.update_operation_with_text_result_async(
                operation_id,
                self.instance_id,
                OperationStatus.INPROGRESS,
                "Running agent workflow.",
                "Running agent workflow...",
                self.user_identity.model_dump_json(exclude_none=True)
            )

            with self.tracer.start_as_current_span(f'{self.name}_workflow_llm_call', kind=SpanKind.INTERNAL):
                router_start_time = time.time()

                # TODO: This is a placeholder for explicit tool invocation logic (currently copied over from the original code and commented out).
                # parsed_user_prompt = request.user_prompt

                # explicit_tool = next((tool for tool in agent.tools if parsed_user_prompt.startswith(f'[{tool.name}]:')), None)
                # if explicit_tool is not None:
                #     tools.append(tool_factory.get_tool(agent.name, explicit_tool, request.objects, self.user_identity, self.config))
                #     parsed_user_prompt = parsed_user_prompt.split(':', 1)[1].strip()

                llm_bound_tools = self.workflow_llm.bind_tools(self.tools)
                try:
                    llm_response = await llm_bound_tools.ainvoke(
                        messages,
                        tool_choice='auto')
                    initial_response = self.get_text_from_message(llm_response)
                    usage = self.__get_canonical_usage(llm_response)
                    input_tokens += usage['input_tokens']
                    output_tokens += usage['output_tokens']
                except Exception as ex:
                    self.logger.error('Error during LLM invocation: %s', str(ex))
                    error_message = str(ex)

                router_end_time = time.time()

            should_process_final_response = False

            if not error_message:

                if llm_response.tool_calls:

                    if 'ROUTER' in commands:
                        # If the special command [ROUTER] is present, return the router's tool selection directly without tool execution.

                        final_response = '\n'.join(f'{tool_call["name"]} [{json.dumps(tool_call["args"])}]' for tool_call in llm_response.tool_calls)

                    else:

                        await self.operations_manager.update_operation_with_text_result_async(
                            operation_id,
                            self.instance_id,
                            OperationStatus.INPROGRESS,
                            "Running agent tools.",
                            "Running agent tools...",
                            self.user_identity.model_dump_json(exclude_none=True)
                        )

                        llm_response_content_text = self.get_text_from_message(llm_response)
                        if llm_response_content_text:
                            intermediate_responses.append(str(llm_response_content_text))

                        for tool_call in llm_response.tool_calls:

                            with self.tracer.start_as_current_span(f'{self.name}_tool_call', kind=SpanKind.INTERNAL) as tool_call_span:
                                tool_call_span.set_attribute('tool_call_id', tool_call['id'])
                                tool_call_span.set_attribute('tool_call_function', tool_call['name'])

                                # Get the tool from the tools list
                                tool = next((t for t in self.tools if t.name == tool_call['name']), None)
                                if tool:
                                    tool_result = await tool.ainvoke(tool_call, runnable_config)
                                    content_artifacts.extend(tool_result.artifact.content_artifacts)
                                    intermediate_responses.append(str(tool_result.artifact.content))
                                    input_tokens += tool_result.artifact.input_tokens
                                    output_tokens += tool_result.artifact.output_tokens
                                else:
                                    self.logger.error(
                                        'Tool %s not found in the tools list. Skipping tool call.', tool_call["name"])

                        # Ask the LLM to verify if the answer is correct if not, loop again with the current messages.
                        # verification_messages = messages_with_toolchain.copy()
                        # verification_messages.append(HumanMessage(content=f'Verify the requirements are met for this request: "{llm_prompt}", use the other messages only for context. If yes, answer with the single word "DONE". If not, generate more detailed instructions to satisfy the request.'))
                        # verification_llm_response = await self.workflow_llm.ainvoke(verification_messages, tools=None)
                        # verification_response = verification_llm_response.content
                        # if verification_response.strip().upper() == 'DONE':
                        #     break # exit the loop if the requirements are met.
                        # else:
                        #     messages_with_toolchain.append(AIMessage(content=verification_response))
                        #     continue # loop again if the requirements are not met.

                        final_message = self.__get_message_for_final_response(
                            intermediate_responses,
                            llm_prompt,
                            workflow_final_prompt
                        )

                        should_process_final_response = True

                else:
                    if 'ROUTER' in commands:
                        final_response = '__NO_TOOL__'

            else:
                final_message = HumanMessage(content=f"Produce a non-technical summary of the following error message: {error_message}." +
                                             "\n\nRemove any references to technology or platform names, code, stack traces, or technical details.")

                should_process_final_response = True

            if should_process_final_response:

                with self.tracer.start_as_current_span(f'{self.name}_final_llm_call', kind=SpanKind.INTERNAL):

                    await self.operations_manager.update_operation_with_text_result_async(
                        operation_id,
                        self.instance_id,
                        OperationStatus.INPROGRESS,
                        "Preparing final response.",
                        "Preparing final response...",
                        self.user_identity.model_dump_json(exclude_none=True)
                    )

                    final_messages = [SystemMessage(content=workflow_main_prompt)] + messages[1:-1]
                    if final_message:
                        final_messages.append(final_message)
                    
                    try:
                        final_llm_response = await self.workflow_llm.ainvoke(final_messages)

                        usage = self.__get_canonical_usage(final_llm_response)
                        input_tokens += usage['input_tokens']
                        output_tokens += usage['output_tokens']
                        final_response = self.get_text_from_message(final_llm_response)
                    except Exception as ex:
                        self.logger.error('Error during final LLM invocation: %s', str(ex))
                        error_message = str(ex)

                        # Attempt to extract a user-friendly error message from the exception
                        error_llm_response = await self.workflow_llm.ainvoke([
                            SystemMessage(content=workflow_main_prompt),
                            HumanMessage(content=f"Produce a non-technical summary of the following error message: {error_message}." +
                                                 "\n\nRemove any references to technology or platform names, code, stack traces, or technical details.")
                        ])
                        usage = self.__get_canonical_usage(error_llm_response)
                        input_tokens += usage['input_tokens']
                        output_tokens += usage['output_tokens']
                        
                        final_response = self.get_text_from_message(error_llm_response)

            workflow_content_artifact = self.create_workflow_execution_content_artifact(
                llm_prompt,
                input_tokens,
                output_tokens,
                router_end_time - router_start_time,
                error_message=error_message
            )
            content_artifacts.append(workflow_content_artifact)

            # Initialize response_content with the result, taking final_response as priority.
            response_content = []
            final_response_content = OpenAITextMessageContentItem(
                value= final_response or initial_response or failure_response,
                agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            )
            response_content.append(final_response_content)

            # Process any generated files.
            for artifact in content_artifacts:
                if artifact.type == ContentArtifactTypeNames.FILE:
                    # if the file path has an image extension, add it to the response_content as an OpenAIImageFileMessageContentItem.
                    if any(artifact.metadata['original_file_name'].lower().endswith(ext) for ext in ['.png', '.jpg', '.jpeg', '.gif', '.bmp', '.tiff', '.ico', '.webp']):
                        response_content.append(OpenAIImageFileMessageContentItem(
                            text = artifact.metadata['original_file_name'],
                            file_id = artifact.filepath,
                            agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                        ))
                    else:
                        # if it's not an image, add it to the final_response_content as an annotation of type OpenAIFilePathMessageContentItem.
                        response_content[0].annotations.append(OpenAIFilePathMessageContentItem(
                            text = artifact.metadata['original_file_name'],
                            file_id = artifact.filepath,
                            agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                        ))

            retvalue = CompletionResponse(
                operation_id=operation_id,
                content = response_content,
                content_artifacts=content_artifacts,
                user_prompt=llm_prompt,
                full_prompt=workflow_main_prompt,
                completion_tokens=output_tokens,
                prompt_tokens=input_tokens,
                total_tokens=output_tokens + input_tokens,
                total_cost=0
            )
            return retvalue

    def __create_context_client(self):
        """
        Creates the context client for the workflow.
        This is used to access the Context API.
        """
        context_api_endpoint_configuration = APIEndpointConfiguration(
            **self.objects.get(CompletionRequestObjectKeys.CONTEXT_API_ENDPOINT_CONFIGURATION, None))
        if context_api_endpoint_configuration:
            self.context_api_client = HttpClientService(
                context_api_endpoint_configuration,
                self.user_identity,
                self.config
            )
        else:
            raise Exception("The Context API endpoint configuration is required to use the workflow.")

    def __get_tools_runnable_config(
            self,
            user_prompt:str,
            user_prompt_rewrite: Optional[str],
            file_history: List[FileHistoryItem],
            conversation_id: Optional[str] = None,
            objects: dict = None) -> RunnableConfig:
        """
        Returns the runnable config used in tool invocation.

        Parameters
        ----------
        user_prompt : str
            The user prompt message.
        user_prompt_rewrite : Optional[str]
            The user prompt rewrite message containing additional context to clarify the user's intent.
        file_history : List[FileHistoryItem]
            The file history.
        conversation_id : Optional[str]
            The conversation identifier for the workflow execution.
        objects : dict
            The exploded objects assigned from the agent.
        """
        if objects is None:
            objects = {}

        runnable_config = RunnableConfig(
            {
                'agent_name': objects['Agent.AgentName'],
                RunnableConfigKeys.ORIGINAL_USER_PROMPT: user_prompt,
                RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE: user_prompt_rewrite,
                RunnableConfigKeys.CONVERSATION_ID: conversation_id,
                RunnableConfigKeys.FILE_HISTORY: file_history
            }
        )
        for tool in self.tools:
            if tool.name in objects:
                # Add the tool properties to the runnable config if it exists in the objects.
                runnable_config[tool.name] = objects[tool.name]

        return runnable_config

    async def __get_message_list(
        self,
        llm_prompt: str,
        workflow_main_prompt: str,
        workflow_router_prompt: str,
        workflow_files_prompt: str,
        message_history: List[MessageHistoryItem],
        objects: dict,
        file_history: List[FileHistoryItem]
    ) -> List[BaseMessage]:
        """
        Returns the message history in the format required by the workflow.

        Parameters
        ----------
        message_history : List[MessageHistoryItem]
            The message history to be processed.
        objects : dict
            The exploded objects assigned from the agent. This is used to pass additional context to the workflow.
        """

        if objects is None:
            objects = {}

        if file_history is None:
            file_history = []

        # Convert message history to LangChain message types
        messages = []
        for message in message_history:
            # Convert MessageHistoryItem to appropriate LangChain message type
            if message.sender == "User":
                messages.append(HumanMessage(content=message.text))
            else:
                messages.append(AIMessage(content=message.text))

        # Enclose the file names in / to enforce the LLM to treat them as file names
        # regardless of their content (e.g., file names starting with 1., 2., etc. could be misinterpreted as numbered lists).
        conversation_files = [f'/{file_name}/' for file_name in objects.get(
            CompletionRequestObjectKeys.WORKFLOW_INVOCATION_CONVERSATION_FILES, [])]
        attached_files = [f'/{file_name}/' for file_name in objects.get(
            CompletionRequestObjectKeys.WORKFLOW_INVOCATION_ATTACHED_FILES, [])]
        context_files = []

        context_file_messages = []
        for context_file in [f for f in file_history if f.embed_content_in_request]:

            self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
            context_file_id = context_file.object_id.split('/')[-1]
            content_type = "application/octet-stream" if context_file.content_type.startswith(("image/", "audio/")) else None
            context_file_content = await self.context_api_client.get_async(
                endpoint = f"/instances/{self.instance_id}/files/{context_file_id}",
                content_type = content_type)

            context_file_message = \
                {
                    'type': 'image_url',
                    'image_url': {
                        'url': f'data:{context_file.content_type};base64,{base64.b64encode(context_file_content).decode("utf-8")}'
                    }
                } if context_file.content_type.startswith("image/") \
                else {
                    'type': 'media',
                    'data': base64.b64encode(context_file_content).decode("utf-8"),
                    'mime_type': context_file.content_type
                } if context_file.content_type.startswith("audio/") \
                else {
                    'type': 'text',
                    'text': f'\nFILE_CONTENT for /{context_file.original_file_name}/:\n{context_file_content}\nEND_FILE_CONTENT\n'
                }

            context_file_messages.append(
                context_file_message)
            context_files.append(f'/{context_file.original_file_name}/')

        context_message = HumanMessage(
            content=[{"type": "text", "text": llm_prompt}]+context_file_messages)

        files_prompt = workflow_files_prompt \
            .replace(f'{{{{{TemplateVariables.CONVERSATION_FILES}}}}}', '\n'.join(conversation_files)) \
            .replace(f'{{{{{TemplateVariables.ATTACHED_FILES}}}}}', '\n'.join(attached_files)) \
            .replace(f'{{{{{TemplateVariables.CONTEXT_FILES}}}}}', '\n'.join(context_files)) \
            if workflow_files_prompt else ''

        system_prompt = '/n'.join([
            workflow_main_prompt,
            workflow_router_prompt,
            files_prompt
        ])

        self.logger.debug('Workflow prompt: %s', system_prompt)

        return [
            SystemMessage(content=system_prompt),
            *messages,
            context_message
        ]

    def __get_message_for_final_response(
            self,
            intermediate_responses: List[str],
            llm_prompt: str,
            workflow_final_prompt: str
    ) -> HumanMessage:
        """
        Returns the final response message for the workflow.

        Parameters
        ----------
        tool_responses : List[str]
            The list of tool responses to be included in the final response.
        llm_prompt : str
            The original LLM prompt used to generate the response.
        """
        tool_results = '\n\n'.join(intermediate_responses)
        if workflow_final_prompt:
            final_response_content = workflow_final_prompt \
                .replace(f'{{{{{TemplateVariables.TOOL_RESULTS}}}}}', tool_results) \
                .replace(f'{{{{{TemplateVariables.PROMPT}}}}}', llm_prompt)
        else:
            final_response_content = f'Answer the QUESTION based on the provided CONTEXT. If there is no context, answer directly:\n\nCONTEXT\n{tool_results}\n\nQUESTION:\n{llm_prompt}'

        return \
            HumanMessage(content=final_response_content)

    def __extract_special_commands(self, llm_prompt: str) -> tuple[List[str], str]:
        """
        Extracts special commands and the main prompt from the LLM prompt.

        Parameters
        ----------
        llm_prompt : str
            The original LLM prompt.

        Returns
        -------
        Tuple[List[str], str]
            A tuple containing a list of special commands and the main prompt.
        """
        match = self.special_commands_pattern.match(llm_prompt)
        if match:
            commands = match.group(1).split(',') if match.group(1) else []
            main_prompt = llm_prompt[match.end():].strip()
            return commands, main_prompt
        return [], llm_prompt

    def __get_canonical_usage(
            self,
            llm_response: AIMessage
    ) -> Dict:
        """
        Returns the canonical usage dictionary from the LLM response.

        Parameters
        ----------
        llm_response : AIMessage
            The LLM response message containing usage metadata.
        """
        if llm_response.usage_metadata:
            return llm_response.usage_metadata

        if llm_response.response_metadata \
            and 'usage' in llm_response.response_metadata \
            and 'prompt_tokens' in llm_response.response_metadata['usage'] \
            and 'completion_tokens' in llm_response.response_metadata['usage']:
            return {
                'input_tokens': llm_response.response_metadata['usage']['prompt_tokens'],
                'output_tokens': llm_response.response_metadata['usage']['completion_tokens'],
                'total_tokens': llm_response.response_metadata['usage']['total_tokens']
            }

        return {
            'input_tokens': 0,
            'output_tokens': 0,
            'total_tokens': 0
        }
